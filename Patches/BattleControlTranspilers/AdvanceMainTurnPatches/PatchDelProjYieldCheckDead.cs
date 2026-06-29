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

namespace BFPlus.Patches.BattleControlTranspilers.AdvanceMainTurnPatches
{
    /// <summary>
    /// In Del Proj the while(checkdead != null) can get stuck and make it so currentturn gets set to -1 on a retried 
    /// fight after the first action. We replace that while with a yield return
    /// </summary>
    public class PatchDelProjYieldCheckDead : PatchBaseAdvanceMainTurn
    {
        public PatchDelProjYieldCheckDead()
        {
            priority = 10509;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After,
               i => i.MatchCall(AccessTools.Method(typeof(BattleControl), "CheckDead"))
            );
            cursor.Remove();
            cursor.Remove();
            Utils.InsertYieldReturn(cursor);
            int cursorIndex = cursor.Index;

            cursor.GotoPrev(i => i.MatchLdloc1(), i => i.MatchLdloc1(), i => i.MatchLdloc1());
            cursor.Remove();
            cursor.Remove();
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Goto(cursorIndex);
        }

    }
}
