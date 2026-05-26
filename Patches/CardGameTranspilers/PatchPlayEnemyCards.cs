using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.CardGameTranspilers
{
    public class PatchPlayEnemyCards : PatchBaseCardGamePlayEnemyCards
    {
        public PatchPlayEnemyCards()
        {
            priority = 38;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdfld(AccessTools.Field(typeof(CardGame.CardData), "tp")));
            cursor.GotoPrev(i => i.MatchLdarg0(), i => i.MatchLdfld(out _), i => i.MatchLdarg0());

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchPlayEnemyCards), "GetTpCostEnemy"));
            Utils.RemoveUntilInst(cursor, i => i.MatchBlt(out _));

            cursor.GotoNext(MoveType.After, i => i.MatchLdfld(AccessTools.Field(typeof(CardGame.CardData), "tp")));
            cursor.GotoPrev(i => i.MatchLdarg0(), i => i.MatchLdfld(out _), i => i.MatchLdarg0());

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchPlayEnemyCards), "GetTpCostEnemy"));
            Utils.RemoveUntilInst(cursor, i => i.MatchSub());
        }

        static int GetTpCostEnemy(CardGame cardGame, int index)
        {
            return PatchCardTpCost.GetTpCost(cardGame, cardGame.handcards[1].ToArray()[index]);
        }
    }
}
