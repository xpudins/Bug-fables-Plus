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
    public class PatchNinjaProjectileProperty : PatchBaseDoAction
    {
        public PatchNinjaProjectileProperty()
        {
            priority = 95776;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(768));
            cursor.GotoNext(i => i.MatchLdcI4(3));

            cursor.Next.OpCode = OpCodes.Ldc_I4;
            cursor.Next.Operand = (int)BattleControl.AttackProperty.Ink;
        }
    }
}
