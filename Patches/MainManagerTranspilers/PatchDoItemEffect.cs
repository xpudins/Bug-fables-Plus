using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.MainManagerTranspilers
{
    public class PatchCheckMaxCharge : PatchBaseMainManagerDoItemEffect
    {
        public PatchCheckMaxCharge()
        {
            priority = 447;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdfld(AccessTools.Field(typeof(MainManager.BattleData), "charge")));
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.Remove();
            cursor.GotoPrev(i => i.MatchLdarga(out _));

            var characterIdRef = cursor.Next.Operand;
            var callRef = cursor.Instrs[cursor.Index + 1].Operand;

            cursor.GotoNext(i => i.MatchLdcI4(3));
            cursor.Emit(OpCodes.Ldarga, characterIdRef);
            cursor.Emit(OpCodes.Call, callRef);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "CheckMaxCharge"));
            cursor.Remove();
        }
    }

    public class PatchCheckRemoveAll : PatchBaseMainManagerDoItemEffect
    {
        public PatchCheckRemoveAll()
        {
            priority = 755;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdstr("Heal3"), i => i.MatchCall(out _), i => i.MatchPop(), i => i.MatchLdcI4(1));

            cursor.GotoNext(i => i.MatchLdarga(out _));
            var characterId = cursor.Next.Operand;
            var getValueRef = cursor.Instrs[cursor.Index + 1].Operand;
            cursor.GotoNext(i => i.MatchBrfalse(out _));

            cursor.GotoPrev(MoveType.After, i => i.MatchCall(out _));

            cursor.Emit(OpCodes.Ldarga_S, characterId);
            cursor.Emit(OpCodes.Call, getValueRef);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchCheckRemoveAll), "RemoveNewNegativeConditions"));
        }

        static void RemoveNewNegativeConditions(int characterId)
        {
            MainManager.RemoveCondition((MainManager.BattleCondition)NewCondition.Dizzy, MainManager.instance.playerdata[characterId]);
            MainManager.RemoveCondition((MainManager.BattleCondition)NewCondition.Paintball, MainManager.instance.playerdata[characterId]);

        }
    }

    public class PatchCheckRemoveAllParty : PatchBaseMainManagerDoItemEffect
    {
        public PatchCheckRemoveAllParty()
        {
            priority = 956;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After,
                i => i.MatchLdstr("Heal3"),
                i => i.MatchCall(out _),
                i => i.MatchPop(),
                i => i.MatchLdcI4(0),
                i => i.MatchStloc3());

            cursor.GotoNext(MoveType.After, i => i.MatchLdelemAny(out _), i => i.MatchCall(out _));

            cursor.Emit(OpCodes.Ldloc_3);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchCheckRemoveAll), "RemoveNewNegativeConditions"));
        }
    }
}
