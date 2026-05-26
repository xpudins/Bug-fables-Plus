using BFPlus.Extensions.BattleStuff.StatusStuff;
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

namespace BFPlus.Patches.BattleControlTranspilers
{
    public class PatchSwitchPartyCarouselEffect : PatchBaseBattleControlSwitchParty
    {
        public PatchSwitchPartyCarouselEffect()
        {
            priority = 20797;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After,i => i.MatchCall(out _), i=>i.MatchLdarg0(), i=>i.MatchLdfld(out _));
            var fastRef = cursor.Prev.Operand;

            cursor.GotoNext(MoveType.After, i => i.MatchStfld(AccessTools.Field(typeof(MainManager.BattleData), "pointer")));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, fastRef);
            cursor.Emit(OpCodes.Ldloc_3);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchSwitchPartyCarouselEffect), nameof(CheckCarousel)));
        }

        static void CheckCarousel(bool fast, int playerId)
        {
            if(!fast)
                MainManager.battle.StartCoroutine(DoCarousel(playerId));
        }

        //just so the pos gets updated before the itemspin
        static IEnumerator DoCarousel(int playerId)
        {
            yield return null;
            Dizzy.DoCarousel(playerId);
        }
    }
}
