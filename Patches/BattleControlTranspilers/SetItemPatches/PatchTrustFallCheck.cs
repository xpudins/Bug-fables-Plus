using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.SetItemPatches
{
    public class PatchTrustFallCheck : PatchBaseSetItem
    {
        public PatchTrustFallCheck()
        {
            priority = 191414;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchSwitch(out _));
            var label = cursor.DefineLabel();
            var jumpLabel = cursor.Body.Instructions[cursor.Index + 2].Operand;

            cursor.Emit(OpCodes.Ldarg_1);
            cursor.Emit(OpCodes.Ldc_I4, (int)NewMenuText.TrustFall);
            cursor.Emit(OpCodes.Bne_Un, label);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "DoTrustFall"));
            cursor.Emit(OpCodes.Br, jumpLabel);
            cursor.MarkLabel(label);
        }

    }
}
