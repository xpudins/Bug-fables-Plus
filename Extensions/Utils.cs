using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Linq;
using UnityEngine;
using OpCodes = Mono.Cecil.Cil.OpCodes;
namespace BFPlus.Extensions
{
    public static class Utils
    {
        public static void InsertYieldReturn(ILCursor cursor)
        {
            var declaringType = ((FieldReference)cursor.Body.Instructions[1].Operand).DeclaringType.Resolve();

            var f_state = declaringType.FindField("<>1__state");
            var f_current = declaringType.FindField("<>2__current");

            int cursorIndex = cursor.Index;

            cursor.Goto(0);
            ILLabel[] labels = new ILLabel[0];
            cursor.GotoNext(i => i.MatchSwitch(out labels));
            var tempList = labels.ToList();
            int nextReturnIndex = tempList.Count;

            var newLabel = cursor.DefineLabel();
            tempList.Add(newLabel);
            cursor.Next.Operand = tempList.ToArray();
            cursor.Goto(cursorIndex);

            cursor.Emit(OpCodes.Stfld, f_current);
            cursor.Emit(OpCodes.Ldarg_0);

            cursor.Emit(OpCodes.Ldc_I4, nextReturnIndex);
            cursor.Emit(OpCodes.Stfld, f_state);
            cursor.Emit(OpCodes.Ldc_I4_1);
            cursor.Emit(OpCodes.Ret).MarkLabel(newLabel);

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldc_I4_M1);
            cursor.Emit(OpCodes.Stfld, f_state);
        }

        public static void InsertStartStylishTimer(ILCursor cursor, float startFrames, float endFrames, int stylishID = 0, float stylishGain = 0.1f, bool commandSuccess = true)
        {
            cursor.Emit(OpCodes.Ldc_R4, startFrames);
            cursor.Emit(OpCodes.Ldc_R4, endFrames);
            cursor.Emit(OpCodes.Ldc_I4, stylishID);
            cursor.Emit(OpCodes.Ldc_R4, stylishGain);
            cursor.Emit(commandSuccess ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "StartStylishTimer"));
        }

        public static void InsertWaitStylish(ILCursor cursor, float waitFrames = 0f)
        {
            cursor.Emit(OpCodes.Ldc_R4, waitFrames);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "WaitStylish"));
            InsertYieldReturn(cursor);
        }

        public static void RemoveUntilInst(ILCursor cursor, params Func<Instruction, bool>[] predicates)
        {
            int cursorIndex = cursor.Index;
            cursor.GotoNext(predicates);
            int matchIndex = cursor.Index;
            cursor.Goto(cursorIndex);
            cursor.RemoveRange(matchIndex - cursorIndex);
        }

        static BattleControl.AttackProperty? GetDizzyProperty()
        {
            return (BattleControl.AttackProperty)NewProperty.Dizzy;
        }

        static BattleControl.AttackProperty? GetInkProperty()
        {
            return BattleControl.AttackProperty.Ink;
        }

        public static Vector2 RotateVector(Vector2 vec, float degAng)
        {
            degAng *= -(float)Math.PI / 180f;
            return new Vector2(
               vec.x * Mathf.Cos(degAng) - vec.y * Mathf.Sin(degAng),
               vec.x * Mathf.Sin(degAng) + vec.y * Mathf.Cos(degAng));
        }
    }
}
