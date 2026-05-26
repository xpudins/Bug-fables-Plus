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

namespace BFPlus.Patches.BattleControlTranspilers
{
    /// <summary>
    /// Remove the set max options call in playerturn
    /// </summary>
    public class PatchRemoveSetMaxOptions : PatchBasePlayerTurn
    {
        public PatchRemoveSetMaxOptions()
        {
            priority = 20;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(
                i => i.MatchLdarg0(), 
                i => i.MatchCall(AccessTools.Method(typeof(BattleControl), nameof(BattleControl.SetMaxOptions))
            ));
            cursor.RemoveRange(2);
        }
    }
}
