using BFPlus.Extensions;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
namespace BFPlus.Patches.DoActionPatches
{
    public class PatchHardChargeMaxAmount : PatchBaseDoAction
    {
        public PatchHardChargeMaxAmount()
        {
            priority = 51202;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(81));
            cursor.GotoNext(i => i.MatchLdcI4(3), i => i.MatchStfld(typeof(MainManager.BattleData).GetField("charge")));

            cursor.Remove();
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(cursor.Body.Instructions[cursor.Index - 3].OpCode, cursor.Body.Instructions[cursor.Index - 3].Operand);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "CheckMaxCharge"));
        }
    }
}
