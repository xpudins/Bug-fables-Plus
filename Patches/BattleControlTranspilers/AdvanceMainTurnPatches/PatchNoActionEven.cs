using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.AdvanceMainTurnPatches
{
    public class PatchNoActionEven : PatchBaseAdvanceMainTurn
    {
        public PatchNoActionEven()
        {
            priority = 11358;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;
            cursor.GotoNext(MoveType.After, i => i.MatchLdfld(AccessTools.Field(typeof(BattleControl), "noaction")),
                i => i.MatchLdcI4(5), i => i.MatchBneUn(out label));

            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(BattleControl), "action"));
            cursor.Emit(OpCodes.Brtrue, label);
        }

    }
}
