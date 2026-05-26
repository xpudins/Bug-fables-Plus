using HarmonyLib;
using BFPlus.Extensions;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;

namespace BFPlus.Patches.DoActionPatches
{
    public class PatchItemAttackTargeting : PatchBaseDoAction
    {
        public PatchItemAttackTargeting()
        {
            priority = 44268;
        }

        protected override void ApplyPatch(ILCursor c, ILContext context)
        {
            c.GotoNext(x => x.MatchLdcI4(1),
                       x => x.MatchCall(AccessTools.Method(typeof(BattleControl), "GetAvaliableTargets", new Type[] { typeof(bool), typeof(bool), typeof(int), typeof(bool) })));

            c.Remove();
            c.Emit(OpCodes.Ldloc_1);
            c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(BattleControl), "selecteditem"));
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "CanItemHitUnderground"));
        }
    }
}