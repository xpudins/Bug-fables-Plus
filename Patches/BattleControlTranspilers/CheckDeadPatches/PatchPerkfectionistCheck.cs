using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.CheckDeadPatches
{
    public class PatchPerkfectionistCheck : PatchBaseCheckDead
    {
        public PatchPerkfectionistCheck()
        {
            priority = 156500;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After,
                i => i.MatchStfld(out _),
                i => i.MatchLdloc1(),
                i => i.MatchLdfld(AccessTools.Field(typeof(BattleControl), "enemydata")),
                i => i.MatchLdlen()
            );

            cursor.GotoPrev(MoveType.After, i => i.MatchLdloc1());
            cursor.Prev.OpCode = OpCodes.Nop;
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "CheckPerkfectionist"));
            Utils.InsertYieldReturn(cursor);
            cursor.Emit(OpCodes.Ldloc_1);
        }

    }
}
