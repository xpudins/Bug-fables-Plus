using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using static MainManager;
using static BattleControl;

namespace BFPlus.Extensions.EnemyAI
{
    public class FrostflyAI : AI
    {
        enum Attacks
        {
            StingerDive,
            Siphon,
            StingerSlash,
            Relay,
            UseItem
        }
        DamageOverride[] STINGER_ATTRIBUTES = new[] {
            (DamageOverride)NewDamageOverride.Magic,
            (DamageOverride)NewDamageOverride.Pierce1
        };
        const int STINGER_DIVE_DMG = 4;
        const int SIPHON_STING_DMG = 2;
        const int SIPHON_LEECH_DMG = 4;
        const int STINGER_SLASH_DMG = 2;
        const int STINGER_SLASH_DEFENSEDOWN = 3;
        int jumpAntId = -1;
        public override IEnumerator DoBattleAI(EntityControl entity, int actionid)
        {
            bool isFlying = battle.enemydata[actionid].position == BattlePosition.Flying;
            if (battle.enemydata[actionid].hitaction)
            {
                entity.Emoticon(2, 35);
                PlaySound("Wam");
                yield return new WaitUntil(() => entity.emoticoncooldown <= 0f);

                if (isFlying)
                    yield return DoStingerDive(entity, actionid);
                else
                    yield return DoStingerSlash(entity, actionid);
                yield break;
            }

            Dictionary<Attacks, int> attacks = new Dictionary<Attacks, int>()
            {
                { Attacks.Siphon, 60},
            };

            attacks.Add(isFlying ? Attacks.StingerDive : Attacks.StingerSlash, 40);

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
                case Attacks.Siphon:
                    yield return DoSiphon(entity, actionid);
                    break;
                case Attacks.StingerSlash:
                    yield return DoStingerSlash(entity, actionid);
                    break;
                case Attacks.StingerDive:
                    yield return DoStingerDive(entity, actionid);
                    break;

                case Attacks.Relay:
                    yield return battle.EnemyRelay(entity, jumpAntId);
                    break;

                case Attacks.UseItem:
                    yield return fightComponent.DoUseItem(entity, itemTargetId, fightComponent);
                    break;
            }

            entity.initialheight = 2;
            yield return battle.ChangePosition(actionid, Random.Range(0, 2) == 0 ? BattlePosition.Ground : BattlePosition.Flying);
        }

        IEnumerator DoStingerSlash(EntityControl entity, int actionid)
        {
            Vector3 basePos = entity.transform.position;

            Vector3 targetPos = instance.playerdata[battle.partypointer[0]].battleentity.transform.position + new Vector3(2, 0, -0.2f);
            entity.MoveTowards(targetPos, 2);
            yield return new WaitUntil(() => !entity.forcemove);

            entity.animstate = 107;
            PlaySound("BugWingFast", -1, 1.2f, 1f);
            yield return entity.ShakeSprite(new Vector3(0.2f, 0.2f), 30f);

            entity.animstate = 103;
            yield return null;
            yield return null;

            float a = 0;
            float b = 40f;

            entity.LockRigid(true);
            targetPos = new Vector3(-15, 1f, -0.2f);
            Vector3 startPos = entity.transform.position;
            bool[] damaged = new bool[instance.playerdata.Length];
            bool blocked = false;

            entity.trail = true;
            PlaySound("PingShot");
            PlaySound("FastWoosh");
            do
            {
                entity.transform.position = Vector3.Lerp(startPos, targetPos, a / b);
                a += TieFramerate(1f);

                if (CheckSlashDamage(ref damaged, entity, actionid))
                {
                    blocked = true;
                    break;
                }

                yield return null;
            } while (a < b);
            entity.trail = false;

            if (blocked)
            {
                entity.animstate = (int)Animations.Hurt;
                startPos = entity.transform.position;
                targetPos = entity.transform.position + new Vector3(2, 0, 0);
                yield return ArcMovement(entity.gameObject, startPos, targetPos, Vector3.zero, 3, 20, false);
            }
            else
            {
                entity.transform.position = new Vector3(15, 0, 0);
            }

            entity.animstate = 0;
            entity.LockRigid(false);
            entity.MoveTowards(basePos, 2);
            yield return new WaitUntil(() => !entity.forcemove);
        }

        bool CheckSlashDamage(ref bool[] damaged, EntityControl entity, int actionid)
        {
            for (int i = 0; i < instance.playerdata.Length; i++)
            {
                if (!damaged[i] && entity.transform.position.x < instance.playerdata[i].battleentity.transform.position.x && instance.playerdata[i].hp > 0)
                {
                    damaged[i] = true;
                    battle.DoDamage(actionid, i, STINGER_SLASH_DMG, null, STINGER_ATTRIBUTES, battle.commandsuccess);

                    if (battle.commandsuccess && !battle.IsStopped(instance.playerdata[i]))
                    {
                        BattleControl_Ext.Instance.DoConditionPierceStatEffect(ref instance.playerdata[i], STINGER_SLASH_DEFENSEDOWN, BattleCondition.DefenseDown, true, false);
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }

        ///105, 106
        IEnumerator DoSiphon(EntityControl entity, int actionid)
        {
            Vector3 basePos = entity.transform.position;

            battle.GetSingleTarget();
            instance.camoffset = new Vector3(1f, 2.3f, -6f);
            instance.camtarget = battle.playertargetentity.transform;
            if (battle.enemydata[actionid].position == BattlePosition.Flying)
                PlaySound("BugWing", -1, 1f, 1f, true);

            Vector3 targetPos = battle.playertargetentity.transform.position + new Vector3(2, 0, -0.2f);
            entity.MoveTowards(targetPos);
            yield return new WaitUntil(() => !entity.forcemove);

            float a;
            float b;
            float startHeight = entity.height;
            targetPos = battle.playertargetentity.transform.position + new Vector3(0.5f, 0f, -0.1f);
            Vector3 startPos = entity.transform.position;
            if (battle.enemydata[actionid].position == BattlePosition.Ground)
            {
                PlaySound("FlipNoise3", 1.2f, 1);
                a = 0f;
                b = 20f;
                do
                {
                    entity.transform.position = BeizierCurve3(startPos, targetPos, 3, a / b);
                    entity.height = Mathf.Lerp(startHeight, 1.15f + battle.playertargetentity.animid * 0.2f, a / b);
                    a += TieFramerate(1f);
                    yield return null;
                } while (a < b);

            }
            else
            {
                a = 0f;
                b = 15f;
                entity.animstate = 0;
                do
                {
                    entity.transform.position = Vector3.Lerp(startPos, targetPos, a / b);
                    entity.height = Mathf.Lerp(startHeight, 1.15f + battle.playertargetentity.animid * 0.2f, a / b);
                    a += TieFramerate(1f);
                    yield return null;
                }
                while (a < b);
            }

            entity.animstate = 105;
            StopSound("BugWing");
            Vector3 startScale = entity.rotater.transform.localScale;
            entity.bobrange = 0;
            entity.bobspeed = 0;

            SpriteBounce bounce = entity.rotater.gameObject.AddComponent<SpriteBounce>();
            bounce.MessageBounce();
            entity.animstate = 106;
            PlaySound("Damage2", 1.2f, 1);
            yield return EventControl.quartersec;

            battle.DoDamage(actionid, battle.playertargetID, SIPHON_STING_DMG, AttackProperty.Freeze, STINGER_ATTRIBUTES, battle.commandsuccess);

            yield return EventControl.tenthsec;
            battle.playertargetentity.StartCoroutine(battle.playertargetentity.ShakeSprite(new Vector3(0.1f, 0f, 0f), 1));
            PlaySound("Kiss", -1, 0.85f, 1);

            a = 0f;
            b = 60f;
            do
            {
                if (battle.blockcooldown <= 0f)
                {
                    battle.playertargetentity.animstate = 11;
                }
                else
                {
                    battle.playertargetentity.animstate = 24;
                }
                a += TieFramerate(1f);
                yield return null;
            }
            while (a < b);
            instance.camoffset += new Vector3(0.75f, 0.5f, -0.75f);

            battle.DoDamage(actionid, battle.playertargetID, SIPHON_LEECH_DMG, null, STINGER_ATTRIBUTES, battle.commandsuccess);
            if (!battle.commandsuccess)
                battle.Heal(ref battle.enemydata[actionid], battle.lastdamage);

            Object.Destroy(bounce);

            yield return null;
            entity.rotater.transform.localScale = startScale;
            entity.animstate = 0;
            startPos = entity.transform.position;

            startHeight = entity.height;
            a = 0;
            b = 20f;
            do
            {
                entity.height = Mathf.Lerp(startHeight, 2, a / b);
                entity.transform.position = Vector3.Lerp(startPos, startPos + new Vector3(1.5f, 0f), a / b);
                a += TieFramerate(1f);
                yield return null;
            }
            while (a < b);

            SetDefaultCamera();

            entity.bobrange = entity.startbf;
            entity.bobspeed = entity.startbs;
            entity.height = 2;
            battle.enemydata[actionid].position = BattlePosition.Flying;

            yield return BattleControl_Ext.LerpPosition(30f, entity.transform.position, basePos, entity.transform);
        }

        IEnumerator DoStingerDive(EntityControl entity, int actionid)
        {
            Vector3 basePos = entity.transform.position;
            battle.GetSingleTarget();
            instance.camoffset += new Vector3(-1.5f, 0f);
            instance.camtarget = entity.transform;

            PlaySound("PingUp");
            entity.animstate = 100;

            float startHeight = entity.height;
            float targetHeight = startHeight + 3f;
            Vector3 startPos = entity.transform.position;
            Vector3 targetPos = startPos + new Vector3(2f, 0f, 0);
            float a = 0;
            float b = 30;
            do
            {
                entity.transform.position = Vector3.Lerp(startPos, targetPos, a / b);
                entity.height = Mathf.Lerp(startHeight, targetHeight, a / b);
                a += TieFramerate(1f);
                yield return null;
            }
            while (a < b);

            entity.animstate = 101;
            a = 0f;
            startHeight = entity.height;

            entity.trail = true;
            entity.overrideflip = true;
            targetPos = battle.playertargetentity.transform.position + new Vector3(0.5f, 0f, -0.1f);

            instance.camtarget = battle.playertargetentity.transform;
            instance.camoffset += new Vector3(2f, 0f);

            PlaySound("PingShot");
            PlaySound("FastWoosh");
            startPos = entity.transform.position;
            b = 20;
            do
            {
                entity.height = Mathf.Lerp(startHeight, 1f, a / b);
                entity.transform.position = Vector3.Lerp(startPos, targetPos, a / b);
                a += TieFramerate(1f);
                yield return null;
            }
            while (a < b);

            entity.trail = false;

            entity.animstate = 102;
            battle.DoDamage(actionid, battle.playertargetID, STINGER_DIVE_DMG, AttackProperty.Freeze, STINGER_ATTRIBUTES, battle.commandsuccess);
            yield return new WaitForSeconds(0.2f);
            startHeight = entity.height;
            a = 0f;
            b = 20f;

            entity.overrideflip = false;
            entity.sprite.transform.localEulerAngles = Vector3.zero;
            startPos = entity.transform.position;
            do
            {
                entity.height = Mathf.Lerp(startHeight, entity.initialheight, a / b);
                entity.transform.position = Vector3.Lerp(startPos, startPos + new Vector3(1.5f, 0f), a / b);
                a += TieFramerate(1f);
                yield return null;
            }
            while (a < b);

            entity.animstate = 0;
            SetDefaultCamera();
            yield return BattleControl_Ext.LerpPosition(30f, entity.transform.position, basePos, entity.transform);
        }
    }
}
