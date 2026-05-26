using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers
{
    public class PatchNewItemSelectSprite : PatchBaseBattleControlUpdateText
    {
        public PatchNewItemSelectSprite()
        {
            priority = 552;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdstr("|center|"), i => i.MatchLdsfld(AccessTools.Field(typeof(MainManager), "instance")));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "GetItemSelectSprite"));
            Utils.RemoveUntilInst(cursor, i => i.MatchLdcI4(0));
        }
    }
}
