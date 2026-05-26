using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.StartBattlePatches
{
    public class PatchCheckCustomAI : PatchBaseStartBattle
    {
        public PatchCheckCustomAI()
        {
            priority = 2932;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After,
                i => i.MatchLdnull(),
                i => i.MatchStfld(AccessTools.Field(typeof(BattleControl), "aiparty")));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "CheckCustomAI"));
        }

    }
}
