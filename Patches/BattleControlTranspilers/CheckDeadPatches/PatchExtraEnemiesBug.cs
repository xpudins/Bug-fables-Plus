using BFPlus.Extensions;
using BFPlus.Extensions.BattleStuff.StatusStuff;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Linq;
using UnityEngine;

namespace BFPlus.Patches.BattleControlTranspilers.CheckDeadPatches
{
    public class PatchExtraEnemiesBug : PatchBaseCheckDead
    {
        public PatchExtraEnemiesBug()
        {
            priority = 156353;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchCall(AccessTools.Method(typeof(BattleControl), nameof(BattleControl.SummonEnemy), new Type[] { typeof(BattleControl.SummonType), typeof(int), typeof(Vector3) })));
            int returnHere = cursor.Index;
            cursor.GotoPrev(i => i.MatchLdfld(out _));
            object deadEnemyPosIndex = cursor.Next.Operand;
            cursor.Goto(returnHere);

            cursor.Remove();
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, deadEnemyPosIndex);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchExtraEnemiesBug), nameof(ShouldEnemyNotMove)));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl), nameof(BattleControl.SummonEnemy), new Type[] { typeof(BattleControl.SummonType), typeof(int), typeof(Vector3), typeof(bool) }));

            cursor.GotoNext(i => i.MatchCall(AccessTools.Method(typeof(BattleControl), "EnemiesAreNotMoving")));
            cursor.GotoNext(MoveType.After, i => i.MatchStfld(AccessTools.Field(typeof(BattleControl), "action")));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchExtraEnemiesBug), "ClearCleanKillPos"));
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Ldc_I4_1);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl), "ReorganizeEnemies", new Type[] { typeof(bool) }));
        }

        // in wave battles, keeps newly-added enemies from immediately attacking when an enemy dies to dizzy recoil
        static bool ShouldEnemyNotMove(int deadEnemyPosIndex)
        {
            return Dizzy.dizzyKOpositions.Where(d => Vector3.Distance(d, MainManager.battle.deadenemypos[deadEnemyPosIndex]) <= 0.01f).Any();
        }

        static void ClearCleanKillPos()
        {
            Dizzy.dizzyKOpositions.Clear();
            BattleControl_Ext.Instance.cleanKilledEnemyPos.Clear();
        }
    }
}
