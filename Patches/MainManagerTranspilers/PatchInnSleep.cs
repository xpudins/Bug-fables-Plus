using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.MainManagerTranspilers
{
    /// <summary>
    /// Check if they slept here before for well rested achievement
    /// </summary>
    public class PatchInnSleep : PatchBaseInnSleep
    {
        public PatchInnSleep()
        {
            priority = 0;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdcR4(-1), i => i.MatchStfld(out _));
            cursor.Emit(OpCodes.Ldc_I4_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "CheckWellRestedAchievement"));
        }
    }
}
