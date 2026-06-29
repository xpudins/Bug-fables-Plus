using BFPlus.Extensions.Stylish;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MainManager;

namespace BFPlus.Extensions.EnemyAI
{
    public class JumpAntAI : AI
    {
        enum Attacks
        {
            ShrinkStomp,
            InkHammer,
            SpringJump,
            PowerJump,
            ChillOutHammer,
            FireDrive,
            HammerThrow,
            Defend,
            TornadoJump,
            UseItem,
            PointSwap
        }
        BattleControl battle = null;
        const int SHRINK_STOMP_DMG = 4;
        const int INK_HAMMER_DMG = 6;
        const int INK_HAMMER_TURNS = 2;
        const int SPRING_JUMP_DMG = 6;
        const int POWER_JUMP_DMG = 7;
        const int POWER_JUMP_SLEEP_TURNS = 3;
        const int CHILL_OUT_DMG = 3;
        const int FIRE_DRIVE_DMG = 6;
        const int FIRE_DRIVE_BURN = 3;
        const int HAMMER_THROW_DMG = 9;
        const int HAMMER_THROW_DEFDOWN = 3;
        const int DEFEND_STURDY_TURNS = 2;
        const int DEFEND_DEFENSE = 2;
        const int TORNADO_JUMP_DMG = 4;
        const int TORNADO_JUMP_DIZZY = 3;
        public override IEnumerator DoBattleAI(EntityControl entity, int actionid)
        {
            battle = MainManager.battle;
            battle.SetData(actionid, 4);

            float hpPercent = battle.HPPercent(battle.enemydata[actionid]);

            JumpAntFight fightComponent = BattleControl_Ext.Instance.jumpAntFightComp;

            Dictionary<Attacks, int> attacks = new Dictionary<Attacks, int>()
            {
                { Attacks.ShrinkStomp, 10},
                { Attacks.InkHammer, 10},
                { Attacks.SpringJump, 10},
                { Attacks.PowerJump, 10},
                { Attacks.ChillOutHammer, 10},
                { Attacks.FireDrive, 10},
                { Attacks.HammerThrow, 10},
                { Attacks.TornadoJump, 10}
            };

            battle.enemydata[actionid].isdefending = false;
            battle.enemydata[actionid].defenseonhit = 0;
            entity.animstate = (int)MainManager.Animations.BattleIdle;
            entity.basestate = entity.animstate;
            BattleControl_Ext.Instance.startState = entity.animstate;

            entity.overrideanim = true;
            entity.overrridejump = true;

            if (battle.enemydata[actionid].data[0] < 0 && hpPercent <= 0.7f
                && MainManager.HasCondition((MainManager.BattleCondition)NewCondition.Huge, battle.enemydata[actionid]) == -1)
            {
                attacks.Add(Attacks.Defend, 10);
            }
            else
            {
                battle.enemydata[actionid].data[0]--;
            }

            List<int> possiblePointSwap = new List<int>();

            if (fightComponent != null)
            {
                if (fightComponent.HasPartnerAlive() && battle.enemydata[actionid].data[2] <= 0 && UnityEngine.Random.Range(0, 10) < 2)
                {
                    battle.enemydata[actionid].data[2] = 2;
                    yield return fightComponent.SwapPartner(false, false);
                }
                else
                {
                    battle.enemydata[actionid].data[2]--;
                }

                if (fightComponent.HasItem((int)NewItem.PointSwap))
                {
                    for (int i = 0; i < battle.enemydata.Length; i++)
                    {
                        if (battle.HPPercent(battle.enemydata[i]) < 0.5f && battle.enemydata[i].hp < MainManager.instance.tp)
                        {
                            possiblePointSwap.Add(i);
                        }
                    }

                    if (possiblePointSwap.Count > 0)
                    {
                        attacks.Add(Attacks.PointSwap, 10);
                    }
                }

                if (battle.enemydata[actionid].data[1] == 0 && hpPercent <= 0.8f && fightComponent.HasItem())
                {
                    attacks.Add(Attacks.UseItem, 10);
                }
                else
                {
                    battle.enemydata[actionid].data[1] = 0;
                }
            }


            Attacks attack = MainManager_Ext.GetWeightedResult(attacks);

            switch (attack)
            {
                case Attacks.ShrinkStomp:
                    yield return DoShrinkStomp(entity, actionid);
                    break;

                case Attacks.PointSwap:
                    fightComponent.RemovePointSwap();
                    int targetId = actionid;
                    for (int i = 0; i < possiblePointSwap.Count; i++)
                    {
                        if (battle.enemydata[possiblePointSwap[i]].hp < battle.enemydata[targetId].hp)
                            targetId = possiblePointSwap[i];
                    }
                    yield return BattleControl_Ext.Instance.UseItem(entity, targetId, battle, (MainManager.Items)NewItem.PointSwap);
                    break;

                case Attacks.FireDrive:
                    yield return DoFireDrive(entity, actionid);
                    break;

                case Attacks.HammerThrow:
                    yield return DoHammerThrow(entity, actionid);
                    break;

                case Attacks.SpringJump:
                    yield return DoSpringJump(entity, actionid);
                    break;

                case Attacks.UseItem:
                    battle.enemydata[actionid].data[1] = 1;

                    for (int i = 0; i < 2; i++)
                    {
                        if (fightComponent.HasItem())
                            yield return fightComponent.DoUseItem(entity, actionid, fightComponent);
                    }
                    break;

                case Attacks.ChillOutHammer:
                    yield return DoChillOutHammer(entity, actionid);
                    break;

                case Attacks.Defend:
                    battle.enemydata[actionid].data[0] = 2;
                    yield return DoDefend(entity, actionid);
                    break;

                case Attacks.TornadoJump:
                    yield return DoTornadoJump(entity, actionid);
                    break;

                case Attacks.InkHammer:
                    yield return DoInkHammer(entity, actionid);
                    break;

                case Attacks.PowerJump:
                    yield return DoPowerJump(entity, actionid);
                    break;
            }
            entity.overrideanim = false;
            entity.overrridejump = false;
        }

        IEnumerator DoTornadoJump(EntityControl entity, int actionid)
        {
            Vector3 basePos = entity.transform.position;
            entity.animstate = (int)MainManager.Animations.Walk;
            yield return EventControl.tenthsec;

            battle.GetSingleTarget();
            entity.MoveTowards(Vector3.zero, 1.5f);
            yield return new WaitUntil(() => !entity.forcemove);

            entity.animstate = (int)MainManager.Animations.Angry;
            yield return new WaitForSeconds(0.15f);

            entity.PlaySound("Jump", 1, 0.9f);
            entity.LockRigid(true);
            entity.animstate = (int)MainManager.Animations.Jump;

            Vector3 baseAngle = entity.spritetransform.localEulerAngles;
            Vector3 startPos = entity.transform.position;
            Vector3 targetPos = battle.playertargetentity.transform.position + new Vector3(0, 1, -0.1f);

            yield return MainManager.ArcMovement(entity.gameObject, startPos, targetPos, Vector3.zero, 5, 30, false);

            entity.animstate = 100;
            yield return null;

            MainManager.PlaySound("HugeHit3", 1.2f, 1);
            MainManager.ShakeScreen(0.25f, 0.2f);
            battle.DoDamage(actionid, battle.playertargetID, TORNADO_JUMP_DMG, BattleControl.AttackProperty.Pierce, battle.commandsuccess);
            int dizzyTurn = battle.HardMode() ? TORNADO_JUMP_DIZZY + 1 : TORNADO_JUMP_DIZZY;

            if (!battle.commandsuccess)
            {
                BattleControl_Ext.Instance.TryDizzy(null, ref MainManager.instance.playerdata[battle.playertargetID], dizzyTurn);
            }
            yield return EventControl.tenthsec;
            int target = battle.playertargetID;
            battle.playertargetID = -1;

            entity.PlaySound("Jump", 1, 0.9f);
            entity.animstate = (int)MainManager.Animations.Jump;
            targetPos = battle.playertargetentity.transform.position + Vector3.up * 3;
            startPos = entity.transform.position;

            entity.spin = new Vector3(0, 20);
            yield return MainManager.ArcMovement(entity.gameObject, startPos, targetPos, Vector3.zero, 5, 30, false);
            MainManager.PlaySound("Spin", 9, 1.2f, 1f, true);
            float a = 0;
            float b = 30;
            do
            {
                entity.spin = new Vector3(0, Mathf.Lerp(20, 40, a / b));
                a += MainManager.TieFramerate(1f);
            } while (a < b);


            GameObject[] hurricanes = new GameObject[1];
            for (int i = 0; i < hurricanes.Length; i++)
            {
                Vector3 particlePos = battle.partymiddle;
                hurricanes[i] = MainManager.PlayParticle("HurricaneBig", particlePos);
                var shape = hurricanes[i].GetComponent<ParticleSystem>().shape;
                shape.radius = 1f;
            }

            yield return EventControl.halfsec;
            MainManager.PlaySound("Woosh3", 1.2f, 1f);

            for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
            {
                if (i != target && MainManager.instance.playerdata[i].hp > 0)
                {
                    battle.DoDamage(actionid, i, TORNADO_JUMP_DMG, BattleControl.AttackProperty.Pierce, battle.commandsuccess);
                    if (!battle.commandsuccess)
                    {
                        BattleControl_Ext.Instance.TryDizzy(null, ref MainManager.instance.playerdata[i], dizzyTurn);
                    }
                }
            }
            yield return EventControl.quartersec;
            MainManager.StopSound("Spin");
            yield return entity.SlowSpinStop(entity.spin, 30);

            entity.spritetransform.localEulerAngles = baseAngle;
            for (int i = 0; i < hurricanes.Length; i++)
            {
                hurricanes[i].GetComponent<ParticleSystem>().Stop();
                UnityEngine.Object.Destroy(hurricanes[i], 5);
            }

            yield return BattleControl_Ext.LerpPosition(10, entity.transform.position, new Vector3(entity.transform.position.x, 0), entity.transform);

            if (UnityEngine.Random.Range(0, 10) < 5)
            {
                int flipAmount = 5;
                startPos = new Vector3(entity.transform.position.x, 0.6f, entity.transform.position.z);
                targetPos = new Vector3(basePos.x, 0.6f, basePos.z);
                a = 0;
                b = 40;
                int stylishCount = 1;
                entity.animstate = (int)MainManager.Animations.Block;
                StylishUtils.ShowStylish(1.2f, entity, 0, false);

                yield return null;
                yield return null;

                entity.anim.enabled = false;
                MainManager_Ext.Instance.ChangeSpritePivot(entity, new Vector2(0.5f, 0.2f));

                do
                {
                    entity.transform.position = Vector3.Lerp(startPos, targetPos, a / b);
                    entity.spritetransform.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(360 * flipAmount, 0, a / b));
                    a += MainManager.TieFramerate(1f);

                    if (a > stylishCount * 5)
                    {
                        MainManager.PlaySound("Damage2", 1.2f, 1);
                        stylishCount++;
                    }
                    yield return null;
                } while (a < b);
                entity.anim.enabled = true;
                entity.transform.position = basePos;
                entity.spritetransform.localEulerAngles = Vector3.zero;
                if (UnityEngine.Random.Range(0, 10) < 5)
                {
                    yield return DoConfetti(entity);
                }
            }
            entity.LockRigid(false);
        }

        IEnumerator DoDefend(EntityControl entity, int actionid)
        {
            MainManager.PlaySound("Spin", 1.2f, 1);
            entity.animstate = (int)MainManager.Animations.Block;
            entity.spin = new Vector3(0, 20);
            yield return EventControl.halfsec;
            entity.spin = Vector3.zero;

            entity.basestate = entity.animstate;
            BattleControl_Ext.Instance.startState = entity.animstate;
            int sturdyTurns = battle.HardMode() ? DEFEND_STURDY_TURNS + 1 : DEFEND_STURDY_TURNS;
            BattleControl_Ext.Instance.SetSturdy(ref battle.enemydata[actionid], sturdyTurns, actionid);

            battle.enemydata[actionid].isdefending = true;
            battle.enemydata[actionid].defenseonhit = DEFEND_DEFENSE;
        }

        IEnumerator DoHammerThrow(EntityControl entity, int actionid)
        {
            battle.nonphyscal = true;
            battle.GetSingleTarget();
            MainManager.PlaySound("Spin", -1, 1.2f, 1, true);
            entity.animstate = 119;
            float a = 0;
            float b = 60;
            do
            {
                entity.spin = new Vector3(0, Mathf.Lerp(0, 30, a / b));
                a += MainManager.TieFramerate(1);
                yield return null;
            } while (a < b);
            MainManager.StopSound("Spin");
            entity.spin = Vector3.zero;
            entity.spritetransform.localEulerAngles = Vector3.zero;

            entity.animstate = 120;
            Sprite hammerSprite = MainManager_Ext.assetBundle.LoadAssetWithSubAssets<Sprite>("JumpAnt")[55];
            SpriteRenderer hammer = MainManager.NewSpriteObject(entity.transform.position + new Vector3(0, 1, -0.1f), battle.battlemap.transform, hammerSprite);
            Vector3 targetPos = new Vector3(-1, 10, -0.1f);

            MainManager.PlaySound("Toss10", 0.9f, 1);
            yield return MainManager.ArcMovement(hammer.gameObject, hammer.transform.position, targetPos, new Vector3(0, 0, 20), 10, 30, false);

            entity.animstate = 121;
            yield return EventControl.halfsec;

            entity.animstate = (int)MainManager.Animations.Flustered;
            yield return EventControl.halfsec;

            entity.animstate = (int)MainManager.Animations.Block;
            targetPos = battle.playertargetentity.transform.position + new Vector3(0, 1, -0.1f);
            MainManager.PlaySound("Toss4");
            yield return MainManager.ArcMovement(hammer.gameObject, hammer.transform.position, targetPos, new Vector3(0, 0, 20), 10, 30, false);

            battle.DoDamage(actionid, battle.playertargetID, HAMMER_THROW_DMG, BattleControl.AttackProperty.DefDownOnBlock, battle.commandsuccess);
            int defenseDownTurns = battle.HardMode() ? HAMMER_THROW_DEFDOWN : HAMMER_THROW_DEFDOWN - 1;
            BattleControl_Ext.Instance.DoConditionPierceStatEffect(ref MainManager.instance.playerdata[battle.playertargetID], defenseDownTurns, BattleCondition.DefenseDown, true, false);
            MainManager.RemoveCondition(MainManager.BattleCondition.Poison, MainManager.instance.playerdata[battle.playertargetID]);

            MainManager.ShakeScreen(0.1f, 0.5f);
            targetPos = new Vector3(-15, 4, -0.1f);
            yield return MainManager.ArcMovement(hammer.gameObject, hammer.transform.position, targetPos, new Vector3(0, 0, 20), 10, 40, false);

            hammer.transform.position = new Vector3(15, 5, -0.1f);
            targetPos = entity.transform.position + new Vector3(0, 1, -0.1f);

            entity.animstate = 121;
            Coroutine flipAround = battle.StartCoroutine(EventControl_Ext.FlipAround(entity, 0.5f));
            yield return EventControl.sec;
            battle.StopCoroutine(flipAround);

            entity.flip = true;
            entity.animstate = 120;
            yield return MainManager.ArcMovement(hammer.gameObject, hammer.transform.position, targetPos, new Vector3(0, 0, 20), 10, 40, true);

            MainManager.PlaySound("WaspKingAxeThrowCatch", 1.2f, 1);
            entity.animstate = 119;
            entity.flip = false;

            a = 0;
            b = 30;
            MainManager.PlaySound("Spin", -1, 1.2f, 1, true);
            do
            {
                entity.spin = new Vector3(0, Mathf.Lerp(30, 0, a / b));
                a += MainManager.TieFramerate(1);
                yield return null;
            } while (a < b);
            MainManager.StopSound("Spin");
            entity.spin = Vector3.zero;
            entity.spritetransform.localEulerAngles = Vector3.zero;
            yield return EventControl.quartersec;

            if (UnityEngine.Random.Range(0, 10) < 5)
            {
                StylishUtils.ShowStylish(1.2f, entity, 0, false);
                entity.animstate = 115;
                yield return EventControl.halfsec;
            }
            else
            {
                entity.animstate = (int)MainManager.Animations.Idle;
            }
        }

        IEnumerator DoFireDrive(EntityControl entity, int actionid)
        {
            battle.nonphyscal = true;
            Vector3 fireBallPos = entity.transform.position + new Vector3(0, 3, -0.1f);
            GameObject fireball = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Particles/Fireball"), fireBallPos, Quaternion.identity, battle.battlemap.transform) as GameObject;
            MainManager.PlaySound("WaspKingMFireball1", 1.2f, 1);

            battle.StartCoroutine(BattleControl_Ext.LerpScale(20, Vector3.zero, Vector3.one, fireball.transform));
            entity.animstate = (int)MainManager.Animations.ItemGet;
            yield return EventControl.halfsec;
            fireBallPos = entity.transform.position + new Vector3(-1, 10, -0.1f);

            entity.animstate = (int)MainManager.Animations.Jump;
            entity.PlaySound("Jump");
            entity.Jump(10);
            yield return new WaitForSeconds(0.05f);
            yield return MainManager.ArcMovement(fireball, fireball.transform.position, fireBallPos, new Vector3(0, 0, 20), 10, 30, false);
            yield return new WaitUntil(() => entity.onground);
            entity.animstate = 110;

            yield return EventControl.quartersec;

            float a = 0;
            float b = 30f;
            bool startedAnim = false;
            Vector3 startPos = fireball.transform.position;
            Vector3 endPos = entity.transform.position + new Vector3(-1, 1, -0.1f);
            do
            {
                fireball.transform.position = Vector3.Lerp(startPos, endPos, a / b);
                a += MainManager.TieFramerate(1f);

                if (a > 20f && !startedAnim)
                {
                    startedAnim = true;
                    entity.animstate = 112;
                }

                yield return null;
            } while (a < b);

            fireBallPos = MainManager.instance.playerdata[battle.partypointer[0]].battleentity.transform.position - Vector3.up;
            MainManager.PlaySound("HugeHit4", 1.2f, 1);
            battle.StartCoroutine(MainManager.ArcMovement(fireball, fireball.transform.position, fireBallPos, new Vector3(0, 0, 20), 5, 30, false));
            entity.StartCoroutine(entity.SlowSpinStop(new Vector3(0, 30, 0), 80));

            yield return EventControl.halfsec;

            fireball.GetComponentInChildren<ParticleSystem>().Stop();
            UnityEngine.Object.Destroy(fireball, 3);

            GameObject[] pillars = new GameObject[8];
            Vector3 pillarPos = fireBallPos;
            GameObject temp = Resources.Load("prefabs/maps/GiantLairBeforeBoss") as GameObject;
            GameObject tempPillar = UnityEngine.Object.Instantiate(temp.transform.Find("firehazard").GetChild(0).gameObject, new Vector3(999, 999), Quaternion.identity, battle.battlemap.transform);
            UnityEngine.Object.Destroy(tempPillar.GetComponent<Animator>());
            Transform particleObj = tempPillar.transform.GetChild(2).transform;

            foreach (ParticleSystem p in particleObj.GetComponentsInChildren<ParticleSystem>())
            {
                var main = p.main;
                p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                p.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }

            yield return null;
            bool[] damaged = new bool[MainManager.instance.playerdata.Length];
            int damage = FIRE_DRIVE_DMG;
            int burnTurns = battle.HardMode() ? FIRE_DRIVE_BURN + 1 : FIRE_DRIVE_BURN;

            bool stylished = false;

            for (int i = 0; i < pillars.Length; i++)
            {
                pillars[i] = UnityEngine.Object.Instantiate(tempPillar, pillarPos, Quaternion.identity, battle.battlemap.transform);
                pillars[i].transform.localScale = new Vector3(0.5f, 0, 0.5f);
                pillarPos += Vector3.left;
                battle.StartCoroutine(DoPillarAnim(pillars[i], new Vector3(0.5f, 0.5f - i * 0.025f, 0.5f)));
                yield return EventControl.tenthsec;
                for (int j = 0; j < damaged.Length; j++)
                {
                    if (!damaged[j] && pillars[i].transform.position.x <= MainManager.instance.playerdata[j].battleentity.transform.position.x && MainManager.instance.playerdata[j].hp > 0)
                    {
                        damaged[j] = true;
                        battle.DoDamage(actionid, j, damage, null, battle.commandsuccess);
                        BattleControl_Ext.Instance.DoConditionPierce(ref MainManager.instance.playerdata[j], burnTurns, MainManager.BattleCondition.Fire);
                        damage--;
                        break;
                    }
                }

                if (i >= 5 && !stylished)
                {
                    stylished = true;
                    if (UnityEngine.Random.Range(0, 10) < 5)
                    {
                        entity.overrideflip = true;
                        entity.spin = Vector3.zero;
                        entity.spritetransform.transform.localEulerAngles = Vector3.zero;
                        StylishUtils.ShowStylish(1.2f, entity, 0, false);
                        entity.animstate = 115;
                    }
                    else
                    {
                        entity.animstate = (int)MainManager.Animations.Idle;
                    }
                }
            }

            yield return new WaitUntil(() => MainManager.ArrayIsEmpty(pillars));
            UnityEngine.Object.Destroy(tempPillar);
            entity.overrideflip = false;
        }

        IEnumerator DoPillarAnim(GameObject pillar, Vector3 targetScale)
        {
            yield return BattleControl_Ext.LerpScale(15, pillar.transform.localScale, targetScale, pillar.transform);
            yield return EventControl.tenthsec;
            yield return BattleControl_Ext.LerpScale(15, pillar.transform.localScale, new Vector3(targetScale.x, 0, targetScale.z), pillar.transform);
            UnityEngine.Object.Destroy(pillar);

        }

        IEnumerator DoChillOutHammer(EntityControl entity, int actionid)
        {
            battle.nonphyscal = true;
            Vector3 basePos = entity.transform.position;
            entity.animstate = (int)MainManager.Animations.Walk;
            entity.MoveTowards(Vector3.zero, 1.5f);
            yield return new WaitUntil(() => !entity.forcemove);

            entity.overrideanim = true;
            GameObject icePart = MainManager.PlayParticle("IceRadius", entity.transform.position + new Vector3(1.2f, 1.5f, -0.1f), -1);
            icePart.transform.parent = entity.spritetransform;

            MainManager.PlaySound("Charge18", 1.2f, 1);
            entity.StartCoroutine(entity.ShakeSprite(0.2f, 60));
            entity.animstate = 104;
            entity.StartCoroutine(BattleControl_Ext.DoSpin(entity, 60, -15f));
            yield return EventControl.sec;

            entity.animstate = 107;
            yield return EventControl.tenthsec;

            entity.LockRigid(true);
            entity.PlaySound("Jump");
            yield return BattleControl_Ext.LerpPosition(5, entity.transform.position, entity.transform.position + Vector3.up * 2, entity.transform);

            MainManager.PlaySound("Spin", -1, 1.2f, 1, true);
            for (int i = 0; i < 4; i++)
                yield return BattleControl_Ext.DoSpin(entity, 15 - i * 3f);
            MainManager.StopSound("Spin");
            yield return EventControl.quartersec;

            entity.animstate = 109;
            entity.transform.position = new Vector3(entity.transform.position.x, 1.2f, entity.transform.position.z);
            yield return EventControl.tenthsec;

            icePart.GetComponent<ParticleSystem>().Stop();
            UnityEngine.Object.Destroy(icePart, 5);
            MainManager.ShakeScreen(0.5f, 1);
            MainManager.PlaySound("Thud3", 0.8f, 1);
            MainManager.PlayParticle(NewParticle.IceImpact.ToString(), entity.transform.position + new Vector3(-1, -1f, -0.1f));

            DialogueAnim[] pillars = new DialogueAnim[MainManager.instance.playerdata.Length];
            for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
            {
                float scale = UnityEngine.Random.Range(0.45f, 0.65f);
                var pillar = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Objects/icepillar"), MainManager.instance.playerdata[i].battleentity.transform.position, Quaternion.Euler(-90f, 0f, 0f)) as GameObject;
                pillars[i] = pillar.AddComponent<DialogueAnim>();
                pillars[i].targetscale = new Vector3(scale, scale, 1f);
                pillars[i].transform.localScale = Vector3.zero;
            }
            yield return EventControl.tenthsec;
            MainManager.PlaySound("WatcherIceSummon", 1.2f, 1);

            BattleControl.AttackProperty property = battle.commandsuccess ? BattleControl.AttackProperty.AtkDownOnBlock : BattleControl.AttackProperty.Freeze;
            battle.PartyDamage(actionid, battle.commandsuccess ? CHILL_OUT_DMG : CHILL_OUT_DMG - 1, property, battle.commandsuccess);
            yield return EventControl.halfsec;

            MainManager.PlaySound("IceMelt", 0.9f, 1);
            foreach (var pillar in pillars)
            {
                pillar.shrink = true;
                UnityEngine.Object.Destroy(pillar.gameObject, 4);
            }
            entity.transform.position = new Vector3(entity.transform.position.x, 0, entity.transform.position.z);
            entity.animstate = (int)MainManager.Animations.Idle;

            bool stylish = UnityEngine.Random.Range(0, 10) <= 4;
            if (stylish)
            {
                yield return DoHammerStylishFlip(entity, 3, 30, 10, basePos);
            }
            entity.LockRigid(false);
            entity.overrideanim = false;
        }

        IEnumerator DoPowerJump(EntityControl entity, int actionid)
        {
            entity.trail = true;
            Vector3 basePos = entity.transform.position;
            entity.animstate = (int)MainManager.Animations.Walk;
            yield return EventControl.tenthsec;

            battle.GetSingleTarget();
            entity.MoveTowards(Vector3.zero, 1.5f);
            yield return new WaitUntil(() => !entity.forcemove);

            entity.animstate = (int)MainManager.Animations.Angry;
            yield return new WaitForSeconds(0.15f);

            entity.PlaySound("Jump", 1, 0.9f);
            entity.LockRigid(true);
            entity.animstate = (int)MainManager.Animations.Jump;

            float a = 0;
            float b = 30;

            Vector3 angle = entity.spritetransform.localEulerAngles;
            Vector3 startPos = entity.transform.position;
            Vector3 targetPos = battle.playertargetentity.transform.position + new Vector3(0, 1, -0.1f);
            bool stylished = false;
            do
            {
                entity.transform.position = MainManager.BeizierCurve3(startPos, targetPos, 7, a / b);
                a += MainManager.TieFramerate(1f);

                if (!stylished && a > 5)
                {
                    stylished = true;

                    if (UnityEngine.Random.Range(0, 10) > 4)
                    {
                        entity.anim.enabled = false;
                        MainManager_Ext.Instance.ChangeSpritePivot(entity, new Vector2(0.5f, 0.4f));
                        StylishUtils.ShowStylish(1.2f, entity, 0, false, Vector3.down * 2);
                        entity.StartCoroutine(BattleControl_Ext.DoSpin(entity, 15f));
                    }
                }
                yield return null;
            } while (a < b);

            entity.anim.enabled = true;
            entity.animstate = 100;
            yield return null;

            MainManager.PlaySound("HugeHit3", 1.2f, 1);
            MainManager.ShakeScreen(0.25f, 0.2f);
            battle.DoDamage(actionid, battle.playertargetID, POWER_JUMP_DMG, BattleControl.AttackProperty.Pierce, battle.commandsuccess);

            if (!battle.commandsuccess)
            {
                int sleepTurns = battle.HardMode() ? POWER_JUMP_SLEEP_TURNS + 1 : POWER_JUMP_SLEEP_TURNS;
                battle.TryCondition(ref MainManager.instance.playerdata[battle.playertargetID], MainManager.BattleCondition.Sleep, sleepTurns);
            }
            yield return EventControl.tenthsec;
            yield return ReturnFromJumpAttack(entity, false);
            entity.trail = false;
        }

        IEnumerator DoSpringJump(EntityControl entity, int actionid)
        {
            Vector3 basePos = entity.transform.position;
            entity.animstate = (int)MainManager.Animations.Walk;
            yield return EventControl.tenthsec;

            battle.GetSingleTarget();
            entity.animstate = (int)MainManager.Animations.Jump;
            entity.Jump(10f);
            entity.PlaySound("Jump", 1, 0.9f);
            yield return EventControl.tenthsec;

            Vector3 startScale = entity.startscale;
            yield return new WaitUntil(() => entity.onground);

            MainManager.PlaySound("Charge3", 1.4f, 1);

            entity.animstate = (int)MainManager.Animations.Angry;
            yield return entity.ChangeScale(new Vector3(startScale.x, 0.5f, startScale.z), 130f, false);

            entity.animstate = (int)MainManager.Animations.Jump;
            GameObject shadow = UnityEngine.Object.Instantiate(entity.shadow.gameObject, battle.battlemap.transform);
            shadow.transform.position = entity.shadow.transform.position;
            shadow.transform.localScale = entity.shadow.transform.localScale;

            entity.startscale = startScale;
            entity.LockRigid(true);
            entity.spin = new Vector3(0, 20);
            Vector3 targetPos = entity.transform.position + new Vector3(-2, 10, 0);
            Vector3 startPos = entity.transform.position;

            MainManager.PlaySound("Boing0", 1.1f, 1);

            float a = 0;
            float b = 30f;
            do
            {
                entity.transform.position = MainManager.BeizierCurve3(startPos, targetPos, 10, a / b);
                shadow.transform.position = new Vector3(entity.transform.position.x, shadow.transform.position.y, entity.transform.position.z);
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < b);

            entity.trail = true;
            entity.spin = Vector3.zero;
            targetPos = new Vector3(battle.playertargetentity.transform.position.x, entity.transform.position.y, battle.playertargetentity.transform.position.z - 0.01f);
            startPos = entity.transform.position;

            a = 0;
            b = 120f;
            do
            {
                entity.transform.position = Vector3.Lerp(startPos, targetPos, a / b);
                shadow.transform.position = new Vector3(entity.transform.position.x, shadow.transform.position.y, entity.transform.position.z);
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < b);

            UnityEngine.Object.Destroy(shadow.gameObject);
            entity.animstate = 103;
            targetPos = battle.playertargetentity.transform.position + new Vector3(0, 1, -0.1f);

            yield return MainManager.ArcMovement(entity.gameObject, entity.transform.position, targetPos, Vector3.zero, 5, 15, false);

            entity.animstate = 100;
            yield return null;

            battle.DoDamage(actionid, battle.playertargetID, SPRING_JUMP_DMG, null, battle.commandsuccess);
            yield return EventControl.tenthsec;

            entity.trail = false;
            yield return DoGroundPound(entity, targetPos);

            yield return null;
            battle.DoDamage(actionid, battle.playertargetID, SPRING_JUMP_DMG, null, battle.commandsuccess);
            yield return EventControl.tenthsec;

            entity.anim.enabled = true;
            yield return ReturnFromJumpAttack(entity);
        }

        public static IEnumerator DoGroundPound(EntityControl entity, Vector3 targetPos)
        {
            entity.PlaySound("Jump", 1, 0.9f);
            entity.animstate = (int)MainManager.Animations.Jump;
            Vector3 mid = entity.transform.position + Vector3.up * 3;

            yield return MainManager.MoveTowards(entity.transform, mid, 10, true, false);
            entity.animstate = 103;
            yield return null;
            yield return null;

            entity.anim.enabled = false;
            MainManager_Ext.Instance.ChangeSpritePivot(entity);
            yield return BattleControl_Ext.DoSpin(entity, 15);
            yield return EventControl.quartersec;
            yield return MainManager.MoveTowards(entity.transform, targetPos + Vector3.up, 10, true, false);
        }

        //piercing blow
        IEnumerator DoInkHammer(EntityControl entity, int actionid)
        {
            bool hardmode = battle.HardMode();
            Vector3 basePos = entity.transform.position;
            entity.animstate = (int)MainManager.Animations.Walk;
            yield return EventControl.tenthsec;

            battle.GetSingleTarget();
            entity.MoveTowards(battle.playertargetentity.transform.position + new Vector3(1.5f, 0, -0.1f), 1.5f);
            yield return new WaitUntil(() => !entity.forcemove);

            entity.animstate = 104;
            yield return EventControl.sec;

            entity.animstate = 106;
            yield return EventControl.tenthsec;

            battle.DoDamage(actionid, battle.playertargetID, INK_HAMMER_DMG, null, battle.commandsuccess);
            int inkTurns = hardmode ? INK_HAMMER_TURNS + 1 : INK_HAMMER_TURNS;
            BattleControl_Ext.Instance.DoConditionPierce(ref MainManager.instance.playerdata[battle.playertargetID], inkTurns, BattleCondition.Inked, "InkGet", "WaterSplash2");
            MainManager.RemoveCondition(MainManager.BattleCondition.Poison, MainManager.instance.playerdata[battle.playertargetID]);
            MainManager.ShakeScreen(0.1f, 0.5f);
            yield return EventControl.tenthsec;

            if (UnityEngine.Random.Range(0, 10) < 5)
            {
                yield return DoHammerStylishFlip(entity, 1, 25, 7, entity.transform.position + Vector3.right * 1.5f);
            }
            else
            {
                yield return EventControl.halfsec;
            }

            entity.animstate = (int)MainManager.Animations.Idle;
        }

        IEnumerator DoShrinkStomp(EntityControl entity, int actionid)
        {
            Vector3 basePos = entity.transform.position;
            entity.animstate = (int)MainManager.Animations.Walk;
            yield return EventControl.tenthsec;

            battle.GetSingleTarget();
            entity.MoveTowards(Vector3.zero, 1.5f);
            yield return new WaitUntil(() => !entity.forcemove);

            entity.animstate = (int)MainManager.Animations.Angry;
            yield return new WaitForSeconds(0.08f);

            entity.PlaySound("Jump", 1, 0.9f);
            entity.LockRigid(true);
            entity.animstate = (int)MainManager.Animations.Jump;
            float a = 0;

            float b = 30;
            Vector3 startPos = entity.transform.position;
            Vector3 targetPos = battle.playertargetentity.transform.position + new Vector3(0, 1, -0.1f);
            do
            {
                entity.transform.position = MainManager.BeizierCurve3(startPos, targetPos, 5, a / b);
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < b);

            entity.animstate = 100;
            yield return null;

            battle.DoDamage(actionid, battle.playertargetID, SHRINK_STOMP_DMG, null, battle.commandsuccess);
            Vector3 partPos = MainManager.instance.playerdata[battle.playertargetID].battleentity.transform.position + Vector3.up;
            CheckBlockProperty(battle.playertargetID, NewCondition.Tiny, 2, "Shot2", NewParticle.TinyPart.ToString(), partPos);

            yield return EventControl.tenthsec;

            entity.PlaySound("Jump", 1, 0.9f);
            entity.animstate = (int)MainManager.Animations.Jump;
            Vector3 mid = entity.transform.position + Vector3.up * 5;
            startPos = entity.transform.position;

            bool stylished = false;
            a = 0;
            b = 30;
            do
            {
                entity.transform.position = MainManager.BeizierCurve3(startPos, targetPos, mid, a / b);
                a += MainManager.TieFramerate(1f);
                if (a >= b / 2 && !stylished)
                {
                    DoShrinkStompStylish(entity);
                    stylished = true;
                }

                yield return null;
            } while (a < b);

            entity.animstate = 100;
            yield return null;
            battle.DoDamage(actionid, battle.playertargetID, SHRINK_STOMP_DMG, null, battle.commandsuccess);
            CheckBlockProperty(battle.playertargetID, NewCondition.Tiny, 2, "Shot2", NewParticle.TinyPart.ToString(), partPos);
            yield return EventControl.tenthsec;

            yield return ReturnFromJumpAttack(entity);
        }

        public static IEnumerator ReturnFromJumpAttack(EntityControl entity, bool doStylish = true, bool aiParty = false)
        {
            entity.PlaySound("Jump", 1, 0.9f);
            entity.animstate = (int)MainManager.Animations.Jump;
            Vector3 startPos = entity.transform.position;

            float a = 0;
            float b = 30;
            bool stylished = false;
            do
            {
                entity.transform.position = MainManager.BeizierCurve3(startPos, Vector3.zero, 5, a / b);
                a += MainManager.TieFramerate(1f);

                if (doStylish && a >= b / 2 && !stylished)
                {
                    DoShrinkStompStylish(entity, aiParty);
                    stylished = true;
                }
                yield return null;
            } while (a < b);

            entity.animstate = (int)MainManager.Animations.Jump;
            for (int i = 0; i < 2; i++)
            {
                Vector3 xPos = !aiParty ? entity.transform.position + new Vector3(1 - i * 0.45f, 0) : entity.transform.position - new Vector3(1 - i * 0.45f, 0);
                yield return MainManager.ArcMovement(entity.gameObject, entity.transform.position, xPos, Vector3.zero, 2 - i * 0.8f, 8 - i * 3f, false);
            }
            entity.transform.position = new Vector3(entity.transform.position.x, 0);

            entity.animstate = (int)MainManager.Animations.Idle;
            entity.LockRigid(false);
        }

        void CheckBlockProperty(int target, NewCondition condition, int turns, string sound, string particle, Vector3 particlePos)
        {
            if (!battle.commandsuccess)
            {
                BattleControl_Ext.Instance.ApplyStatus((MainManager.BattleCondition)condition, ref MainManager.instance.playerdata[target], turns, sound, 1.2f, 1, particle, particlePos, Vector3.one);
            }
        }

        public static void DoShrinkStompStylish(EntityControl entity, bool increaseBar =false)
        {
            if (UnityEngine.Random.Range(0, 10) < 4)
            {
                entity.animstate = 101;
                StylishUtils.ShowStylish(1.2f, entity, MainManager.battle.enemy ? 0 : 0.1f, increaseBar, Vector3.down * 2);
            }
        }

        IEnumerator DoShrinkStompJump(EntityControl entity, float endTime, bool stylish, Vector3 startPos, Vector3 targetPos)
        {
            float a = 0;
            bool stylished = false;
            do
            {
                entity.transform.position = MainManager.SmoothLerp(startPos, targetPos, a / endTime);
                a += MainManager.TieFramerate(1f);

                if (a >= endTime / 2 && !stylished && stylish)
                {
                    DoShrinkStompStylish(entity);
                    stylished = true;
                }
                yield return null;
            } while (a < endTime);
        }

        public static IEnumerator DoHammerStylishFlip(EntityControl entity, int spinAmount, float flipEndTime, float flipHeight, Vector3 endPos, bool increaseBar = false)
        {
            StylishUtils.ShowStylish(1.2f, entity, MainManager.battle.enemy ? 0 : 0.1f, increaseBar);
            entity.LockRigid(true);
            entity.overrridejump = true;
            entity.overrideonlyflip = true;

            Vector3 startAngle = entity.spritetransform.eulerAngles;
            Vector3 startPos = entity.transform.position;
            float a = 0;

            entity.animstate = (int)MainManager.Animations.Jump;
            yield return null;
            yield return null;

            entity.anim.enabled = false;
            MainManager_Ext.Instance.ChangeSpritePivot(entity);


            float startSpin = 360 * spinAmount;
            float endSpin = 0;

            if (endPos.x < startPos.x)
            {
                startSpin = 0;
                endSpin = 360 * spinAmount;
            }

            do
            {
                entity.spritetransform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(startSpin, endSpin, a / flipEndTime));
                entity.transform.position = MainManager.BeizierCurve3(startPos, endPos, flipHeight, a / flipEndTime);
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < flipEndTime);

            entity.overrridejump = false;
            entity.overrideonlyflip = false;

            entity.transform.position = new Vector3(entity.transform.position.x, 0);
            entity.spritetransform.eulerAngles = startAngle;
            entity.LockRigid(false);
            entity.anim.enabled = true;
            entity.animstate = (int)MainManager.Animations.Idle;
            yield return EventControl.tenthsec;

            if (UnityEngine.Random.Range(0, 10) < 5)
            {
                yield return DoConfetti(entity, increaseBar);
            }
        }

        public static IEnumerator DoConfetti(EntityControl entity, bool increaseBar = false)
        {
            StylishUtils.ShowStylish(1.2f, entity, MainManager.battle.enemy ? 0 : 0.1f, increaseBar);
            entity.animstate = (int)MainManager.Animations.ItemGet;
            var ps = MainManager.PlayParticle(NewParticle.Confetti.ToString(), entity.transform.position + Vector3.up * 2).GetComponent<ParticleSystemRenderer>();
            ps.material = MainManager.spritedefaultunity;
            yield return EventControl.halfsec;
        }
    }
}
