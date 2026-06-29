using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace BFPlus.Patches.BattleControlTranspilers.SetItemPatches
{

    public class PatchItemAttackTargeting : PatchBaseSetItem
    {
        public PatchItemAttackTargeting()
        {
            priority = 191619;
        }

        protected override void ApplyPatch(ILCursor c, ILContext context)
        {
            c.GotoNext(x => x.MatchCall(AccessTools.Method(typeof(MainManager), "GetItemUse", new Type[] { typeof(int) })));
            c.GotoNext(MoveType.After, x => x.MatchLdcI4(-1));
            c.Remove();

            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "CanItemHitUnderground"));
        }

    }
}