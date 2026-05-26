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
using static BattleControl;

namespace BFPlus.Patches.BattleControlTranspilers.CalculateBaseDamagePatches
{
    public class PatchExtraTopple : PatchBaseCalculateBaseDamage
    {
        public PatchExtraTopple()
        {
            priority = 1631;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;

            cursor.GotoNext(i => i.MatchLdfld(AccessTools.Field(typeof(MainManager.BattleData), "cantfall")));
            cursor.GotoNext(x => x.MatchCall<BattleControl>(nameof(BattleControl.CanBeToppled)));
            cursor.GotoNext(MoveType.After, x => x.MatchBrfalse(out label));
            cursor.Emit(OpCodes.Ldarg, 8);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchExtraTopple), nameof(CanImmediatelyDropTarget)));
            cursor.Emit(OpCodes.Brtrue, label);
        }

        static bool CanImmediatelyDropTarget(DamageOverride[] overrides) => overrides?.Contains((DamageOverride)NewDamageOverride.ExtraAirTopple) ?? false;

    }
}
