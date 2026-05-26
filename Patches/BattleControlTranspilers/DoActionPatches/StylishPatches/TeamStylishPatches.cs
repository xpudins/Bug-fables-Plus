using BFPlus.Extensions;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.DoActionPatches.StylishPatches
{
    public class PatchRoyalDecreeSetStylish : PatchBaseDoAction
    {
        public PatchRoyalDecreeSetStylish()
        {
            priority = 62423;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(197));
            cursor.GotoNext(MoveType.After, i => i.MatchBlt(out _), i => i.MatchLdarg0());
            cursor.Prev.OpCode = OpCodes.Nop;
            Utils.InsertStartStylishTimer(cursor, 3f, 15f, stylishGain: 0.12f, commandSuccess: false);
            cursor.Emit(OpCodes.Ldarg_0);
        }
    }
}
