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

namespace BFPlus.Patches.MainManagerTranspilers.SetTextPatches
{
    public class PatchMaxQuests : PatchBaseSetText
    {
        public PatchMaxQuests()
        {
            priority = 27319;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.Before, i => i.MatchLdcI4(60), i => i.MatchBlt(out _));
            cursor.Remove();
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "GetMaxQuests"));
        }
    }
}
