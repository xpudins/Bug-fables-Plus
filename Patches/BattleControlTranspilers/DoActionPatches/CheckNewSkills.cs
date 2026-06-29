using BFPlus.Extensions;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.DoActionPatches
{
    public class PatchCheckNewSkills : PatchBaseDoAction
    {
        public PatchCheckNewSkills()
        {
            priority = 44427;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After,
                i => i.MatchLdfld(typeof(BattleControl).GetField("checkingdead")),
                i => i.MatchBrtrue(out _));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "CheckNewSkills"));
            Utils.InsertYieldReturn(cursor);
        }

    }
}
