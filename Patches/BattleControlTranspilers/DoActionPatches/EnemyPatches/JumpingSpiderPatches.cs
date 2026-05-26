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

namespace BFPlus.Patches.BattleControlTranspilers.DoActionPatches.EnemyPatches
{
    public class PatchJumpingSpiderBigJumpProperty : PatchBaseDoAction
    {
        public PatchJumpingSpiderBigJumpProperty()
        {
            priority = 95260;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(758));
            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(5));
            cursor.RemoveRange(3);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(Utils), "GetDizzyProperty"));
        }
    }
}
