using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.SetItemPatches
{

    public class PatchCheckHoloSkill : PatchBaseSetItem
    {
        public PatchCheckHoloSkill()
        {
            priority = 191596;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(6), i => i.MatchBneUn(out _));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "CheckHoloSkill"));
            Utils.RemoveUntilInst(cursor, i => i.MatchBr(out _));
        }

    }
}
