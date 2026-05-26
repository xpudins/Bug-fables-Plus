using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.AddExperiencePatches
{
    public class PatchLevelCap : PatchBaseAddExperience
    {
        public PatchLevelCap()
        {
            priority = 12650;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            int cursorIndex = cursor.Index;
            for (int i = 0; i < 3; i++)
            {
                cursor.GotoNext(j => j.MatchLdfld(out _), j => j.MatchLdcI4(27));
                cursor.GotoNext();
                cursor.Emit(OpCodes.Ldc_I4, MainManager_Ext.newMaxLevel);
                cursor.Remove();
            }
            cursor.Goto(cursorIndex);
        }

    }
}
