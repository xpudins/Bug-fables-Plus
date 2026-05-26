using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.DoActionPatches
{
    public class CheckDelayedConditionPlayer : PatchBaseDoAction
    {
        public CheckDelayedConditionPlayer()
        {
            priority = 150469;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdloc1(), i => i.MatchCall(AccessTools.Method(typeof(BattleControl), "EndPlayerTurn")));
            cursor.Next.OpCode = OpCodes.Nop;
            cursor.GotoNext(i => i.MatchCall(out _));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "CheckDelayedConditionsPlayer"));
            Utils.InsertYieldReturn(cursor);
            cursor.Emit(OpCodes.Ldloc_1);
        }

    }
}
