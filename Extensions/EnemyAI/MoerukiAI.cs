using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BFPlus.Extensions.EnemyAI
{
    class MoerukiAI : AI
    {
        enum Attacks
        {
            Inferno,
            FireBarrier,
            Relay,
            Charge,
            UseItem
        }
        BattleControl battle = null;
        MoerukiAnim anim = null;
        const int INFERNO_DAMAGE = 5;
        const int INFERNO_BURN_TURNS = 3;
        const int FIREBARRIER_DAMAGE = 6;
        const int FIREBARRIER_DELAY = 2;
        int jumpAntId = -1;
        public override IEnumerator DoBattleAI(EntityControl entity, int actionid)
        {
            anim = entity.GetComponent<MoerukiAnim>();
            battle = MainManager.battle;
            float lerpSpeed = anim.lerpSpeed;
            float targetValue = anim.targetValue;
            yield return null;

            anim.noShift = true;

            if (battle.enemydata[actionid].isdefending)
            {
                MainManager.PlaySound("TextBack2");
                battle.enemydata[actionid].isdefending = false;
                battle.enemydata[actionid].defenseonhit = 0;
                yield return DoInferno(entity, actionid);
                BattleControl_Ext.Instance.startState = 0;
            }
            else
            {
                Dictionary<Attacks, int> attacks = new Dictionary<Attacks, int>()
                {
                    { Attacks.Inferno, 65},
                };
                if (MainManager.HasCondition((MainManager.BattleCondition)NewCondition.Vitiation, battle.enemydata[actionid]) < 1)
                {
                    attacks.Add(Attacks.FireBarrier, 35);
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
                    case Attacks.Inferno:
                        yield return DoInferno(entity, actionid);
                        break;
                    case Attacks.FireBarrier:
                        yield return DoFireBarrier(entity, actionid);
                        break;

                    case Attacks.Relay:
                        yield return battle.EnemyRelay(entity, jumpAntId);
                        break;

                    case Attacks.UseItem:
                        yield return fightComponent.DoUseItem(entity, itemTargetId, fightComponent);
                        break;
                }
            }

            anim.noShift = false;
            anim.lerpSpeed = lerpSpeed;
            anim.targetValue = targetValue;
        }

        IEnumerator DoInferno(EntityControl entity, int actionid)
        {
            entity.overrideflip = true;
            battle.GetSingleTarget();
            MainManager.instance.camoffset = new Vector3(1f, 2.3f, -6f);
            MainManager.instance.camtarget = MainManager.instance.playerdata[battle.playertargetID].battleentity.transform;
            entity.MoveTowards(battle.playertargetentity.transform.position + new Vector3(1f, 0f, -0.1f), 2f, 100, 101);
            while (entity.forcemove)
            {
                yield return null;
            }
            anim.targetValue += 5;
            entity.animstate = 101;
            MainManager.PlaySound("Charge17");
            battle.StartCoroutine(entity.ShakeSprite(new Vector3(0.1f, 0f), 60f));
            yield return EventControl.sec;
            entity.animstate = 103;
            battle.DoDamage(actionid, battle.playertargetID, INFERNO_DAMAGE, null, battle.commandsuccess);
            MainManager.PlayParticle("Fire", entity.transform.position + new Vector3(0f, 0.5f, -0.1f));
            MainManager.PlaySound("WaspKingMFireBall2");
            BattleControl_Ext.Instance.DoConditionPierce(ref MainManager.instance.playerdata[battle.playertargetID], INFERNO_BURN_TURNS, MainManager.BattleCondition.Fire);

            anim.targetValue -= 5;
            anim.lerpSpeed = 10;
            yield return new WaitForSeconds(0.7f);
            BattleControl.SetDefaultCamera();
        }

        IEnumerator DoFireBarrier(EntityControl entity, int actionid)
        {
            anim.targetValue += 5;
            entity.animstate = 101;
            MainManager.PlaySound("Charge17");
            battle.StartCoroutine(entity.ShakeSprite(new Vector3(0.1f, 0f), 30f));
            yield return EventControl.halfsec;
            entity.animstate = 103;
            anim.targetValue -= 5;
            battle.GetSingleTarget();

            Transform fireball = (UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Particles/Fireball"), entity.transform.position + new Vector3(-0.2f, 0.8f, -0.2f), Quaternion.identity, battle.battlemap.transform) as GameObject).transform;
            Vector3 startPosition = fireball.position;
            float a = 0f;
            float b = 30f;
            do
            {
                fireball.position = MainManager.SmoothLerp(startPosition, new Vector3(entity.transform.position.x - 1f, 10f), a / b);
                a += MainManager.TieFramerate(1f);
                yield return null;
            }
            while (a < b + 1f);
            fireball.position = new Vector3(0f, 20f);

            entity.overrideanim = true;
            entity.FlipAngle(true);
            MainManager.PlaySound("TextBack");
            battle.enemydata[actionid].isdefending = true;
            battle.enemydata[actionid].defenseonhit = 2;
            BattleControl_Ext.Instance.startState = (int)MainManager.Animations.Block;
            entity.animstate = 104;
            MainManager.PlaySound("Shield");
            yield return EventControl.tenthsec;

            int VITIATION = 20;
            BattleControl_Ext.Instance.AddEnemyBuff(actionid, (MainManager.BattleCondition)NewCondition.Vitiation, VITIATION, null, -1);
            if (jumpAntId != -1 && !battle.IsStopped(battle.enemydata[jumpAntId]))
            {
                BattleControl_Ext.Instance.AddEnemyBuff(jumpAntId, (MainManager.BattleCondition)NewCondition.Vitiation, VITIATION, null, -1);
            }

            yield return EventControl.halfsec;

            battle.AddDelayedProjectile(fireball.gameObject, battle.playertargetID, FIREBARRIER_DAMAGE, FIREBARRIER_DELAY, 0, BattleControl.AttackProperty.Fire, 55f, battle.enemydata[BattleControl_Ext.actionID], "WaspKingMFireBall2", "Fire", "Fall2");
        }
    }
}
