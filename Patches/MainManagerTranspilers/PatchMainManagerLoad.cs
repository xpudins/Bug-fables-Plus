using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.MainManagerTranspilers
{
    public class PatchFlagVar : PatchBaseMainManagerLoad
    {
        public PatchFlagVar()
        {
            priority = 1349;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(70));
            cursor.Next.OpCode = OpCodes.Ldc_I4;
            cursor.Next.Operand = MainManager_Ext.FlagVarNumber;
        }
    }
}
