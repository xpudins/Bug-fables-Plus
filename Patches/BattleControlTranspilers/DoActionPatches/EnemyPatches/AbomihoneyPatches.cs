using BFPlus.Extensions;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.DoActionPatches.EnemyPatches
{
    public class PatchAbomiberryUnderBiteDMG : PatchBaseDoAction
    {
        public PatchAbomiberryUnderBiteDMG()
        {
            priority = 70152;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(341));
            cursor.GotoNext(MoveType.After, i => i.MatchLdfld(AccessTools.Field(typeof(BattleControl), "playertargetID")));
            cursor.Emit(OpCodes.Ldc_I4, 3);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "GetStickyProperty"));
            cursor.RemoveRange(3);
        }
    }


    public class PatchAbomiberryBite : PatchBaseDoAction
    {
        public PatchAbomiberryBite()
        {
            priority = 71435;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(359));
            cursor.GotoNext(MoveType.After, i => i.MatchLdfld(AccessTools.Field(typeof(BattleControl), "playertargetID")));
            cursor.Emit(OpCodes.Ldc_I4, 3);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "GetStickyProperty"));
            cursor.RemoveRange(4);
        }
    }
}
