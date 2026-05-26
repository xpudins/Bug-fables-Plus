using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace BFPlus.Patches.BattleControlTranspilers.CalculateBaseDamagePatches
{
    public class PatchNewAttackProperty : PatchBaseCalculateBaseDamage
    {
        public PatchNewAttackProperty()
        {
            priority = 601;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchCall(out _), i => i.MatchStloc(out _));
            var propertyRef = cursor.Prev.Operand;

            cursor.Emit(OpCodes.Ldloc, propertyRef);
            cursor.Emit(OpCodes.Ldarg_2);
            cursor.Emit(OpCodes.Ldarg, 4);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchNewAttackProperty), "CheckNewProperty"));
        }

        static void CheckNewProperty(BattleControl.AttackProperty property, ref MainManager.BattleData target, bool block)
        {
            NewProperty newProperty = (NewProperty)property;

            if (!block)
            {
                switch (newProperty)
                {
                    case NewProperty.Tiny:
                        BattleControl_Ext.Instance.ApplyStatus((MainManager.BattleCondition)NewCondition.Tiny, ref target, 2, "Shot2", 1.2f, 1, null, Vector3.zero, Vector3.one);
                        break;

                    case NewProperty.Dizzy:
                        BattleControl_Ext.Instance.TryDizzy(null, ref target, 2);
                        break;

                    case NewProperty.Huge:
                        break;
                }
            }
        }

    }
}
