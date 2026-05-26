using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using BFPlus.Patches.ShowItemListPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.MainManagerTranspilers.UpdatePatches
{
    public class PatchTrustFallOption : PatchBaseMainManagerUpdate
    {
        public PatchTrustFallOption()
        {
            priority = 2135;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;
            cursor.GotoNext(
                i => i.MatchBneUn(out label),
                i => i.MatchLdsfld(out _),
                i => i.MatchLdfld(AccessTools.Field(typeof(BattleControl), "currentaction")));

            cursor.GotoNext(i => i.MatchLdarg0());
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "CanUseTrustFall"));
            cursor.Emit(OpCodes.Brtrue, label);


            cursor.GotoNext(i => i.MatchCall(AccessTools.Method(typeof(MainManager), "AllPartyFree")));
            cursor.Next.OpCode = OpCodes.Nop;

            cursor.GotoNext(i => i.MatchBrtrue(out label));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), nameof(BattleControl_Ext.CanBypassSwapRestrictions)));
            cursor.Emit(OpCodes.Brtrue, label);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager), "AllPartyFree"));

            //push the flee index to 5 if trust fall is equipped
            cursor.GotoNext(i => i.MatchLdcI4(4));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchStrategyTextValues), "GetFleeIndex"));
            cursor.Remove();
        }
    }
}
