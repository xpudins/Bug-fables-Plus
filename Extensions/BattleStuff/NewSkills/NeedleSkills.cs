using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MainManager;
using static BattleControl;
using static BFPlus.Extensions.BattleControl_Ext;
using static UnityEngine.Object;
using BFPlus.Extensions.BattleStuff.NewCommands;
using InputIOManager;

namespace BFPlus.Extensions.BattleStuff.Skills
{
    public class NeedleSkills
    {
        public static int totalHits;
        public static int damage;
        public static AttackProperty property;
        public static List<DamageOverride> overrides;

        public static int?[] targetDamages;
        public static int[] targetHits;

        public static IEnumerator DoNewNeedlePincer()
        {
            bool vanilla = !MainManager_Ext.Instance.balanceChanges[(int)NewMenuText.NeedlePincer];

            BattleData viData = MainManager.instance.playerdata[battle.currentturn];
            EntityControl vi = viData.battleentity;
            int targetID = battle.GetEnemyFromAvaliable(battle.avaliabletargets[battle.target]).battleentity.battleid;

            vi.animstate = 115;
            yield return new WaitForSeconds(0.65f);

            SetCamera(battle.enemydata[targetID].battleentity.transform.position, MainManager.instance.camangleoffset, 
                new Vector3(0f, 2.5f, -6f), 0.02f);

            vi.MoveTowards(battle.enemydata[targetID].battleentity.transform.position +  new Vector3(-battle.GetEnemySize(targetID),
                0f, -0.1f), 2f, 1, 121);
            yield return new WaitUntil(() => !vi.forcemove);

            damage = viData.atk;
            property = AttackProperty.None;
            overrides = GetNeedleOverrides();

            float actionCommandTime = 60f;
            totalHits = GetTotalHits(2);
            for (int i = 0; i < totalHits; i++)
            {
                battle.StartCoroutine(battle.DoCommand(actionCommandTime, ActionCommands.PressKeyTimer, 
                    new float[1] { UnityEngine.Random.Range(4, 7) }));
                yield return null;
                yield return new WaitUntil(() => !battle.doingaction);

                if (!battle.commandsuccess)
                {
                    vi.animstate = 106;
                    yield return EventControl.quartersec;

                    int finalNeedleDamage = damage;
                    if (i > 0 && finalNeedleDamage > 1)
                        finalNeedleDamage = Mathf.FloorToInt(finalNeedleDamage * ((3 - i) / 3f) / 2f);
                    overrides = new List<DamageOverride> { DamageOverride.FailSound };

                    if (!vanilla)
                        battle.DoDamage(viData, ref battle.enemydata[targetID], finalNeedleDamage, property, 
                            overrides.ToArray(), false);
                    else
                        battle.DoDamage(viData, ref battle.enemydata[targetID], battle.GetPlayerAttack(viData.trueid, false), 
                            property, overrides.ToArray(), false);

                    NeedleStatuses_Attacker(ref viData);
                    break;
                }

                vi.animstate = (i % 3 == 0) ? 122 : 123;

                battle.StartCoroutine(MoveTowards(vi.transform, vi.transform.position + new Vector3(0.65f, 0f), 
                    vi.transform.position, 10f, smooth: false, null));
                if (i == 0)
                {
                    if (!vanilla)
                    {
                        damage = battle.DoDamage(viData, ref battle.enemydata[targetID], damage, property, overrides.ToArray(),
                        block: false);
                        property = AttackProperty.NoExceptions;
                    }
                    else
                    {
                        battle.DoDamage(viData, ref battle.enemydata[targetID], viData.atk, AttackProperty.Pierce,
                            overrides.ToArray(), false);
                    }
                }
                else
                {
                    if (!vanilla)
                    {
                        if (damage > 1)
                            damage = Mathf.Max(1, Mathf.FloorToInt(damage / 2f));

                        battle.DoDamage(viData, ref battle.enemydata[targetID], damage, property, overrides.ToArray(),
                            block: false);
                    }
                    else
                    {
                        int vanillaDamageCalc = Mathf.Clamp(Mathf.FloorToInt((float)viData.atk * 0.7f * (i != 1 ? 0.7f : 1f)), 1, 99);
                        battle.DoDamage(viData, ref battle.enemydata[targetID], vanillaDamageCalc, AttackProperty.Pierce,
                            overrides.ToArray(), false);
                    }
                }
                NeedleStatuses_Attacker(ref viData);
                NeedleStatuses_Target(ref battle.enemydata[targetID], vanilla:vanilla);
                StartStylishTimer(4f, 14f, stylishID: (i == totalHits - 1) ? 1 : 0, 0.05f);

                yield return new WaitUntil(() => battle.commandsprites[0] == null);

                actionCommandTime *= 0.75f;

                if (i != totalHits - 1 && i % 3 == 1)
                    vi.animstate = 121;
            }

            yield return WaitStylish(battle.commandsuccess ? 0.2f : 0.5f);
        }

        public static IEnumerator DoNewNeedleToss()
        {
            BattleData viData = MainManager.instance.playerdata[battle.currentturn];
            EntityControl vi = viData.battleentity;

            vi.animstate = 115;
            yield return new WaitForSeconds(0.65f);
            vi.overridefly = true;
            vi.animstate = 117;

            Vector3 startPos = vi.transform.position;
            Vector3 endPos = Vector3.Lerp(
                    MainManager.instance.playerdata[battle.partypointer[0]].battleentity.transform.position,
                    MainManager.instance.playerdata[battle.partypointer[battle.partypointer.Length - 1]].battleentity.transform.position,
                    0.3f);
            endPos.z -= 0.65f;

            float a = 0f;
            float b = 20f;
            MainManager.instance.camtargetpos = Vector3.Lerp(new Vector3(-1.75f, 0f, -0.5f), 
                battle.avaliabletargets[battle.target].battleentity.transform.position, 0.5f);
            do
            {
                float f = Mathf.Pow(a / b, 0.4f);
                vi.transform.position = Vector3.Lerp(startPos, endPos, f);
                vi.height = Mathf.Lerp(0f, 2.75f, f);
                a += TieFramerate(1f);
                yield return null;
            }
            while (a < b + 1f);

            int[] targetIDs = battle.GetEnemies(flying: true, underground: true, flyinghigh: true);

            List<BattleData> targets = new List<BattleData>();
            for (int i = 0; i < targetIDs.Length; i++)
                targets.Add(battle.enemydata[targetIDs[i]]);

            targetDamages = new int?[battle.enemydata.Length];
            targetHits = new int[battle.enemydata.Length];

            damage = viData.atk;
            overrides = GetNeedleOverrides();
            totalHits = GetTotalHits(2);
            int reticleRNG = UnityEngine.Random.Range(0, 2);
            Vector3 baseSpriteAngle = vi.spritetransform.localEulerAngles;

            for (int t = 0; t < totalHits; t++)
            {
                vi.animstate = 118;
                vi.bobspeed = 0.1f;
                vi.bobrange = 3f;
                vi.overrideflip = true;
                vi.spritetransform.localEulerAngles = new Vector3(0, 180, 340);

                yield return CursorCommand.DoCursorCommand(targets.ToArray(), 150f, new Vector2(4f, 4f), 1.15f, 
                    (t + reticleRNG % 2 == 0) ? 1f : -1f);

                int id = battle.commandsuccess ? CursorCommand.commandTarget : UnityEngine.Random.Range(0, targets.Count);
                Vector3 targetPos = targets[id].battleentity.transform.position + Vector3.up * targets[id].battleentity.height;
                if (battle.commandsuccess)
                {
                    targetPos = new Vector3(0, 15, 0);
                }

                vi.overrideanim = true;
                vi.bobrange = 0f;
                vi.bobspeed = 0f;
                vi.animstate = 119;
                PlaySound("Toss");
                StartStylishTimer(4, 16, stylishGain: 0.02f * Mathf.Max(1, 3 - Instance.stylishCountThisAction));
                yield return new WaitForSeconds(0.05f);

                battle.StartCoroutine(TossViNeedle(viData, vi.sprite.transform.position, targetPos, damage, id, battle.commandsuccess, targetHits[targetIDs[id]]));
                NeedleStatuses_Attacker(ref MainManager.instance.playerdata[battle.currentturn]);

                battle.UpdateAnim();
                targetHits[targetIDs[id]]++;
                yield return new WaitForSeconds(0.5f);
            }
            vi.spritetransform.localEulerAngles = baseSpriteAngle;

            float startHeight = vi.height;
            a = 0f;
            b = 15f;
            do
            {
                vi.height = Mathf.Lerp(startHeight, 0, a / b);
                a += framestep;
                yield return null;
            }
            while (a < b + 1f);
            vi.height = 0f;
            vi.overrideanim = false;
            vi.overridefly = false;

            targetDamages = null;
            targetHits = null;
        }

        static IEnumerator TossViNeedle(BattleData attacker, Vector3 throwerPos, Vector3 targetPos, 
            int baseDMG, int targetID, bool successfulThrow, int repeats)
        {
            SpriteRenderer needleSprite = NewSpriteObject(throwerPos, null, 
                MainManager.instance.projectilepsrites[2]);

            needleSprite.transform.position = throwerPos + new Vector3(0.8f, 1.6f, -0.1f);
            needleSprite.transform.localScale = Vector3.one * 1.15f;

            Vector3 needleStartPos = needleSprite.transform.position;
            needleSprite.transform.localEulerAngles = new Vector3(0, 0, 130);
            needleSprite.gameObject.AddComponent<ShadowLite>().SetUp(0.3f, 0.5f);

            float frametime = (targetPos - needleStartPos).magnitude * 3.5f;
            if (successfulThrow)
            {
                float a = 0;
                do
                {
                    needleSprite.transform.position = Vector3.Lerp(needleStartPos, targetPos, a / frametime);
                    a += framestep;
                    yield return null;
                }
                while (a < frametime + 1f);
            }
            else
            {
                float rng = UnityEngine.Random.value;
                float spinFac = 0.5f * Mathf.Pow(Mathf.Sin(Mathf.PI * rng), 0.5f);

                if (rng > 0.5f) 
                    spinFac = 1f - spinFac;

                Vector3 spin = new Vector3(0, 0, Mathf.Lerp(20, 50, spinFac));
                float height = UnityEngine.Random.Range(2.5f, 3.75f);
                yield return ArcMovement(needleSprite.gameObject, needleStartPos, targetPos, spin, height, frametime, true);
            }

            if (!successfulThrow)
            {
                // on fail: immediately hits a random enemy for half damage and zero additional effects
                if (battle.enemydata[targetID].hp > 0 && battle.enemydata[targetID].position != BattlePosition.Underground 
                    && battle.enemydata[targetID].position != BattlePosition.OutOfReach)
                {
                    if (targetDamages[targetID] == null)
                    {
                        targetDamages[targetID] = battle.DoDamage(attacker, ref battle.enemydata[targetID], 
                            Mathf.FloorToInt(baseDMG / 2f), null, null, false);
                    }
                    else
                    {
                        if (targetDamages[targetID].Value > 1)
                            targetDamages[targetID] = Mathf.FloorToInt(targetDamages[targetID].Value / 2f);
                        property = AttackProperty.NoExceptions;
                        battle.DoDamage(attacker, ref battle.enemydata[targetID], targetDamages[targetID].Value, property, null, false);
                    }
                }
            }
            else
            {
                // on success: throws a needle into the air, hitting the target at the end of next turn for full dmg, 2 pierce,
                // and needle effects
                property = repeats > 0 ? AttackProperty.NoExceptions : AttackProperty.None;
                Instance.AddDelProjsPlayer(needleSprite.gameObject, DelProjType.NeedleToss, targetID, baseDMG, 1, 0, 
                    property, overrides, 35f, attacker, null, null, "@Toss4", true, new List<BattlePosition>() { BattlePosition.Ground, BattlePosition.Flying });
                Instance.SetLastDelProjArgs("move,0,-0.5,0@noshadow@partoff,0,0.5,0@partoff,0,1,0");
                needleSprite.transform.localEulerAngles = new Vector3(0, 0, -90f - 90f * Vector2.Dot(Vector2.up, 
                    (battle.enemydata[targetID].battleentity.transform.position - targetPos).normalized));
            }
        }

        // these will all be reused for redone needle pincer
        static List<DamageOverride> GetNeedleOverrides()
        {
            List<DamageOverride> overrides = new List<DamageOverride>()
            { 
                BadgeIsEquipped((int)BadgeTypes.NumbNeedle) ? DamageOverride.DontAwake : DamageOverride.IgnoreNumb 
            };

            int frostNeedles = BadgeHowManyEquipped((int)Medal.FrostNeedles);
            if (frostNeedles > 0)
                overrides.Add((DamageOverride)NewDamageOverride.Magic);

            for (int pierce = 1 + frostNeedles; pierce > 0; pierce--)
                overrides.Add((DamageOverride)NewDamageOverride.Pierce1);

            return overrides;
        }

        public static void NeedleStatuses_Target(ref BattleData target, int turnBonus = 0, int resPierce = 0, bool vanilla = false)
        {
            if (HasCondition(BattleCondition.Sturdy, target) > -1) 
                return;

            Entity_Ext extEnt = Entity_Ext.GetEntity_Ext(target.battleentity);
            if (!vanilla && extEnt != null)
                extEnt.piercedStatusRes += resPierce;
   
            int stsDur = 1 + BadgeHowManyEquipped((int)BadgeTypes.NumbNeedle);

            if (!vanilla)
            {
                int stickyBoost = 0;
                if (HasCondition(BattleCondition.Sticky, target) > -1)
                    stickyBoost += 1 + BadgeHowManyEquipped((int)Medal.SturdyStrands);

                if (stsDur > 1)
                {
                    if (HasCondition(BattleCondition.Sleep, target) < stsDur + stickyBoost)
                    {
                        battle.TryCondition(ref target, BattleCondition.Sleep, stsDur + turnBonus);
                    }
                }
                else if (HasCondition(BattleCondition.Numb, target) < stsDur + stickyBoost)
                {
                    battle.TryCondition(ref target, BattleCondition.Numb, stsDur + turnBonus);
                }
            }
            else
            {
                if (stsDur > 1)
                    battle.TryCondition(ref target, BattleCondition.Sleep, stsDur);
            }

            if ((stsDur = BadgeHowManyEquipped((int)BadgeTypes.PoisonNeedle)) > 0)
                battle.TryCondition(ref target, BattleCondition.Poison, stsDur + 1 + turnBonus);

            if ((stsDur = BadgeHowManyEquipped((int)Medal.FireNeedles)) > 0)
                battle.TryCondition(ref target, BattleCondition.Fire, stsDur + 1 + turnBonus);

            if (!vanilla && extEnt != null)
                extEnt.piercedStatusRes -= resPierce;
        }

        public static void NeedleStatuses_Attacker(ref BattleData attacker, int turnBonus = 0)
        {
            if (HasCondition(BattleCondition.Sturdy, attacker) > -1) 
                return;

            int stsDur = BadgeHowManyEquipped((int)Medal.FireNeedles);

            if (stsDur > 0)
                battle.TryCondition(ref attacker, BattleCondition.Fire, stsDur + 1 + turnBonus);
        }

        static int GetTotalHits(int baseHits)
        {
            return baseHits + BadgeHowManyEquipped((int)BadgeTypes.Beemerang2) - BadgeHowManyEquipped((int)Medal.FireNeedles);
        }
    }
}