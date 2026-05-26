using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFPlus.Patches.BattleControlTranspilers.EventDialoguePatches
{
    public class PatchAriaDeathEvent : PatchBaseEventDialogue
    {
        public PatchAriaDeathEvent()
        {
            priority = 6848;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchStfld(out _), i => i.MatchLdarg(0), i => i.MatchLdloc(1), i => i.MatchLdcI4(21));
            cursor.GotoNext(MoveType.After, i => i.MatchStfld(AccessTools.Field(typeof(MainManager.BattleData), "eventondeath")));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl), nameof(BattleControl.CheckDead)));
            Utils.InsertYieldReturn(cursor);
            Utils.RemoveUntilInst(cursor, i=>i.MatchBr(out _));
        }
    }
}
