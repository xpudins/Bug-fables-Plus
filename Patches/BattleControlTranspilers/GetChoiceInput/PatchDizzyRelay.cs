using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.GetChoiceInput
{
    public class PatchDizzyRelay : PatchBaseGetChoiceInput
    {
        public PatchDizzyRelay()
        {
            priority = 350;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;
            cursor.GotoNext(i => i.MatchLdfld(AccessTools.Field(typeof(MainManager.BattleData), "locktri")));
            cursor.GotoNext(MoveType.After, i => i.MatchBneUn(out label));

            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "CantUseRelayDizzy"));
            cursor.Emit(OpCodes.Brtrue, label);
        }

    }
}
