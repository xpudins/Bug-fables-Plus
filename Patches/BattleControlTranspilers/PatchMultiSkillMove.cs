using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace BFPlus.Patches
{
    /// <summary>
    /// Adds checks for usecharge and if you suffer from tired
    /// </summary>
    public class PatchCanUseCharge : PatchBaseBattleControlMultiSkillMove
    {
        public PatchCanUseCharge()
        {
            priority = 50;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchStfld(AccessTools.Field(typeof(MainManager.BattleData), "charge")));
            cursor.GotoPrev(i => i.MatchLdsfld(out _));
            var label = cursor.DefineLabel();
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "CanUseCharge", new Type[] { typeof(int) }));
            cursor.Emit(OpCodes.Brfalse, label);
            cursor.GotoNext();
            cursor.GotoNext(i => i.MatchLdsfld(out _));

            cursor.GotoNext(i => i.MatchLdflda(AccessTools.Field(typeof(MainManager.BattleData), "tired")));
            cursor.GotoPrev(i => i.MatchLdsfld(out _));
            var otherLabel = cursor.DefineLabel();

            cursor.MarkLabel(label);
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "CheckRecharge", new Type[] { typeof(int) }));
            cursor.Emit(OpCodes.Brfalse, otherLabel);
            cursor.GotoNext(i => i.MatchBr(out _)).MarkLabel(otherLabel);
        }
    }
}
