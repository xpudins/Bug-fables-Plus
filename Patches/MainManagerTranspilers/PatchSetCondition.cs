using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Linq;
using static MainManager;

namespace BFPlus.Patches.MainManagerTranspilers
{
    /// <summary>
    /// We make sure that ink and sticky status turns get added instead of being set
    /// </summary>
    public class PatchStatusTurns : PatchBaseMainManagerSetCondition
    {
        public PatchStatusTurns()
        {
            priority = 48;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchBrfalse(out _), i => i.MatchLdarg0());

            ILLabel label = null;
            int index = cursor.Index;
            cursor.GotoNext(i => i.MatchLdstr("Player"), i => i.MatchCallvirt(out _), i => i.MatchBrfalse(out label));
            cursor.Goto(index);

            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchStatusTurns), "IsNewStackableStatus"));
            cursor.Emit(OpCodes.Brtrue, label);
            cursor.Emit(OpCodes.Ldarg_0);
        }

        static bool IsNewStackableStatus(MainManager.BattleCondition condition)
        {
            MainManager.BattleCondition[] newStackables =
            {
                //BattleCondition.Reflection,
                BattleCondition.Shield,
                BattleCondition.Inked,
                BattleCondition.Sticky,
                (BattleCondition)NewCondition.Vitiation,
                (BattleCondition)NewCondition.Paintball,
                (BattleCondition)NewCondition.Slugskin,
                (BattleCondition)NewCondition.Dizzy
            };
            return newStackables.Contains(condition);
        }
    }
}
