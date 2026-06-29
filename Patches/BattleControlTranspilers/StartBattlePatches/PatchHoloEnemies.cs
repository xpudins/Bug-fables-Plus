using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.StartBattlePatches
{
    public class PatchHoloEnemies : PatchBaseStartBattle
    {
        public PatchHoloEnemies()
        {
            priority = 1659;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            for (int i = 0; i < 2; i++)
            {
                cursor.GotoNext(j => j.MatchLdsfld(out _), j => j.MatchLdfld(out _), j => j.MatchLdcI4(162));
                cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "IsHolo"));
                cursor.RemoveRange(4);
            }
        }
    }
}
