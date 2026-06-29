using BFPlus.Extensions;
using BFPlus.Extensions.BattleStuff.StatusStuff;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;

namespace BFPlus.Patches.BattleControlTranspilers.CheckDeadPatches
{
    public class PatchInkBlotCheck : PatchBaseCheckDead
    {
        static List<EntityControl> listRef;
        public PatchInkBlotCheck()
        {
            priority = 156182;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchCall(AccessTools.Method(typeof(BattleControl), "ReorganizeEnemies", new Type[] { })));
            cursor.Prev.OpCode = OpCodes.Nop;
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "DoInkBlotEnemy"));

            cursor.Emit(OpCodes.Ldloc, 4);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchInkBlotCheck), nameof(SaveListRef)));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldc_I4_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(Dizzy), "WaitForDizzyKO"));
            Utils.InsertYieldReturn(cursor);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchInkBlotCheck), nameof(RestoreListRef)));
            cursor.Emit(OpCodes.Stloc, 4);
            cursor.Emit(OpCodes.Ldloc_1);
        }

        ///list local in checkdead() is not saved inbetween yields, we need to save and restore it after our stuff.
        static void SaveListRef(List<EntityControl> list)
        {
            listRef = list;
        }

        static List<EntityControl> RestoreListRef()
        {
            return listRef;
        }
    }
}
