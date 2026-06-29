using BFPlus.Patches.DoActionPatches;
using BFPlus.Extensions.BattleStuff.StatusStuff;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace BFPlus.Patches.BattleControlTranspilers.AdvanceTurnEntityPatches
{
    public class PatchBadDreams : PatchBaseAdvanceTurnEntity
    {
        public PatchBadDreams()
        {
            priority = 437;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = cursor.DefineLabel();
            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(81), i => i.MatchCall(out _));
            cursor.Remove();

            int cursorIndex = cursor.Index;

            ILLabel jumpLabel = null;
            cursor.GotoNext(i => i.MatchBge(out jumpLabel));
            cursor.Goto(cursorIndex);

            cursor.Emit(OpCodes.Brfalse, label);

            cursor.Emit(OpCodes.Ldarg_1);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(Sleep), "DoBadDreams"));
            cursor.Emit(OpCodes.Ldarg_2);
            cursor.Emit(OpCodes.Ldc_I4_1);
            cursor.Emit(OpCodes.Stind_I1);

            cursor.Emit(OpCodes.Br, jumpLabel);
            cursor.MarkLabel(label);
        }
    }
}
