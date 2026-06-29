using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.StartBattlePatches
{
    public class PatchLevelChecks : PatchBaseStartBattle
    {
        public PatchLevelChecks()
        {
            priority = 2171;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, j => j.MatchLdfld(AccessTools.Field(typeof(MainManager), "partylevel")));
            cursor.Emit(OpCodes.Ldc_I4, MainManager_Ext.newMaxLevel);
            cursor.Remove();

            cursor.GotoNext(MoveType.After, j => j.MatchLdfld(AccessTools.Field(typeof(MainManager), "partylevel")), i => i.MatchConvR4());
            cursor.Emit(OpCodes.Ldc_R4, (float)MainManager_Ext.newMaxLevel);
            cursor.Remove();
        }
    }
}
