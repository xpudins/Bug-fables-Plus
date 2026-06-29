using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.PlayerControlTranspilers
{

    public class PatchCancelActionDashAnim : PatchBasePlayerControlCancelAction
    {
        public PatchCancelActionDashAnim()
        {
            priority = 61;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;
            cursor.GotoNext(MoveType.After, i => i.MatchLdfld(AccessTools.Field(typeof(PlayerControl), "flying")), i => i.MatchBrtrue(out label));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerControl), "dashing"));
            cursor.Emit(OpCodes.Brtrue, label);
        }
    }
}
