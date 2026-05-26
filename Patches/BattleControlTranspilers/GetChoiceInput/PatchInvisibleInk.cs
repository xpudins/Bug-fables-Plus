using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.GetChoiceInput
{
    public class PatchInvisibleInk : PatchBaseGetChoiceInput
    {
        public PatchInvisibleInk()
        {
            priority = 216;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(18));
            Utils.RemoveUntilInst(cursor, i => i.MatchBneUn(out _));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "CantUseSkillInked"));
            cursor.Next.OpCode = OpCodes.Brfalse;
        }

    }
}
