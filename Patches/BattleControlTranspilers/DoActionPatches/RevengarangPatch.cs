using BFPlus.Extensions;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.DoActionPatches
{
    public class PatchCheckRevengarang : PatchBaseDoAction
    {
        public PatchCheckRevengarang()
        {
            priority = 150484;

        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {

            cursor.GotoNext(i => i.MatchCall(AccessTools.Method(typeof(BattleControl), "EndPlayerTurn")));
            cursor.GotoNext(MoveType.After, i => i.MatchBr(out _));
            cursor.Next.OpCode = OpCodes.Nop;
            cursor.GotoNext();
            cursor.GotoNext();
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "CheckRevengarang"));
            Utils.InsertYieldReturn(cursor);
            cursor.Emit(OpCodes.Ldloc_1);
        }
    }
}
