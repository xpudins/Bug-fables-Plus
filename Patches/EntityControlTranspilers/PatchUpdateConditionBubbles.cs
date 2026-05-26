using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Linq;

namespace BFPlus.Patches.EntityControlTranspilers
{
    /// <summary>
    /// change text color and type for conditions
    /// </summary>
    public class PatchConditionText : PatchBaseUpdateConditionBubbles
    {
        public PatchConditionText()
        {
            priority = 528;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After,
                i => i.MatchLdstr("|color,4|"),
                i => i.MatchBr(out _),
                i => i.MatchLdstr(""),
                i => i.MatchStloc(out _));
            var colorTextRef = cursor.Prev.Operand;
            var arrayRef = cursor.Next.Operand;

            cursor.Emit(OpCodes.Ldloc, colorTextRef);
            cursor.Emit(OpCodes.Ldloc, arrayRef);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchConditionText), "CheckTextColor"));
            cursor.Emit(OpCodes.Stloc, colorTextRef);

            int index = cursor.Index;

            ILLabel label = null;
            cursor.GotoNext(MoveType.After, x => x.MatchBneUn(out label));
            cursor.Goto(index);

            cursor.Emit(OpCodes.Ldloc, arrayRef);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchConditionText), "HasConditionText"));
            cursor.Emit(OpCodes.Brtrue, label);

            cursor.GotoNext(MoveType.After,
                       x => x.MatchLdcI4(20),
                       x => x.MatchBeq(out label));

            cursor.Emit(OpCodes.Ldloc, arrayRef);
            cursor.Emit(OpCodes.Ldarg_2);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchConditionText), "ShouldShowAsStackCounter"));
            cursor.Emit(OpCodes.Brtrue, label);
        }

        static bool HasConditionText(int[] condition)
        {
            int[] noTextConditions = new int[]
            {
                (int)MainManager.BattleCondition.Topple,
                (int)MainManager.BattleCondition.Flipped,
                (int)MainManager.BattleCondition.Sturdy
            };
            return !noTextConditions.Contains(condition[0]);
        }

        static string CheckTextColor(string text, int[] condition)
        {
            if (condition[0] == (int)MainManager.BattleCondition.Sticky
                || condition[0] == (int)NewCondition.Paintball
                || condition[0] == (int)NewCondition.Dizzy
                || condition[0] == (int)NewCondition.Vitiation)
            {
                text = "|color,4||dropshadow,0.05,-0.05|";
            }

            return text;
        }

        static bool ShouldShowAsStackCounter(int[] condition, MainManager.BattleData target)
        {
            int[] stackers = new int[]
            {
                (int)MainManager.BattleCondition.Reflection,
                (int)NewCondition.Vitiation,
                (int)NewCondition.Slugskin,
                (int)NewCondition.Paintball
            };
            return stackers.Contains(condition[0]);
        }
    }

    /// <summary>
    /// Remove the last wind icon when in low hp (<=4 hp)
    /// </summary>
    public class PatchLastWindIcon : PatchBaseUpdateConditionBubbles
    {
        public PatchLastWindIcon()
        {
            priority = 915;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(87));
            cursor.Next.OpCode = OpCodes.Nop;
            cursor.GotoNext(i => i.MatchLdarg2());
            Utils.RemoveUntilInst(cursor, i => i.MatchLdcI4(82));
        }
    }
}
