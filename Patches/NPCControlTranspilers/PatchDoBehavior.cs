using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.NPCControlTranspilers
{

    public class PatchMoveAwayState : PatchBaseNPCControlDoBehavior
    {
        public PatchMoveAwayState()
        {
            priority = 690;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdfld(AccessTools.Field(typeof(PlayerControl), "tattling")));

            cursor.GotoNext(i => i.MatchLdcI4(1));
            cursor.Next.Operand = (int)MainManager.Animations.Chase;
            cursor.Next.OpCode = OpCodes.Ldc_I4;
        }
    }
}
