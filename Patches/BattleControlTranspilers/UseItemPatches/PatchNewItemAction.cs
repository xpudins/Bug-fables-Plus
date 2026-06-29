using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.UseItemPatches
{
    public class PatchNewItemAction : PatchBaseUseItem
    {
        public PatchNewItemAction()
        {
            priority = 18348;
        }

        protected override void ApplyPatch(ILCursor c, ILContext context)
        {
            c.GotoNext(i => i.MatchLdcI4(13));
            c.GotoNext(i => i.OpCode == OpCodes.Ldc_I4_M1);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "CheckNewItemAction"));
            c.Remove();

            // skips the actionID reassign for most vanilla attack items, so CheckNewItemAction doesn't get overwritten
            ILLabel label = null;
            c.GotoNext(x => x.MatchLdcI4(9),
                       x => x.MatchStfld(out _),
                       x => x.MatchBr(out label));

            OpCode prevOp = c.Prev.OpCode;
            var prevOperand = c.Prev.Operand;

            c.Prev.OpCode = OpCodes.Nop;
            c.Emit(OpCodes.Br, label);
            c.Emit(prevOp, prevOperand);
        }

    }
}