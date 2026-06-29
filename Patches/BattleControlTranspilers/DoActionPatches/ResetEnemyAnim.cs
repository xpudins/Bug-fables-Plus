using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFPlus.Patches.BattleControlTranspilers.DoActionPatches
{

    public class PatchResetEnemyAnim : PatchBaseDoAction
    {
        public PatchResetEnemyAnim()
        {
            priority = 44220;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchCall(AccessTools.Method(typeof(BattleControl), nameof(BattleControl.UpdateAnim), new Type[] { })));

            int index = cursor.Index;
            cursor.GotoNext(i => i.MatchLdfld(out _));
            var entityRef = cursor.Next.Operand;
            cursor.Goto(index);

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, entityRef);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchResetEnemyAnim), "ResetAnim"));
        }

        static void ResetAnim(EntityControl entity)
        {
            Entity_Ext.GetEntity_Ext(entity).ResetDizzyAngle(true);
        }
    }
}
