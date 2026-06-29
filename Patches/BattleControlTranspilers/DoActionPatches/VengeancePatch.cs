using BFPlus.Extensions;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
namespace BFPlus.Patches.DoActionPatches
{
    public class PatchVengeanceCharge : PatchBaseDoAction
    {
        public PatchVengeanceCharge()
        {
            priority = 44153;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After,
                i => i.MatchStfld(AccessTools.Field(typeof(BattleControl), "hasblocked")),
                i => i.MatchLdloc1()
            );

            cursor.Remove();
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "CheckVengeanceCharge"));
        }
    }
}