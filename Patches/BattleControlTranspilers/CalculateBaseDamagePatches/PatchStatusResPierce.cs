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
using static BattleControl;
using static MainManager;

namespace BFPlus.Patches.BattleControlTranspilers.CalculateBaseDamagePatches
{
    public class PatchStatusPierce : PatchBaseCalculateBaseDamage
    {
        public PatchStatusPierce()
        {
            priority = 599;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, x => x.MatchStloc(13));

            cursor.Emit(OpCodes.Ldarg_2);
            cursor.Emit(OpCodes.Ldloca, 13);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchStatusPierce), nameof(ApplyStatusResPierce)));
        }

        static void ApplyStatusResPierce(ref BattleData target, ref float resPierce) => resPierce += Entity_Ext.GetEntity_Ext(target.battleentity).piercedStatusRes;

    }
}
