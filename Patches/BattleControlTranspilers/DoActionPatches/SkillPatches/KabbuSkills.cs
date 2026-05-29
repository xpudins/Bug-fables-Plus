using BFPlus.Extensions;
using BFPlus.Extensions.Events;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BattleControl;
using static MainManager;

namespace BFPlus.Patches.BattleControlTranspilers.DoActionPatches.SkillPatches
{
    /// <summary>
    /// Forget replacing the Heavy Strike GetPlayerData calls; just replace the whole damn DoDamage call
    /// </summary>
    public class PatchHeavyStrike : PatchBaseDoAction
    {
        public PatchHeavyStrike()
        {
            priority = 45134;
        }

        protected override void ApplyPatch(ILCursor c, ILContext context)
        {
            c.GotoNext(x => x.MatchLdloc(1),
                       x => x.MatchLdcI4(1));
            int returnHere = c.Index;
            c.GotoNext(MoveType.After, x => x.MatchPop());
            ILLabel skipToLabel = c.MarkLabel();
            c.Goto(returnHere);

            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchHeavyStrike), "HeavyStrikeDoDamage"));
            c.Emit(OpCodes.Br, skipToLabel);
        }

        static void HeavyStrikeDoDamage()
        {
            int userID = battle.currentturn;
            int targetID = battle.GetEnemyFromAvaliable(battle.avaliabletargets[battle.target]).battleentity.battleid;
            int damage = instance.playerdata[userID].atk + 2;
            battle.DoDamage(instance.playerdata[userID], ref battle.enemydata[targetID], damage, AttackProperty.Flip, new DamageOverride[1] { DamageOverride.NoSound }, false);
            
            int dizzyTurns = BadgeHowManyEquipped((int)Medal.HornRattle);
            if (dizzyTurns > 0)
                BattleControl_Ext.Instance.TryDizzy(instance.playerdata[userID], ref battle.enemydata[targetID], dizzyTurns + 1);
        }
    }

    // <summary>
    /// adds Horn Rattle boost
    /// </summary>
    public class PatchKabbuDashThrough : PatchBaseDoAction
    {
        public PatchKabbuDashThrough()
        {
            priority = 60305;
        }

        protected override void ApplyPatch(ILCursor c, ILContext context)
        {
            c.GotoNext(x => x.MatchLdstr("FastWoosh"));
            c.GotoNext(x => x.MatchCall(AccessTools.Method(typeof(BattleControl), "DoDamage", new Type[] { typeof(BattleData?), typeof(BattleData).MakeByRefType(), typeof(int), typeof(AttackProperty?) })));
            c.GotoNext(MoveType.After, x => x.MatchPop());
            ILLabel skipToLabel = c.MarkLabel();

            c.GotoPrev(x => x.MatchLdfld(out _));
            object dmgRef = c.Next.Operand;

            c.GotoPrev(x => x.MatchLdloc(out _));
            object targetIdRef = c.Next.Operand;

            c.GotoPrev(x => x.MatchLdsfld(out _));

            OpCode preOp = c.Prev.OpCode;
            var preOperand = c.Prev.Operand;
            c.Prev.OpCode = OpCodes.Nop;

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, dmgRef);
            c.Emit(OpCodes.Ldloc, targetIdRef);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchKabbuDashThrough), "DashthroughDoDamage"));
            c.Emit(OpCodes.Br, skipToLabel);
            c.Emit(preOp, preOperand);
        }

        static DamageOverride[] DashthroughOverrides()
        {
            List<DamageOverride> overrides = new List<DamageOverride>()
            {
                (DamageOverride)NewDamageOverride.FlipNoPierce,
                (DamageOverride)NewDamageOverride.Pierce1
            };
            return overrides.ToArray();
        }
        static void DashthroughDoDamage(int damage, int targetID)
        {
            battle.DoDamage(instance.playerdata[battle.currentturn], ref battle.enemydata[targetID], damage, null, DashthroughOverrides(), false);
            if (battle.barfill >= 1)
            {
                int dizzyTurns = BadgeHowManyEquipped((int)Medal.HornRattle);
                if (dizzyTurns > 0)
                    BattleControl_Ext.Instance.TryDizzy(instance.playerdata[battle.currentturn], ref battle.enemydata[targetID], dizzyTurns + 1);
            }
        }
    }

    /// <summary>
    /// Gootbye, BattleControl_Ext.Instance.damageMultiplier
    /// </summary>
    public class PatchUnderstrike : PatchBaseDoAction
    {
        public PatchUnderstrike()
        {
            priority = 61659;
        }

        protected override void ApplyPatch(ILCursor c, ILContext context)
        {
            c.GotoNext(MoveType.After, x => x.MatchCall(AccessTools.Method(typeof(BattleControl), "FlipWithinPos")));
            ILLabel skipToLabel = c.MarkLabel();
            c.GotoPrev(MoveType.After, x => x.MatchPop());

            ILLabel label = c.DefineLabel();
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchUnderstrike), nameof(NewUnderstrike)));
            c.Emit(OpCodes.Brfalse, label);
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchUnderstrike), "UnderstrikeDoDamage"));
            c.Emit(OpCodes.Br, skipToLabel);
            c.MarkLabel(label);

            c.GotoNext(MoveType.After,
                       x => x.MatchLdcR4(0.85f),
                       x => x.MatchCall(out _));
            int returnHere = c.Index;
            c.GotoNext(MoveType.After, x => x.MatchCall(AccessTools.Method(typeof(BattleControl), "DamageWithinPos", new Type[] { typeof(int), typeof(int), typeof(AttackProperty?), typeof(float), typeof(bool) })));
            skipToLabel = c.MarkLabel();
            c.Goto(returnHere);

            label = c.DefineLabel();
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchUnderstrike), nameof(NewUnderstrike)));
            c.Emit(OpCodes.Brfalse, label);
            c.Emit(OpCodes.Ldc_I4_1);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchUnderstrike), "UnderstrikeDoDamage"));
            c.Emit(OpCodes.Br, skipToLabel);
            c.MarkLabel(label);
        }

        static int?[] targetDamages;
        static DamageOverride[] UnderstrikeOverrides(int hit, int dmg)
        {
            List<DamageOverride> overrides = new List<DamageOverride>()
            {
                (DamageOverride)NewDamageOverride.Pierce1
            };

            if (hit == 0)
                overrides.Add((DamageOverride)NewDamageOverride.FlipNoPierce);

            if (dmg <= 0)
            {
                overrides.Add(DamageOverride.NoCounter);
                overrides.Add(DamageOverride.NoDamageAnim);
                overrides.Add(DamageOverride.NoSound);
            }
            return overrides.ToArray();
        }
        static void UnderstrikeDoDamage(int hit)
        {
            if (targetDamages == null)
                targetDamages = new int?[battle.enemydata.Length];

            int userID = battle.currentturn;
            Vector3 userPos = instance.playerdata[userID].battleentity.transform.position;

            int baseDamage = Mathf.Clamp(instance.playerdata[userID].atk, 1, 99);
            DamageOverride[] overrides;
            for (int i = 0; i < battle.enemydata.Length; i++)
            {
                if (battle.enemydata[i].position == BattlePosition.OutOfReach 
                    || !battle.IsInRadius(userPos, battle.enemydata[i], 2f, true, true) 
                    || battle.enemydata[i].hp <= 0)
                    continue;

                if (targetDamages[i] == null)
                {
                    overrides = UnderstrikeOverrides(hit, baseDamage);
                    targetDamages[i] = battle.DoDamage(instance.playerdata[userID], ref battle.enemydata[i], baseDamage, AttackProperty.Atleast1, overrides, false);
                }
                else
                {
                    AttackProperty property = AttackProperty.NoExceptions;
                    if (targetDamages[i].Value > 1)
                    {
                        targetDamages[i] = Mathf.FloorToInt(targetDamages[i].Value / 2f);
                    }
                    overrides = UnderstrikeOverrides(hit, targetDamages[i].Value);
                    battle.DoDamage(instance.playerdata[userID], ref battle.enemydata[i], targetDamages[i].Value, property, overrides, false);
                }
            }

            if (hit == 1)
                targetDamages = null;
        }

        static bool NewUnderstrike() => MainManager_Ext.Instance.GetBalanceChangeState((int)NewMenuText.Understrike);
    }

    // <summary>
    /// Add atk down to main hit of boulder toss; instead self-inflicts if AC is failed
    /// Also add Avalance to smaller rocks
    /// </summary>
    public class PatchBoulderToss : PatchBaseDoAction
    {
        public PatchBoulderToss()
        {
            priority = 50883;
        }

        protected override void ApplyPatch(ILCursor c, ILContext context)
        {
            c.GotoNext(x => x.MatchCall(AccessTools.Method(typeof(BattleControl), "DoDamage", new Type[] { typeof(BattleData?), typeof(BattleData).MakeByRefType(), typeof(int), typeof(AttackProperty?), typeof(bool) })));
            c.GotoPrev(MoveType.After, x => x.MatchLdfld<BattleControl>(nameof(BattleControl.enemydata)));
            c.GotoNext(x => x.MatchLdfld(out _));

            Instruction targetID = c.Next;

            c.GotoNext(MoveType.After, x => x.MatchPop());

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(targetID.OpCode, targetID.Operand);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchBoulderToss), "StatusEffects_MainTarget"));

            c.GotoNext(x => x.MatchCall(AccessTools.Method(typeof(BattleControl), "LateDamage")));
            c.GotoPrev(x => x.MatchLdloc(out _));

            targetID = c.Next;

            c.GotoNext(MoveType.After, x => x.MatchPop());

            c.Emit(targetID.OpCode, targetID.Operand);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchBoulderToss), "StatusEffects_CollateralTargets"));

            c.GotoNext(x => x.MatchRet());
            c.GotoNext(MoveType.After, x => x.MatchStfld(out _));

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchBoulderToss), "WaitForAvalanche"));
            Utils.InsertYieldReturn(c);

            c.GotoNext(x => x.MatchCall(AccessTools.Method(typeof(BattleControl), "DoDamage", new Type[] { typeof(BattleData).MakeByRefType(), typeof(int) })));
            c.GotoNext(MoveType.After, x => x.MatchPop());

            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchBoulderToss), "StatusEffects_Self"));
        }

        static List<GameObject> icecles;

        public static void StatusEffects_MainTarget(int targetID)
        {
            if (battle.combo >= 3)
            {
                battle.StatusEffect(battle.enemydata[targetID], BattleCondition.AttackDown, 2, true, false);
            }
            if (battle.combo >= 5)
            {
                int dizzyTurns = BadgeHowManyEquipped((int)Medal.HornRattle);
                if (dizzyTurns > 0)
                    BattleControl_Ext.Instance.TryDizzy(null, ref battle.enemydata[targetID], dizzyTurns + 1);
            }
        }
        public static void StatusEffects_CollateralTargets(int targetID)
        {
            if (battle.combo >= 3 && BadgeIsEquipped((int)Medal.Avalanche))
            {
                battle.StartCoroutine(DelayedAvalanche(targetID, 0.5f));
            }
        }
        static IEnumerator DelayedAvalanche(int targetID, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (icecles == null)
                icecles = new List<GameObject>();
            BattleControl_Ext.Instance.CheckAvalanche(battle, targetID, icecles);
        }
        static IEnumerator WaitForAvalanche()
        {
            if (icecles != null)
            {
                yield return new WaitUntil(() => ArrayIsEmpty(icecles.ToArray()));
                icecles = null;
            }
        }

        public static void StatusEffects_Self()
        {
            battle.StatusEffect(instance.playerdata[battle.currentturn], BattleCondition.AttackDown, 2, true, false);
            int dizzyTurns = BadgeHowManyEquipped((int)Medal.HornRattle);
            if (dizzyTurns > 0)
                BattleControl_Ext.Instance.TryDizzy(null, ref instance.playerdata[battle.currentturn], dizzyTurns + 1);
        }
    }

    /// <summary>
    /// make pierce lower but apply to all hits, and add magic property
    /// also only flips twice, instead of with every hit
    /// </summary>
    public class PatchKabbuFrozenDrill : PatchBaseDoAction
    {
        public PatchKabbuFrozenDrill()
        {
            priority = 55017;
        }

        protected override void ApplyPatch(ILCursor c, ILContext context)
        {
            var doDamageRef = AccessTools.Method(typeof(BattleControl), "DoDamage", new Type[] 
            { 
                typeof(BattleData?), typeof(BattleData).MakeByRefType(), typeof(int), typeof(AttackProperty?), typeof(bool) 
            });

            c.GotoNext(x => x.MatchLdarg(0),
                       x => x.MatchLdcI4(109));

            c.GotoPrev(x => x.MatchCall(doDamageRef));
            c.GotoPrev(x => x.MatchCall(doDamageRef));
            c.GotoNext(MoveType.After, x => x.MatchPop());

            ILLabel skipLabel = c.MarkLabel();

            c.GotoPrev(x => x.MatchLdfld(out _));

            object dmgRef = c.Next.Operand;

            int prevOperand = -1;
            c.GotoPrev(x => x.MatchLdloca(out prevOperand));

            c.Prev.OpCode = OpCodes.Nop;
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, dmgRef);
            c.Emit(OpCodes.Ldc_I4_3);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchKabbuFrozenDrill), "FrozenDrillDoDamage"));
            c.Emit(OpCodes.Br, skipLabel);
            c.Emit(OpCodes.Ldloca, prevOperand);

            c.GotoPrev(MoveType.After, x => x.MatchCall(AccessTools.Method(typeof(BattleControl), "Flip")));
            skipLabel = c.MarkLabel();

            c.GotoNext(x => x.MatchLdfld(out _));
            object hitCountRef = c.Next.Operand;

            c.GotoPrev(x => x.MatchCall(AccessTools.Method(typeof(BattleControl), "DoDamage", new Type[] { typeof(BattleData?), typeof(BattleData).MakeByRefType(), typeof(int), typeof(AttackProperty?) })));
            c.GotoPrev(MoveType.After, x => x.MatchPop());

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, dmgRef);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, hitCountRef);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchKabbuFrozenDrill), "FrozenDrillDoDamage"));
            c.Emit(OpCodes.Br, skipLabel);
        }

        static DamageOverride[] FrozenDrillOverrides(int hit)
        {
            List<DamageOverride> overrides = new List<DamageOverride>()
            {
                (DamageOverride)NewDamageOverride.Magic,
                (DamageOverride)NewDamageOverride.Pierce1,
                (DamageOverride)NewDamageOverride.Pierce1
            };
            if (hit == 0 || hit == 3)
                overrides.Add((DamageOverride)NewDamageOverride.FlipNoPierce);
            return overrides.ToArray();
        }
        static void FrozenDrillDoDamage(int damage, int hit)
        {
            int targetID = battle.GetEnemyFromAvaliable(battle.avaliabletargets[battle.target]).battleentity.battleid;
            AttackProperty property = (hit == 3) ? AttackProperty.Freeze : AttackProperty.None;
            battle.DoDamage(null, ref battle.enemydata[targetID], damage + 1, property, FrozenDrillOverrides(hit), false);
        }
    }
}
