using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace BFPlus.Patches.BattleControlTranspilers.UpdateAnimPatches
{
    public class PatchVengeanceCheck : PatchBaseUpdateAnim
    {
        public PatchVengeanceCheck()
        {
            priority = 241;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;
            cursor.GotoNext(MoveType.After,
                i => i.MatchCall(AccessTools.Method(typeof(MainManager), "BadgeIsEquipped", new Type[] { typeof(int), typeof(int) })),
                i => i.MatchBrtrue(out label));
            cursor.Emit(OpCodes.Ldloc_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "CheckVengeance"));
            cursor.Emit(OpCodes.Brtrue, label);
        }

    }
}
