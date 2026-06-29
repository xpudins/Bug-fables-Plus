using BFPlus.Extensions;
using BFPlus.Extensions.BattleStuff.StatusStuff;
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
    public class PatchSwitchPosCarouselEffect : PatchBaseBattleControlSwitchPos
    {
        public PatchSwitchPosCarouselEffect()
        {
            priority = 20388;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdfld(out _), i=>i.MatchBneUn(out _));       
            var targetedRef = cursor.Next.Operand;
            cursor.GotoNext(i => i.MatchLdfld(out _), i => i.MatchBneUn(out _));
            var calledRef = cursor.Next.Operand;

            cursor.GotoNext(MoveType.After, i => i.MatchCall(AccessTools.Method(typeof(BattleControl), nameof(BattleControl.CancelList))));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, targetedRef);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, calledRef);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchSwitchPosCarouselEffect), nameof(DoCarousel)));
        }

        static void DoCarousel(int targetId, int calledId)
        {
            Dizzy.DoCarousel(MainManager.battle.partypointer[calledId]);
            Dizzy.DoCarousel(MainManager.battle.partypointer[targetId]);
        }
    }
}
