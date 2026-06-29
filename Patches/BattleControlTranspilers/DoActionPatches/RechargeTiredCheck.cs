using BFPlus.Extensions;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace BFPlus.Patches.DoActionPatches
{
    public class PatchCheckRechargeTired : PatchBaseDoAction
    {
        public PatchCheckRechargeTired()
        {
            priority = 150415;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdflda(typeof(MainManager.BattleData).GetField("tired")));

            ILLabel label = null;
            cursor.GotoPrev(MoveType.After, i => i.MatchBeq(out label));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "CheckRecharge", new Type[] { }));
            cursor.Emit(OpCodes.Brfalse, label);

            cursor.GotoNext(i => i.MatchLdflda(typeof(MainManager.BattleData).GetField("tired")));
            cursor.GotoNext(i => i.MatchLdflda(typeof(MainManager.BattleData).GetField("tired")));
            cursor.GotoPrev(MoveType.After, i => i.MatchBrtrue(out label));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "CheckRecharge", new Type[] { }));
            cursor.Emit(OpCodes.Brfalse, label);
        }
    }
}
