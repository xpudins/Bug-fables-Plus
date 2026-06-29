using BFPlus.Extensions;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
namespace BFPlus.Patches.DoActionPatches
{
    public class PatchKabbuPebbleToss : PatchBaseDoAction
    {
        public PatchKabbuPebbleToss()
        {
            priority = 52083;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(88));
            cursor.GotoNext(i => i.MatchBr(out _));

            var breakLabel = cursor.Next.Operand;
            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(28), i => i.MatchStfld(out _));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "DoPebbleToss"));
            Utils.InsertYieldReturn(cursor);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "IsUsingItem"));
            cursor.Emit(OpCodes.Brfalse, breakLabel);
        }
    }
}
