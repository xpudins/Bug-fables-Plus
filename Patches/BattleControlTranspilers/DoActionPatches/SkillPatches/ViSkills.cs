using BFPlus.Extensions;
using BFPlus.Extensions.BattleStuff;
using BFPlus.Extensions.BattleStuff.Skills;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static BattleControl;
using static MainManager;
using Random = UnityEngine.Random;

namespace BFPlus.Patches.BattleControlTranspilers.DoActionPatches.SkillPatches
{
    /// <summary>
    /// We insert our new formula for ttoss damages
    /// </summary>
    public class PatchViTornadoToss : PatchBaseDoAction
    {
        public PatchViTornadoToss()
        {
            priority = 59785;
        }
        public static int baseDamage;

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdfld(AccessTools.Field(typeof(BattleControl), "lastdamage")));

            cursor.GotoPrev(MoveType.After,
                i => i.MatchCall(AccessTools.Method(typeof(BattleControl), "SetDefaultCamera", new Type[] { })));

            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchViTornadoToss), nameof(GetTornadoFirstHitProperty)));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchViTornadoToss), nameof(GetFirstHitBaseDamage)));
            
            cursor.GotoNext(i => i.MatchLdloc1(), i => i.MatchLdloc1());
            int index = cursor.Index;

            cursor.GotoPrev(i => i.MatchLdfld(out _));
            var indexRef = cursor.Next.Operand;
            cursor.Goto(index);

            ILLabel label = cursor.DefineLabel();
            ILLabel jumpLabel = cursor.DefineLabel();
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchViTornadoToss), nameof(GetBalanceState)));
            cursor.Emit(OpCodes.Brfalse, label);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, indexRef);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchViTornadoToss), nameof(NewMultiHitDamage)));
            cursor.Emit(OpCodes.Br, jumpLabel);
            cursor.MarkLabel(label);

            cursor.GotoNext(MoveType.After,i => i.MatchPop());
            cursor.MarkLabel(jumpLabel);

            label = cursor.DefineLabel();
            cursor.GotoNext(i => i.MatchBle(out _));
            cursor.GotoPrev(MoveType.After,i => i.MatchLdarg0());
            cursor.Prev.OpCode = OpCodes.Nop;
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchViTornadoToss), nameof(GetBalanceState)));
            cursor.Emit(OpCodes.Brtrue, label);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.GotoNext(MoveType.After,i => i.MatchBle(out _));
            cursor.MarkLabel(label);

            ILLabel defaultCodeLabel = cursor.DefineLabel();
            ILLabel skipDefaultLabel = cursor.DefineLabel();
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchViTornadoToss), nameof(GetBalanceState)));
            cursor.Emit(OpCodes.Brfalse, defaultCodeLabel);

            cursor.Emit(OpCodes.Ldsfld, AccessTools.Field(typeof(PatchViTornadoToss), "baseDamage"));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, indexRef);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "GetMultiHitDamage"));
            cursor.Emit(OpCodes.Br, skipDefaultLabel);
            cursor.MarkLabel(defaultCodeLabel);
            cursor.GotoNext(MoveType.After,i => i.MatchSub());
            cursor.MarkLabel(skipDefaultLabel);

            cursor.GotoNext(i => i.MatchLdcI4(1), i => i.MatchLdcI4(99));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchViTornadoToss), nameof(GetClampedMin)));
            cursor.Remove();
        }

        public static void GetFirstHitBaseDamage(AttackProperty? property)
        {
            BattleData playerData = GetPlayerData(battle.currentturn, true);
            var overrides = new DamageOverride[] { (DamageOverride)NewDamageOverride.IgnoreInvulnerable };
            int damage = Mathf.Clamp(Mathf.FloorToInt((float)playerData.atk - 1f), 0, 99);
            baseDamage = DamagePipelineHandler.GetFinalDMG(damage, playerData, ref battle.enemydata[battle.target], property, ref overrides)+1;
        }

        static void NewMultiHitDamage(int index)
        {
            battle.DoDamage(
                instance.playerdata[battle.currentturn],
                ref battle.enemydata[battle.target],
                BattleControl_Ext.GetMultiHitDamage(baseDamage, index),
                AttackProperty.NoExceptions
            );
        }

        static AttackProperty? GetTornadoFirstHitProperty() => null;

        static bool GetBalanceState() => MainManager_Ext.Instance.GetBalanceChangeState((int)NewMenuText.TornadoToss);
        static int GetClampedMin() => GetBalanceState() ? 0 : 1;
    }

    public class PatchViHurricaneToss : PatchBaseDoAction
    {
        public PatchViHurricaneToss()
        {
            priority = 59230;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(155));
            cursor.GotoPrev(i => i.MatchLdfld(AccessTools.Field(typeof(BattleControl), "lastdamage")));
            cursor.GotoPrev(MoveType.After,i => i.MatchStfld(AccessTools.Field(typeof(BattleControl), "combo")));

            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchViHurricaneToss), nameof(GetHurricaneFirstHitProperty)));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchViTornadoToss), nameof(PatchViTornadoToss.GetFirstHitBaseDamage)));

            cursor.GotoNext(i => i.MatchLdloc1(), i => i.MatchLdloc1());
            int index = cursor.Index;

            cursor.GotoPrev(i => i.MatchLdfld(out _));
            var indexRef = cursor.Next.Operand;
            cursor.Goto(index);

            ILLabel label = cursor.DefineLabel();
            ILLabel jumpLabel = cursor.DefineLabel();
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchViHurricaneToss), nameof(GetBalanceState)));
            cursor.Emit(OpCodes.Brfalse, label);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, indexRef);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchViHurricaneToss), nameof(NewFirstHitDamage)));
            cursor.Emit(OpCodes.Br, jumpLabel);
            cursor.MarkLabel(label);

            cursor.GotoNext(MoveType.After, i => i.MatchPop());
            cursor.MarkLabel(jumpLabel);

            label = cursor.DefineLabel();
            cursor.GotoNext(i => i.MatchBle(out _));
            cursor.GotoPrev(MoveType.After, i => i.MatchLdarg0());
            cursor.Prev.OpCode = OpCodes.Nop;
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchViHurricaneToss), nameof(GetBalanceState)));
            cursor.Emit(OpCodes.Brtrue, label);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.GotoNext(MoveType.After, i => i.MatchBle(out _));
            cursor.MarkLabel(label);

            ILLabel defaultCodeLabel = cursor.DefineLabel();
            ILLabel skipDefaultLabel = cursor.DefineLabel();
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchViHurricaneToss), nameof(GetBalanceState)));
            cursor.Emit(OpCodes.Brfalse, defaultCodeLabel);

            cursor.Emit(OpCodes.Ldsfld, AccessTools.Field(typeof(PatchViTornadoToss), "baseDamage"));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, indexRef);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "GetMultiHitDamage"));
            cursor.Emit(OpCodes.Br, skipDefaultLabel);
            cursor.MarkLabel(defaultCodeLabel);
            cursor.GotoNext(MoveType.After, i => i.MatchSub());
            cursor.MarkLabel(skipDefaultLabel);

            cursor.GotoNext(i=>i.MatchLdcI4(1),i => i.MatchLdcI4(99));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchViHurricaneToss), nameof(GetClampedMin)));
            cursor.Remove();
        }

        public static void NewFirstHitDamage(int index)
        {
            battle.DoDamage(
                instance.playerdata[battle.currentturn],
                ref battle.enemydata[battle.target],
                BattleControl_Ext.GetMultiHitDamage(PatchViTornadoToss.baseDamage, index),
                AttackProperty.NoExceptions,
                new DamageOverride[] { (DamageOverride)NewDamageOverride.FlipNoPierce },
                false
            );
        }

        static int GetClampedMin() =>GetBalanceState() ? 0 : 1;

        static AttackProperty? GetHurricaneFirstHitProperty() => AttackProperty.Flip;
        static bool GetBalanceState() => MainManager_Ext.Instance.GetBalanceChangeState((int)NewMenuText.HurricaneToss);
    }

    /// <summary>
    /// If we have the balance changes enabled, go to new needle toss, otherwise stay in vanilla needle toss
    /// </summary>
    public class PatchViRedoneNeedleToss : PatchBaseDoAction
    {
        public PatchViRedoneNeedleToss()
        {
            priority = 56591;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;

            cursor.GotoNext(x => x.MatchLdcI4(115), i => i.MatchStfld(AccessTools.Field(typeof(EntityControl), "animstate")));
            cursor.GotoPrev(x => x.MatchBr(out label));
            cursor.GotoNext(MoveType.After, x => x.MatchLdarg(0));

            cursor.Prev.OpCode = OpCodes.Nop;
            ILLabel jumpLabel = cursor.DefineLabel();
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchViRedoneNeedleToss), nameof(CheckNeedleTossType)));
            cursor.Emit(OpCodes.Brfalse, jumpLabel);

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(NeedleSkills), "DoNewNeedleToss"));
            Utils.InsertYieldReturn(cursor);
            cursor.Emit(OpCodes.Br, label);

            cursor.MarkLabel(jumpLabel);
            cursor.Emit(OpCodes.Ldarg_0);
        }

        static bool CheckNeedleTossType()
        {
            return MainManager_Ext.Instance.balanceChanges[(int)NewMenuText.NeedleToss];
        }
    }

    /// <summary>
    /// Change the hits amount to accomodate burn needles.
    /// </summary>
    public class PatchNeedleTossHits : PatchBaseDoAction
    {
        public PatchNeedleTossHits()
        {
            priority = 56740;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {

            cursor.GotoNext(x => x.MatchLdcI4(28));

            FieldReference hits = null;
            cursor.GotoNext(MoveType.After, i => i.MatchStfld(out hits));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldc_I4_2);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(NeedleSkills), "GetTotalHits"));
            cursor.Emit(OpCodes.Stfld, hits);
        }
    }

    public class PatchNeedleTossStatus : PatchBaseDoAction
    {
        public PatchNeedleTossStatus()
        {
            priority = 57348;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            MethodInfo doDamageRef = AccessTools.Method(typeof(BattleControl), nameof(BattleControl.DoDamage),
                new Type[]
                {
                    typeof(BattleData?), typeof(BattleData).MakeByRefType(), typeof(int), typeof(AttackProperty?)
                });

            MethodInfo doDamageFullRef = AccessTools.Method(typeof(BattleControl), nameof(BattleControl.DoDamage),
                new Type[]
                {
                    typeof(BattleData?), typeof(BattleData).MakeByRefType(), typeof(int), typeof(AttackProperty?),
                    typeof(DamageOverride[]), typeof(bool)
                });

            cursor.GotoNext(i => i.MatchCall(doDamageRef), i=>i.MatchPop());

            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchNeedleTossStatus), nameof(GetOverrides)));
            cursor.Emit(OpCodes.Ldc_I4_0);
            cursor.Emit(OpCodes.Call, doDamageFullRef);
            cursor.Remove();

            cursor.GotoNext(i => i.MatchLdarg0());

            int index = cursor.Index;
            int enemyIndex = 0;
            cursor.GotoNext(i => i.MatchLdloc(out enemyIndex));

            FieldReference hitsRef = null;
            cursor.GotoNext(i => i.MatchLdfld(out hitsRef));

            cursor.GotoNext(i => i.MatchLdcI4(126));
            cursor.GotoNext(i => i.MatchBlt(out _));

            var totalHits = cursor.Prev.Operand;
            cursor.Goto(index);

            cursor.Emit(OpCodes.Ldloc, enemyIndex);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, hitsRef);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, totalHits);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchNeedleTossStatus), nameof(DoNeedleStatus)));
        }

        static void DoNeedleStatus(int targetId, int hits, int totalHits)
        {
            int stsDur;

            if(MainManager.HasCondition(BattleCondition.Sturdy, instance.playerdata[battle.currentturn]) == -1)
            {
                stsDur = BadgeHowManyEquipped((int)Medal.FireNeedles);
                if (stsDur > 0)
                    battle.TryCondition(ref MainManager.instance.playerdata[battle.currentturn], BattleCondition.Fire, stsDur + 1);
            }

            if (MainManager.HasCondition(BattleCondition.Sturdy, battle.enemydata[targetId]) == -1)
            {
                if ((stsDur = BadgeHowManyEquipped((int)BadgeTypes.PoisonNeedle)) > 0)
                    battle.TryCondition(ref battle.enemydata[targetId], BattleCondition.Poison, stsDur + 1);

                if ((stsDur = BadgeHowManyEquipped((int)Medal.FireNeedles)) > 0)
                    battle.TryCondition(ref battle.enemydata[targetId], BattleCondition.Fire, stsDur + 1);

                if ((stsDur = BadgeHowManyEquipped((int)BadgeTypes.NumbNeedle)) > 0)
                    battle.TryCondition(ref battle.enemydata[targetId], BattleCondition.Sleep, stsDur + 1);
            }

            BattleControl_Ext.StartStylishTimer(4f, 14f, stylishID: (hits == totalHits - 1) ? 1 : 0, 0.05f, commandSuccess:false);
        }

        static DamageOverride[] GetOverrides()
        {
            DamageOverride[] overrides = null;
            int frostNeedles = BadgeHowManyEquipped((int)Medal.FrostNeedles);
            if (frostNeedles > 0)
                overrides = new DamageOverride[] { (DamageOverride)NewDamageOverride.Magic};
            return overrides;
        }
    }

    public class PatchViRedoneNeedlePincer : PatchBaseDoAction
    {
        public PatchViRedoneNeedlePincer()
        {
            priority = 58096;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;

            cursor.GotoNext(x => x.MatchLdcI4(140));
            cursor.GotoNext(x => x.MatchBr(out label));
            cursor.GotoPrev(x => x.MatchLdcI4(115));
            cursor.GotoPrev(MoveType.After, x => x.MatchLdarg(0));

            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(NeedleSkills), "DoNewNeedlePincer"));
            Utils.InsertYieldReturn(cursor);
            cursor.Emit(OpCodes.Br, label);
            cursor.Emit(OpCodes.Ldarg_0);
        }
    }

    public class PatchViHeavyThrow : PatchBaseDoAction
    {
        public PatchViHeavyThrow()
        {
            priority = 59229;
        }

        protected override void ApplyPatch(ILCursor c, ILContext context)
        {
            c.GotoPrev(x => x.MatchLdstr("Prefabs/Objects/BeerangBattle"));
            c.GotoNext(x => x.MatchCall(AccessTools.Method(typeof(UnityEngine.Object), "Destroy", new Type[] { typeof(UnityEngine.Object) })));

            object rangField = c.Prev.Operand;

            c.GotoPrev(x => x.MatchLdarg(0));
            ILLabel skipToDestroy = c.MarkLabel();

            c.GotoNext(x => x.MatchPop());

            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchViHeavyThrow), "HeavyThrow_SelfDebuffOnFail"));

            c.GotoNext(x => x.MatchLdcI4(100));
            c.GotoNext(x => x.MatchLdarg(0));

            ILLabel skipToSlowSpinStop = c.MarkLabel();

            c.GotoPrev(x => x.MatchLdstr("HugeHit"));
            c.GotoNext(MoveType.After, x => x.MatchPop());

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, rangField);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchViHeavyThrow), "DoEnhancedHeavyThrow"));
            Utils.InsertYieldReturn(c);

            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchViHeavyThrow), "FailedEnhancedHeavyThrow"));
            c.Emit(OpCodes.Brtrue, skipToSlowSpinStop);

            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchViHeavyThrow), "SucceededEnhancedHeavyThrow"));
            c.Emit(OpCodes.Brtrue, skipToDestroy);
        }

        public static void HeavyThrow_SelfDebuffOnFail()
        {
            battle.StatusEffect(instance.playerdata[battle.currentturn], BattleCondition.DefenseDown, 2, effect: true);
        }

        const float Successful_HT_Barfill = 0.85f;
        public static int Enhanced_HT_State;

        public static bool SucceededEnhancedHeavyThrow() => Enhanced_HT_State == 1; // this sucks ass so bad
        public static bool FailedEnhancedHeavyThrow() => Enhanced_HT_State == -1; // but i don't know how to do it better
        public static IEnumerator DoEnhancedHeavyThrow(GameObject beemerang)
        {
            Enhanced_HT_State = 0;

            int targetID = battle.GetEnemyFromAvaliable(battle.avaliabletargets[battle.target]).battleentity.battleid;

            if (!BadgeIsEquipped((int)BadgeTypes.Beemerang2) || battle.barfill < Successful_HT_Barfill)
            {
                yield break;
            }

            Enhanced_HT_State = 1;

            EntityControl vi = instance.playerdata[battle.currentturn].battleentity;
            float sfxPitch = 1.2f;
            float spinMult = 1.5f;

            int damage = (instance.playerdata[battle.currentturn].atk + Mathf.FloorToInt(3f * battle.barfill)) / 2;
            int statusDuration = 1;
            int reboundsAmount = BadgeHowManyEquipped((int)BadgeTypes.Beemerang2);

            int i = targetID;
            List<int> unreachableEnemies = new List<int>();
            Vector3 viPos = vi.transform.position;
            float a;
            float b = 15f * Mathf.Lerp(2f, 1f, Mathf.Clamp(battle.barfill, 0.5f, 1f));

            while (reboundsAmount > 0)
            {
                if (battle.enemydata.Length > 1)
                {
                    i++;
                    if (i >= battle.enemydata.Length)
                    {
                        i = 0;
                    }
                }

                if (unreachableEnemies.Count == battle.enemydata.Length)
                {
                    break;
                }

                if (unreachableEnemies.Contains(i))
                {
                    continue;
                }

                if (battle.enemydata[i].position == BattlePosition.Underground ||
                    battle.enemydata[i].position == BattlePosition.OutOfReach)
                {
                    unreachableEnemies.Add(i);
                    continue;
                }

                a = 0f;
                Vector3 startPos = beemerang.transform.position;
                Vector3 targetPos = viPos + Vector3.up;
                do // return beemerang to vi
                {
                    beemerang.transform.position = Vector3.Lerp(startPos, targetPos, a / b);
                    a += MainManager.TieFramerate(1f);
                    yield return null;
                }
                while (a < b);

                Vector3 oldBeemerangScale = beemerang.transform.localScale;
                beemerang.transform.localScale = Vector3.zero;

                PlaySound("DLGammaStep");
                vi.animstate = 100;

                BattleControl_Ext.StartStylishTimer(3, 15, stylishGain: 0.5f);
                yield return new WaitForSeconds(0.25f);
                yield return new WaitUntil(() => !battle.doingaction);
                yield return BattleControl_Ext.WaitStylish(0);

                vi.animstate = 100;

                battle.StartCoroutine(battle.DoCommand(50f, ActionCommands.PressKeyTimer, new float[1] { Random.Range(4, 7) }));
                yield return new WaitUntil(() => !battle.doingaction);

                Vector3 oldSpin = vi.spin;
                a = 0;

                if (battle.commandsuccess)
                {
                    do // accelerate vi spin
                    {
                        vi.spin = Vector3.Lerp(oldSpin, oldSpin * spinMult, a / b);
                        battle.StartCoroutine(vi.ShakeSprite(0.2f * Mathf.Pow(a / b, 0.5f), 1f));
                        yield return null;
                        a += MainManager.TieFramerate(1f);
                        yield return null;
                    }
                    while (a < b);
                }
                else
                {
                    b *= 3f;
                    sfxPitch /= 2f;
                    PlaySound("Toss8", sfxPitch, 1);
                    do // spin vi out
                    {
                        vi.spin = oldSpin * Mathf.Lerp(1f, 0.4f, Mathf.Pow(a / b, 0.5f));
                        float spinoutFac = Mathf.Sin(Mathf.PI * 2f * Mathf.Clamp01(a / b));
                        vi.transform.position = new Vector3(viPos.x + spinoutFac, viPos.y, viPos.z);
                        a += MainManager.TieFramerate(1f);
                        yield return null;
                    }
                    while (a < b);
                }

                vi.animstate = 101;
                vi.sprite.transform.localPosition = Vector3.zero;
                beemerang.transform.localScale = oldBeemerangScale;
                a = 0;

                if (!battle.commandsuccess)
                {
                    sfxPitch /= 2f;
                    PlaySound("Toss8", sfxPitch, 1);
                    float offset = 30f;
                    startPos = targetPos;
                    targetPos += Vector3.right * offset;
                    do // throw beemerang clear offscreen...
                    {
                        beemerang.transform.position = BeizierCurve3(startPos, targetPos, 8f, a / b);
                        a += MainManager.TieFramerate(1f);
                        yield return null;
                    }
                    while (a < (b + 1f) / 2f);

                    targetPos = startPos;
                    startPos += Vector3.left * offset;

                    beemerang.transform.position = new Vector3(beemerang.transform.position.x, beemerang.transform.position.y - 50f, beemerang.transform.position.z);
                    yield return null;
                    beemerang.transform.position = new Vector3(startPos.x, beemerang.transform.position.y, beemerang.transform.position.z);
                    yield return null;

                    beemerang.transform.localScale = oldBeemerangScale;

                    do // ...then into vi
                    {
                        beemerang.transform.position = BeizierCurve3(startPos, targetPos, 8f, a / b);
                        a += MainManager.TieFramerate(1f);
                        yield return null;
                    }
                    while (a < b);

                    vi.overrideanim = true;
                    vi.animstate = 11;
                    damage = 3;
                    statusDuration = 2;
                    battle.DoDamage(null, ref instance.playerdata[vi.battleid], damage, null, block: false);
                    battle.StatusEffect(instance.playerdata[vi.battleid], BattleCondition.DefenseDown, statusDuration, effect: true);
                    Enhanced_HT_State = -1;
                    UnityEngine.Object.Destroy(beemerang);
                    yield break;
                }

                PlaySound("Toss8", sfxPitch, 1);
                startPos = targetPos;
                targetPos = battle.enemydata[i].battleentity.transform.position + Vector3.up * (1f + battle.enemydata[i].battleentity.height);
                do // throw beemerang at new target, and decelerate vi spin
                {
                    beemerang.transform.position = Vector3.Lerp(startPos, targetPos, a / b);
                    vi.spin = Vector3.Lerp(oldSpin * spinMult, oldSpin, a / b);
                    a += MainManager.TieFramerate(1f);
                    yield return null;
                }
                while (a < b + 1f);
                PlaySound("HugeHit", sfxPitch, 1);
                ShakeScreen(0.3f, 0.55f);

                battle.DoDamage(instance.playerdata[vi.battleid], ref battle.enemydata[i], damage, null, new DamageOverride[1] { DamageOverride.NoSound }, block: false);
                if (statusDuration > 0)
                {
                    battle.StatusEffect(battle.enemydata[i], BattleCondition.DefenseDown, statusDuration, effect: true);
                }

                damage /= 2;
                statusDuration /= 2;

                reboundsAmount--;
            }

            viPos += Vector3.up;
            a = 0f;
            b = 80f;
            Vector3 lastRangPos = beemerang.transform.position;
            bool didCommand = false;
            do
            {
                beemerang.transform.position = BeizierCurve3(lastRangPos, viPos, 15f, a / b);
                if (!didCommand && a / b > 0.4f)
                {
                    battle.StartCoroutine(battle.DoCommand(60f, ActionCommands.PressKeyTimer, new float[1] { Random.Range(4, 7) }));
                    didCommand = true;
                }
                a += MainManager.TieFramerate(1f);
                yield return null;
            }
            while (a < b);
        }

    }

    /// <summary>
    /// Change properties; now pierces 3 def and double-flips
    /// </summary>
    public class PatchViFlyDrop : PatchBaseDoAction
    {
        public PatchViFlyDrop()
        {
            priority = 55723;
        }

        protected override void ApplyPatch(ILCursor c, ILContext context)
        {
            c.GotoNext(x => x.MatchLdstr("HugeHit"));
            c.GotoNext(MoveType.After, x => x.MatchPop());
            int returnHere = c.Index;
            c.GotoNext(x => x.MatchCall(AccessTools.Method(typeof(BattleControl), "DoDamage", new Type[] { typeof(BattleData?), typeof(BattleData).MakeByRefType(), typeof(int), typeof(AttackProperty?), typeof(DamageOverride[]), typeof(bool) })));
            c.GotoNext(x => x.MatchLdarg(0));

            ILLabel skipToLabel = c.MarkLabel();

            c.Goto(returnHere);

            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchViFlyDrop), "FlyDropDoDamage"));
            c.Emit(OpCodes.Br, skipToLabel);
        }

        static DamageOverride[] FlyDropOverrides()
        {
            if (!MainManager_Ext.Instance.GetBalanceChangeState((int)NewMenuText.FlyDrop))
            {
                return new DamageOverride[]
                {
                    (DamageOverride)NewDamageOverride.ExtraAirTopple
                };
            }

            return new DamageOverride[]
            {
                (DamageOverride)NewDamageOverride.ExtraAirTopple,
                (DamageOverride)NewDamageOverride.FlipNoPierce,
                (DamageOverride)NewDamageOverride.FlipNoPierce,
                (DamageOverride)NewDamageOverride.Pierce1,
                (DamageOverride)NewDamageOverride.Pierce1,
                (DamageOverride)NewDamageOverride.Pierce1,
            };
        }
        static void FlyDropDoDamage()
        {
            AttackProperty? property = null;
            if(MainManager_Ext.Instance.GetBalanceChangeState((int)NewMenuText.FlyDrop))
                property = AttackProperty.Pierce;

            int targetID = battle.GetEnemyFromAvaliable(battle.avaliabletargets[battle.target]).battleentity.battleid;
            int damage = 3 + Mathf.FloorToInt(battle.barfill * 2f) + battle.GetMultiDamage(new int[2] { 0, 1 });

            battle.DoDamage(null, ref battle.enemydata[targetID], damage, property, FlyDropOverrides(), false);
            if (battle.barfill >= 1)
            {
                int dizzyTurns = BadgeHowManyEquipped((int)Medal.HornRattle);
                if (dizzyTurns > 0)
                    BattleControl_Ext.Instance.TryDizzy(null, ref battle.enemydata[targetID], dizzyTurns + 1, 15);
            }
        }
    }
}