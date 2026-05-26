using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace BFPlus.Patches.BattleControlTranspilers.UseItemPatches
{
    public class PatchMultiUseFix : PatchBaseUseItem
    {
        public PatchMultiUseFix()
        {
            priority = 19220;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;
            cursor.GotoNext(MoveType.After,
                i => i.MatchCall(AccessTools.Method(typeof(MainManager), "BadgeIsEquipped", new Type[] { typeof(int), typeof(int) })),
                i => i.MatchBrfalse(out label));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "CanReUseItem"));
            cursor.Emit(OpCodes.Brfalse, label);
        }

    }
}
