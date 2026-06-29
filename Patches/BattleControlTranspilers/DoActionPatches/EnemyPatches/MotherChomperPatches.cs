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

namespace BFPlus.Patches.BattleControlTranspilers.DoActionPatches.EnemyPatches
{

    public class MotherChomperPatches : PatchBaseDoAction
    {
        public MotherChomperPatches()
        {
            priority = 149915;
        }    

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(1665));
            cursor.GotoNext(i => i.MatchLdfld(AccessTools.Field(typeof(BattleControl), "demomode")));
            cursor.GotoPrev(MoveType.After, i => i.MatchInitobj(out _));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MotherChomperPatches), "ResetMotherChomperAnim"));
            Utils.InsertYieldReturn(cursor);
        }

        static IEnumerator ResetMotherChomperAnim()
        {
            BattleControl_Ext.Instance.entityAttacking.animstate = 0;
            yield return null;
        }
    }
}
