using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.GetChoiceInput
{
    public class PatchStatusInfoKey : PatchBaseGetChoiceInput
    {
        public PatchStatusInfoKey()
        {
            priority = 477;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = cursor.DefineLabel();
            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(6));
            cursor.Prev.OpCode = OpCodes.Nop;

            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchStatusInfoKey), "CheckPressStatusKey"));
            cursor.Emit(OpCodes.Brfalse, label);

            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchStatusInfoKey), "CallShowStatusInfo"));
            cursor.Emit(OpCodes.Ret);


            cursor.MarkLabel(label);
            cursor.Emit(OpCodes.Ldc_I4, 6);
        }

        static bool CheckPressStatusKey()
        {
            return MainManager.GetKey(9, false);
        }

        static void CallShowStatusInfo()
        {
            MainManager.battle.StartCoroutine(BattleControl_Ext.Instance.statusInfo.ShowStatusInfo());
        }
    }
}
