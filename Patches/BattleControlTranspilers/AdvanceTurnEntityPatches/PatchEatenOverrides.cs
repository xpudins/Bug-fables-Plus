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

namespace BFPlus.Patches.BattleControlTranspilers.AdvanceTurnEntityPatches
{

    public class PatchEatenOverrides : PatchBaseAdvanceTurnEntity
    {
        public PatchEatenOverrides()
        {
            priority = 338;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(15), i=>i.MatchStelemI4());

            //add status damage overide to overrides
            cursor.GotoNext(i => i.MatchLdcI4(0));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "GetStatusDamageOverrides"));
        }
    }
}
