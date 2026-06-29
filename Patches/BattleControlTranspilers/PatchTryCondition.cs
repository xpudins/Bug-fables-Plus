using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace BFPlus.Patches.BattleControlTranspilers
{
    //Remove every update anim call
    public class PatchTryCondition : PatchBaseBattleControlTryCondition
    {
        public PatchTryCondition()
        {
            priority = 0;
        }

        protected override void ApplyPatch(ILCursor c, ILContext context)
        {
            c.Goto(0);

            var updateAnimRef = AccessTools.Method(typeof(BattleControl), "UpdateAnim", new Type[] { });
            while (c.TryGotoNext(i => i.MatchLdarg0(), i => i.MatchCall(updateAnimRef)))
            {
                for (int i = 0; i < 2; i++)
                {
                    c.Next.OpCode = OpCodes.Nop;
                    c.GotoNext();
                }
            }
            c.Goto(0);
            c.GotoNext(MoveType.After, x => x.MatchStloc(0));

            c.Emit(OpCodes.Ldloca, 0);
            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchTryCondition), "GetResistancePierce"));
        }

        public static void GetResistancePierce(ref float resPierce, ref MainManager.BattleData target)
        {
            Entity_Ext extEnt = Entity_Ext.GetEntity_Ext(target.battleentity);
            resPierce += extEnt?.piercedStatusRes ?? 0;
        }
    }
}
