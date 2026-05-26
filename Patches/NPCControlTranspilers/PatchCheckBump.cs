using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.NPCControlTranspilers
{
    /// <summary>
    /// replace max level check by our nex max level
    /// </summary>
    public class PatchBumpMaxLevel : PatchBaseNPCControlCheckBump
    {
        public PatchBumpMaxLevel()
        {
            priority = 0;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            for (int j = 0; j < 2; j++)
            {
                cursor.GotoNext(i => i.MatchLdcI4(27));
                cursor.Emit(OpCodes.Ldc_I4, MainManager_Ext.newMaxLevel);
                cursor.Remove();
            }
        }
    }
}
