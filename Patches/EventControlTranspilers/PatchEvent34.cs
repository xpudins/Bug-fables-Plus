using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.EventControlTranspilers
{
    //Chuck event

    /// <summary>
    /// Changes quest reward for grumble gravel
    /// </summary>
    public class PatchChuckQuestReward : PatchBaseEvent34
    {
        public PatchChuckQuestReward()
        {
            priority = 34811;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(13), i => i.MatchBox(out _));
            cursor.Emit(OpCodes.Ldc_I4, (int)Medal.GrumbleGravel);
            cursor.Remove();
        }
    }
}
