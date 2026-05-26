using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.AddExperiencePatches
{
    /// <summary>
    /// Make sure to reset the max hp BEFORE there is any lvl up shenanigans
    /// </summary>
    public class PatchResetMaxHP : PatchBaseAddExperience
    {
        public PatchResetMaxHP()
        {
            priority = 12458;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(70));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchResetMaxHP), "ResetMaxHP"));
        }

        static void ResetMaxHP()
        {
            MainManager.ApplyBadges();
            MainManager.ApplyStatBonus();
        }
    }
}
