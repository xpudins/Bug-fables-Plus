using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MainManager;

namespace BFPlus.Extensions.EnemyAI
{
    public class SplotchSpiderAI : AI
    {
        enum Attacks
        {
            InkHop,
            SplatterJump,
            ChargeUp,
            StamperStompers,
            Relay,
            UseItem
        }
        BattleControl battle = null;
        const int INK_HOP_DAMAGE = 4;
        const int SPLATTER_JUMP_DAMAGE = 5;
        const int STOMPER_DAMAGE = 3;
        const int INK_BUBBLE_TURNS = 2;
        const int INK_TURNS = 2;
        int hits = 10;
        int jumpAntId = -1;
        public override IEnumerator DoBattleAI(EntityControl entity, int actionid)
        {
            battle = MainManager.battle;
            battle.SetData(actionid, 3);

            if (battle.enemydata[actionid].data[0] == 1)
            {
                battle.enemydata[actionid].data[0] = 0;
                battle.enemydata[actionid].data[1] = 2;
                yield return SplatterJump(entity, actionid);
                yield break;
            }

            Dictionary<Attacks, int> attacks = new Dictionary<Attacks, int>()
            {
                { Attacks.InkHop, 60}
            };

            if (battle.enemydata[actionid].data[1] <= 0)
                attacks.Add(Attacks.ChargeUp, 40);

            int jumpingFriend = battle.EnemyInField(new int[] { (int)MainManager.Enemies.JumpingSpider, (int)NewEnemies.JumpAnt });
            if (jumpingFriend != -1 && !battle.IsStopped(battle.enemydata[jumpingFriend]) && battle.enemydata[actionid].data[2] == 0)
            {
                attacks.Add(Attacks.StamperStompers, 60);
            }

            jumpAntId = battle.EnemyInField((int)NewEnemies.JumpAnt);
            JumpAntFight fightComponent = BattleControl_Ext.Instance.jumpAntFightComp;
            int itemTargetId = -1;

            if (jumpAntId != -1)
            {
                itemTargetId = fightComponent.GetItemTargetId(jumpAntId);
                fightComponent.AddPartnerAttacks(attacks, actionid, jumpAntId, itemTargetId, Attacks.Relay, Attacks.UseItem);
            }

            Attacks attack = MainManager_Ext.GetWeightedResult(attacks);
            switch (attack)
            {
                case Attacks.InkHop:
                    yield return InkHop(entity, actionid);
                    break;

                case Attacks.ChargeUp:
                    battle.enemydata[actionid].data[0] = 1;
                    yield return ChargeUp(entity, actionid);
                    break;

                case Attacks.StamperStompers:
                    battle.enemydata[actionid].data[2] = 1;
                    yield return StamperStompers(entity, jumpingFriend, actionid);
                    battle.EndEnemyTurn(jumpingFriend);
                    break;

                case Attacks.Relay:
                    yield return battle.EnemyRelay(entity, jumpAntId);
                    break;

                case Attacks.UseItem:
                    yield return fightComponent.DoUseItem(entity, itemTargetId, fightComponent);
                    break;
            }

            battle.enemydata[actionid].data[1]--;
        }

        IEnumerator StamperStompers(EntityControl entity, int friendId, int actionid)
        {
            EntityControl friend = battle.enemydata[friendId].battleentity;
            Vector3 friendBasePos = friend.transform.position;

            for (int i = 0; i < 2; i++)
            {
                entity.PlaySound("Jump", 1, 0.9f);
                entity.Jump(10);
                yield return EventControl.halfsec;

                friend.PlaySound("Jump", 1, 0.9f);
                friend.Jump(10);

                yield return new WaitUntil(() => entity.onground && friend.onground);
            }

            EntityControl attackingEntity;
            int attackingId;
            BattleControl.AttackProperty? property = null;

            entity.overrideanim = true;
            entity.overrridejump = true;
            entity.LockRigid(true);

            friend.overrideanim = true;
            friend.overrridejump = true;
            friend.LockRigid(true);
            for (int i = 0; i < hits; i++)
            {
                if (MainManager.GetAlivePlayerAmmount() == 0)
                    break;
                if (i % 2 == 0)
                {
                    attackingEntity = entity;
                    attackingId = actionid;
                    property = BattleControl.AttackProperty.InkOnBlock;
                }
                else
                {
                    attackingEntity = friend;
                    attackingId = friendId;
                }
                Vector3 startPos = attackingEntity.transform.position;

                battle.GetSingleTarget();
                yield return null;

                Vector3 targetPos = battle.playertargetentity.transform.position + new Vector3(-0.25f, 1.5f, -0.1f);

                battle.StartCoroutine(DoStomperJump(attackingEntity, startPos, targetPos, 30, attackingId, STOMPER_DAMAGE, battle.playertargetID, property));
                yield return EventControl.quartersec;
            }

            for (int i = 0; i < 2; i++)
            {
                battle.StartCoroutine(SplotchReturn(i == 0 ? entity : friend, i == 0 ? Vector3.zero : friendBasePos));
                yield return EventControl.quartersec;
            }
            yield return EventControl.sec;
        }

        IEnumerator DoStomperJump(EntityControl entity, Vector3 startPos, Vector3 targetPos, float endTime, int actionid, int damage, int targetId, BattleControl.AttackProperty? property = null)
        {
            yield return SplotchHop(entity, startPos, targetPos, 30);
            if (MainManager.instance.playerdata[targetId].hp > 0)
            {
                battle.DoDamage(actionid, targetId, damage, property, battle.commandsuccess);

                if (!MainManager_Ext.IsNewEnemy(entity, NewEnemies.SplotchSpider))
                {
                    BattleControl_Ext.Instance.TryDizzy(null, ref MainManager.instance.playerdata[battle.playertargetID], 1);
                }

                if (battle.commandsuccess)
                {
                    bool superBlocked = battle.GetSuperBlock(0) || battle.superblockedthisframe > 0f;
                    hits--;

                    if (superBlocked)
                        hits--;
                }
            }
        }

        IEnumerator InkHop(EntityControl entity, int actionid)
        {
            bool hardmode = battle.HardMode();
            battle.GetSingleTarget();
            Vector3 startPos = battle.playertargetentity.transform.position + new Vector3(3f, 0f, -0.1f);

            battle.CameraFocusTarget();
            entity.MoveTowards(startPos, 1f, 23, 0);
            while (entity.forcemove || !entity.onground)
            {
                yield return null;
            }
            entity.overrideanim = true;
            entity.overrridejump = true;
            entity.LockRigid(true);

            Vector3 targetPos;

            startPos = entity.transform.position;
            targetPos = battle.playertargetentity.transform.position + new Vector3(-0.25f, 1.5f, -0.1f);

            yield return SplotchHop(entity, startPos, targetPos, hardmode ? 35 : 45);
            battle.DoDamage(actionid, battle.playertargetID, INK_HOP_DAMAGE, null, battle.commandsuccess);
            InkBubble(ref MainManager.instance.playerdata[battle.playertargetID]);

            yield return EventControl.tenthsec;

            entity.PlaySound("Jump", 1, 0.9f);
            entity.animstate = (int)MainManager.Animations.Jump;
            Vector3 mid = entity.transform.position + Vector3.up * 5;
            startPos = entity.transform.position;

            yield return SplotchHop(entity, startPos, targetPos, 30, mid);
            battle.DoDamage(actionid, battle.playertargetID, INK_HOP_DAMAGE, BattleControl.AttackProperty.Ink, battle.commandsuccess);
            InkBubble(ref MainManager.instance.playerdata[battle.playertargetID]);

            entity.UpdateAnimSpecific();
            MainManager.PlaySound("Turn2");
            yield return SplotchReturn(entity, Vector3.zero);
        }

        IEnumerator SplatterJump(EntityControl entity, int actionid)
        {
            battle.MiddleCamera(entity.transform.position, battle.partymiddle);
            Vector3 startScale = entity.rotater.localScale;
            Vector3 targetScale = MainManager.MultiplyVector(startScale, new Vector3(1.15f, 0.65f, 1f));

            bool hardmode = battle.HardMode();

            battle.StartCoroutine(entity.ShakeSprite(0.15f, 45f));
            entity.overrideanim = true;
            entity.overrridejump = true;
            entity.animstate = 1;
            MainManager.PlaySound("BMCharge", -1, 1.2f, 1f);

            float a = 0f;
            float b = 45f;
            do
            {
                entity.rotater.localScale = Vector3.Lerp(startScale, targetScale, a / b);
                a += MainManager.framestep;
                yield return null;
            }
            while (a < b);

            entity.animstate = 2;
            entity.spin = new Vector3(0f, 30f);

            a = 0f;
            b = hardmode ? 65 : 80;

            entity.rotater.localScale = startScale;
            Vector3 startPos = entity.transform.position;
            Vector3 targetPos = battle.partymiddle + new Vector3(0.25f, 2f, -0.1f);
            MainManager.PlaySound("Turn", -1, 0.75f, 1f);
            do
            {
                entity.transform.position = MainManager.BeizierCurve3(startPos, targetPos, 15f, a / b);
                if (a / b > 0.5f)
                {
                    entity.animstate = 3;
                }
                a += MainManager.framestep;
                yield return null;
            }
            while (a < b + 1f);
            entity.spin = Vector3.zero;
            MainManager.ShakeScreen(0.2f, 0.75f, true);
            battle.PartyDamage(actionid, SPLATTER_JUMP_DAMAGE, null, battle.commandsuccess);

            for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
            {
                if (MainManager.instance.playerdata[i].hp > 0)
                    InkBubble(ref MainManager.instance.playerdata[i]);
            }
            MainManager.PlaySound("BigHit");
            MainManager.PlayParticle("impactsmoke", battle.partymiddle).transform.localScale = Vector3.one * 0.65f;

            yield return SplotchReturn(entity, Vector3.zero);
        }

        IEnumerator ChargeUp(EntityControl entity, int actionId)
        {
            MainManager.PlaySound("Charge10", 0.8f, 1);
            entity.spin = new Vector3(0f, 30f);
            yield return EventControl.sec;
            entity.spin = Vector3.zero;

            BattleControl_Ext.Instance.SetSturdy(ref battle.enemydata[actionId], 2, actionId);
            MainManager.PlaySound("StatUp");
            battle.enemydata[actionId].charge = Mathf.Clamp(battle.enemydata[actionId].charge + 2, 0, 3);
            battle.StartCoroutine(battle.StatEffect(battle.enemydata[actionId].battleentity, 4));
            battle.dontusecharge = true;

        }

        IEnumerator SplotchReturn(EntityControl entity, Vector3 targetPos)
        {
            Vector3 startPos = entity.transform.position;
            float a = 0f;
            float b = 40f;
            do
            {
                entity.transform.position = MainManager.BeizierCurve3(startPos, targetPos, 5f, a / b);
                a += MainManager.framestep;
                yield return null;
            }
            while (a < b + 1f);
            entity.transform.position = targetPos;
            entity.overrideanim = false;
            entity.overrridejump = false;
            entity.LockRigid(false);
            entity.animstate = 0;
            yield return EventControl.quartersec;
        }

        IEnumerator SplotchHop(EntityControl entity, Vector3 startPos, Vector3 targetPos, float endTime, Vector3? mid = null)
        {
            float a = 0f;
            MainManager.PlaySound("Turn");
            do
            {
                if (mid == null)
                    entity.transform.position = MainManager.BeizierCurve3(startPos, targetPos, 5f, a / endTime);
                else
                    entity.transform.position = MainManager.BeizierCurve3(startPos, targetPos, mid.Value, a / endTime);

                if (a / endTime > 0.5f)
                    entity.animstate = 3;
                else
                    entity.animstate = 2;
                a += MainManager.TieFramerate(1f);
                yield return null;
            }
            while (a < endTime);
        }

        void InkBubble(ref MainManager.BattleData target)
        {
            int turns = BattleControl_Ext.Instance.GetConditionTurnPierce(target, INK_BUBBLE_TURNS);

            Vector3 particlePos = target.battleentity.transform.position + Vector3.up;
            if (turns > 0)
            {
                //ink bubble
                BattleControl_Ext.Instance.
                    ApplyStatus((MainManager.BattleCondition)NewCondition.Paintball, ref target, turns, "Shield", 0.8f, 1, "InkGet", particlePos, Vector3.one);
            }
            else
            {
                //ink
                BattleControl_Ext.Instance.
                    ApplyStatus(BattleCondition.Inked, ref target, INK_TURNS, "WaterSplash2", 0.8f, 1, "InkGet", particlePos, Vector3.one);
            }
        }
    }
}
