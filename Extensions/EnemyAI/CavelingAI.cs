using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MainManager;

namespace BFPlus.Extensions.EnemyAI
{
    public class CavelingAI : AI
    {
        enum Attacks
        {
            HeadSpinner,
            SpinBash,
            Relay = 10,
            Charge,
            UseItem
        }
        BattleControl battle = null;
        const int HEADSPINNER_DMG = 4;
        const int HEADSPINNER_DIZZY = 3;
        const int SPIN_BASH_DMG = 4;
        const int SPIN_BASH_DIZZY = 3;
        public override IEnumerator DoBattleAI(EntityControl entity, int actionid)
        {
            battle = MainManager.battle;
            battle.SetData(actionid, 1);
            BattleControl.BattlePosition battlePos = battle.enemydata[actionid].position;

            if (battlePos != BattleControl.BattlePosition.Ground)
            {
                switch (battlePos)
                {
                    case BattleControl.BattlePosition.Underground:
                        PlaySound("DigPop2");
                        break;
                    case BattleControl.BattlePosition.Flying:
                        entity.StartCoroutine(BattleControl_Ext.LerpScale(15, entity.extras[0].transform.localScale, Vector3.zero, entity.extras[0].transform));
                        break;
                }
                yield return battle.ChangePosition(actionid, BattleControl.BattlePosition.Ground);
            }

            Dictionary<Attacks, int> attacks = new Dictionary<Attacks, int>()
            {
                { Attacks.HeadSpinner, 50},
                { Attacks.SpinBash, 50},
            };

            int jumpAntId = battle.EnemyInField((int)NewEnemies.JumpAnt);
            JumpAntFight fightComponent = BattleControl_Ext.Instance.jumpAntFightComp;
            int itemTargetId = -1;

            if (jumpAntId != -1)
            {
                itemTargetId = fightComponent.GetItemTargetId(jumpAntId);
                fightComponent.AddPartnerAttacks(attacks, actionid, jumpAntId, itemTargetId, Attacks.Relay, Attacks.UseItem);
                if (battle.enemydata[actionid].charge < 3)
                    attacks.Add(Attacks.Charge, 10);
            }

            Attacks attack = MainManager_Ext.GetWeightedResult(attacks);

            int bonusDamage = battlePos == BattleControl.BattlePosition.Flying ? 1 : 0;

            switch (attack)
            {
                case Attacks.HeadSpinner:
                    yield return DoHeadSpinner(entity, actionid, bonusDamage);
                    break;
                case Attacks.SpinBash:
                    yield return DoSpinBash(entity, actionid, bonusDamage);
                    break;

                case Attacks.Charge:
                    yield return DoCharge(entity, actionid);
                    break;

                case Attacks.Relay:
                    yield return battle.EnemyRelay(entity, jumpAntId);
                    break;

                case Attacks.UseItem:
                    yield return fightComponent.DoUseItem(entity, itemTargetId, fightComponent);
                    break;
            }

            if (battlePos != BattleControl.BattlePosition.Ground)
            {
                if (battlePos == BattleControl.BattlePosition.Underground)
                {
                    yield return battle.Dig(actionid, 1);
                    yield break;
                }

                if (battlePos == BattleControl.BattlePosition.Flying)
                {
                    entity.StartCoroutine(BattleControl_Ext.LerpScale(15, Vector3.zero, Vector3.one, entity.extras[0].transform));
                }

                entity.initialheight = 2;
                yield return battle.ChangePosition(actionid, battlePos);
            }
        }

        IEnumerator DoCharge(EntityControl entity, int actionid)
        {
            battle.dontusecharge = true;
            MainManager.PlaySound("Charge10", 0.8f, 1);
            entity.animstate = 103;
            yield return EventControl.halfsec;
            entity.animstate = 104;
            MainManager.PlaySound("StatUp", -1, 1.25f, 1f);
            battle.StartCoroutine(battle.StatEffect(battle.enemydata[actionid].battleentity, 4));
            battle.enemydata[actionid].charge = 3;
            yield return EventControl.halfsec;

            entity.animstate = 101;
            yield return EventControl.halfsec;

            entity.animstate = (int)MainManager.Animations.Idle;
        }

        IEnumerator DoSpinBash(EntityControl entity, int actionid, int bonusDamage)
        {
            battle.GetSingleTarget();
            yield return null;
            battle.CameraFocusTarget();

            int fakeouts = GetFakeoutAmount();

            Vector3 basePos = battle.playertargetentity.transform.position + new Vector3(fakeouts > 0 ? 1f : 1.5f, 0, -0.1f);
            entity.MoveTowards(basePos, 2);
            yield return new WaitUntil(() => !entity.forcemove);

            entity.LockRigid(true);
            if (fakeouts > 0)
            {
                entity.animstate = (int)Animations.Jump;
                float pitch = Mathf.Max(0.6f, 1f - 0.15f * (fakeouts - 1));
                for (int i = fakeouts; i > 0; i--)
                {
                    PlaySound("Jump", pitch, 1);
                    entity.animstate = (fakeouts % 2 == 0) ? 102 : 101;
                    fakeouts--;
                    pitch += 0.15f;
                    yield return ArcMovement(entity.gameObject, entity.transform.position, entity.transform.position + Vector3.right, Vector3.zero, 2.5f, 20, false);

                }
            }

            entity.animstate = 103;
            for (int i = 0; i < 3; i++)
            {
                PlaySound("ItemBounce1", 1f + i * 0.25f, 2f + i * 0.4f);
                yield return new WaitForSeconds(0.125f);
            }

            entity.transform.position = new Vector3(entity.transform.position.x, 0, entity.transform.position.z);
            PlaySound("Spin", -1, 1.2f, 1, true);
            entity.animstate = 104;
            basePos = battle.playertargetentity.transform.position + new Vector3(0f, 0, -0.1f);
            yield return BattleControl_Ext.LerpPosition(10, entity.transform.position, basePos, entity.transform);

            battle.DoDamage(actionid, battle.playertargetID, SPIN_BASH_DMG + bonusDamage, null, battle.commandsuccess);
            int dizzyTurn = battle.HardMode() ? SPIN_BASH_DIZZY + 1 : SPIN_BASH_DIZZY;
            dizzyTurn = BattleControl_Ext.Instance.GetConditionTurnPierce(MainManager.instance.playerdata[battle.playertargetID], dizzyTurn);
            if (dizzyTurn > 0)
            {
                BattleControl_Ext.Instance.TryDizzy(null, ref MainManager.instance.playerdata[battle.playertargetID], dizzyTurn);
            }

            bool superBlocked = battle.GetSuperBlock(0) || battle.superblockedthisframe > 0f;
            SpriteRenderer item = StealItem(entity, actionid, superBlocked);

            BattleControl.SetDefaultCamera();
            yield return ArcMovement(entity.gameObject, entity.transform.position, Vector3.zero, Vector3.zero, 5, 30, false);

            StopSound("Spin");
            entity.animstate = 102;

            if (item != null)
            {
                PlaySound("ItemGet");
                yield return EventControl.quartersec;
                Object.Destroy(item.gameObject);
            }
            yield return EventControl.quartersec;
            entity.LockRigid(false);
            entity.animstate = 0;
        }

        IEnumerator DoHeadSpinner(EntityControl entity, int actionid, int bonusDamage)
        {
            battle.GetSingleTarget();
            yield return null;

            battle.CameraFocusTarget();

            Vector3 basePos = battle.playertargetentity.transform.position + new Vector3(1.5f, 0, -0.1f);
            entity.MoveTowards(basePos, 2);
            yield return new WaitUntil(() => !entity.forcemove);

            PlaySound("Jump");
            entity.LockRigid(true);
            entity.animstate = (int)Animations.Jump;
            Vector3 targetPos = battle.playertargetentity.transform.position + new Vector3(-0.1f, 1.5f, -0.1f);
            yield return ArcMovement(entity.gameObject, entity.transform.position, targetPos, Vector3.zero, 5, 30, false);

            Vector3 startPos = entity.transform.position;
            int fakeouts = GetFakeoutAmount();

            if (fakeouts > 0)
            {
                entity.LockRigid(false);
                float pitch = Mathf.Max(0.6f, 1f - 0.15f * (fakeouts - 1));
                while (fakeouts > 0)
                {
                    PlaySound("Jump", pitch, 1);
                    entity.Jump();
                    entity.animstate = (fakeouts % 2 == 0) ? 102 : 101;
                    fakeouts--;
                    pitch += 0.15f;
                    yield return EventControl.tenthsec;
                    yield return new WaitUntil(() => entity.transform.position.y <= startPos.y);
                }
                entity.LockRigid(true);
                entity.transform.position = startPos;
            }

            entity.animstate = 103;
            for (int i = 0; i < 3; i++)
            {
                PlaySound("ItemBounce1", 1f + i * 0.25f, 2f + i * 0.4f);
                yield return new WaitForSeconds(0.125f);
            }

            PlaySound("Spin", -1, 1.2f, 1, true);
            entity.animstate = 104;
            yield return EventControl.tenthsec;

            battle.DoDamage(actionid, battle.playertargetID, HEADSPINNER_DMG + bonusDamage, null, battle.commandsuccess);

            int dizzyTurn = battle.HardMode() ? HEADSPINNER_DIZZY + 1 : HEADSPINNER_DIZZY;
            dizzyTurn = BattleControl_Ext.Instance.GetConditionTurnPierce(MainManager.instance.playerdata[battle.playertargetID], dizzyTurn);

            if (dizzyTurn > 0)
            {
                BattleControl_Ext.Instance.TryDizzy(null, ref MainManager.instance.playerdata[battle.playertargetID], dizzyTurn);
            }

            bool superBlocked = battle.GetSuperBlock(0) || battle.superblockedthisframe > 0f;
            yield return new WaitForSeconds(0.2f);

            SpriteRenderer item = StealItem(entity, actionid, superBlocked);

            BattleControl.SetDefaultCamera();
            yield return ArcMovement(entity.gameObject, entity.transform.position, Vector3.zero, Vector3.zero, 5, 30, false);

            StopSound("Spin");
            entity.LockRigid(false);
            entity.animstate = 102;

            if (item != null)
            {
                PlaySound("ItemGet");
                yield return EventControl.quartersec;
                Object.Destroy(item.gameObject);
            }
            yield return EventControl.quartersec;
            entity.animstate = 0;
        }

        int GetFakeoutAmount()
        {
            int fakeouts = Random.value < 0.3f ? 0 : 2;
            if (Random.value < 0.5f)
                return 1;

            return fakeouts;
        }


        SpriteRenderer StealItem(EntityControl entity, int actionid, bool superBlocked)
        {
            int stolenItem;
            SpriteRenderer item = null;
            var entityExt = Entity_Ext.GetEntity_Ext(entity);

            if (!BadgeIsEquipped((int)BadgeTypes.SecurePouch) && entityExt.itemId == -1 
                && instance.items[0].Count() > 0 && !superBlocked)
            {
                stolenItem = instance.items[0][Random.Range(0, instance.items[0].Count())];
                instance.items[0].Remove(stolenItem);
                item = NewSpriteObject(new Vector3(0f, 2f, -0.1f), entity.transform, itemsprites[0, stolenItem]);

                if (battle.caller != null)
                {
                    var npcControlExt = NPCControl_Ext.GetNPCControl_Ext(battle.caller);
                    npcControlExt.usedItem[actionid] = false;
                    npcControlExt.items[actionid] = stolenItem;
                }
                entityExt.CreateItem(stolenItem);
            }
            return item;
        }
    }
}
