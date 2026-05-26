using BFPlus.Patches.DoActionPatches;
using BFPlus.Extensions;
using BFPlus.Extensions.BattleStuff.StatusStuff;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace BFPlus.Patches.BattleControlTranspilers.CheckDeadPatches
{
    public class PatchWaitForPlayerSpinout : PatchBaseCheckDead
    {
        public PatchWaitForPlayerSpinout()
        {
            priority = 155790;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, x => x.MatchCall(AccessTools.Method(typeof(MainManager), nameof(MainManager.GetAlivePlayerAmmount))));

            cursor.Prev.OpCode = OpCodes.Nop;

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldc_I4_1);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(Dizzy), "WaitForDizzyKO"));
            Utils.InsertYieldReturn(cursor);

            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager), nameof(MainManager.GetAlivePlayerAmmount)));
        }
    }
}