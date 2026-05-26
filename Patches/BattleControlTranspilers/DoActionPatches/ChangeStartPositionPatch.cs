using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BFPlus.Patches.BattleControlTranspilers.DoActionPatches
{
    public class ChangeStartPositionPatch : PatchBaseDoAction
    {
        public ChangeStartPositionPatch()
        {
            priority = 150015;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(1666));
            cursor.GotoNext(i => i.MatchCallvirt(out _));
            cursor.GotoNext(i => i.MatchLdfld(out _));
            var startp = cursor.Next.Operand;
            cursor.GotoPrev(MoveType.After, i => i.MatchStloc(out _));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldflda, startp);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(ChangeStartPositionPatch), nameof(GetNewStartPosition)));

        }

        static void GetNewStartPosition(ref Vector3 startPosition)
        {
            if(BattleControl_Ext.Instance.startPosition.HasValue)
                startPosition = BattleControl_Ext.Instance.startPosition.Value;
            BattleControl_Ext.Instance.startPosition = null;
        }
    }
}
