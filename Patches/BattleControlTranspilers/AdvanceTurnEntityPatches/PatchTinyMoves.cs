using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.AdvanceTurnEntityPatches
{

    public class PatchTinyMoves : PatchBaseAdvanceTurnEntity
    {
        public PatchTinyMoves()
        {
            priority = 848;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdarg1(), i => i.MatchLdfld(AccessTools.Field(typeof(MainManager.BattleData), "moreturnnextturn")));
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchTinyMoves), "CheckTinyMoves"));
        }

        static void CheckTinyMoves(ref MainManager.BattleData target)
        {
            if (MainManager.HasCondition((MainManager.BattleCondition)NewCondition.Tiny, target) > -1)
                target.cantmove--;
        }
    }
}
