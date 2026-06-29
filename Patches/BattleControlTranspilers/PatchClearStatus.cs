using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Linq;

namespace BFPlus.Patches.BattleControlTranspilers
{
    public class PatchClearStatus : PatchBaseClearStatus
    {
        public PatchClearStatus()
        {
            priority = 47;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchStloc1());
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchClearStatus), "CheckUnClearableStatus"));
            cursor.Emit(OpCodes.Stloc_1);
        }

        static MainManager.BattleCondition[] CheckUnClearableStatus(MainManager.BattleCondition[] conditions)
        {
            var tempList = conditions.ToList();
            tempList.Add(MainManager.BattleCondition.Sturdy);
            tempList.Add((MainManager.BattleCondition)NewCondition.Tiny);
            tempList.Add((MainManager.BattleCondition)NewCondition.Huge);

            if (MainManager.BadgeIsEquipped((int)Medal.PermanentInk))
            {
                tempList.Add(MainManager.BattleCondition.Inked);
            }

            if (MainManager.BadgeIsEquipped((int)Medal.SturdyStrands))
            {
                tempList.Add(MainManager.BattleCondition.Sticky);
            }

            return tempList.ToArray();
        }
    }
}
