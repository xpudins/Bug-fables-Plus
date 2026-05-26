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
    /// <summary>
    /// Replaced delayed proj attack property by ink on block
    /// </summary>
    public class PatchArcherDelayedProperty : PatchBaseDoAction
    {
        public PatchArcherDelayedProperty()
        {
            priority = 97230;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(794));
            cursor.GotoNext(i => i.MatchLdcR4(50));
            cursor.GotoPrev(i => i.MatchLdcI4(0));
            cursor.Emit(OpCodes.Ldc_I4, (int)BattleControl.AttackProperty.InkOnBlock);
            cursor.Remove();
        }
    }

    public class PatchArcherBowProperty : PatchBaseDoAction
    {
        public PatchArcherBowProperty()
        {
            priority = 97031;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(789));
            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(2));
            cursor.Emit(OpCodes.Ldc_I4, (int)BattleControl.AttackProperty.Ink);
            cursor.Remove();
        }
    }
}
