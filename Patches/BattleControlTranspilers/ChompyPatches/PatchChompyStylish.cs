using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.ChompyPatches
{
    public class PatchChompyStylish : PatchBaseChompy
    {
        public PatchChompyStylish()
        {
            priority = 41401;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcR4(0.4f));
            cursor.Prev.OpCode = OpCodes.Nop;
            Utils.InsertStartStylishTimer(cursor, 10f, 20f);
            cursor.Emit(OpCodes.Ldarg_0);

            cursor.GotoNext(i => i.MatchLdloc1());
            cursor.Emit(OpCodes.Ldarg_0);
            Utils.InsertWaitStylish(cursor);
        }
    }
}
