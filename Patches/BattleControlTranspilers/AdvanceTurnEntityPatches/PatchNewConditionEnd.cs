using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.AdvanceTurnEntityPatches
{
    public class PatchNewConditionEnd : PatchBaseAdvanceTurnEntity
    {
        public PatchNewConditionEnd()
        {
            priority = 602;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchStfld(AccessTools.Field(typeof(MainManager.BattleData), "frozenlastturn")));
            cursor.GotoNext(i => i.MatchLdloc(out _), i => i.MatchSwitch(out _));

            var conditionRef = cursor.Next.Operand;

            cursor.Emit(OpCodes.Ldloc, conditionRef);
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchNewConditionEnd), "CheckNewConditionEnd"));
        }

        static void CheckNewConditionEnd(MainManager.BattleCondition condition, ref MainManager.BattleData target)
        {
            NewCondition newCondition = (NewCondition)condition;
            var entity_Ext = Entity_Ext.GetEntity_Ext(target.battleentity);
            switch (newCondition)
            {
                case NewCondition.Huge:
                case NewCondition.Tiny:
                    BattleControl_Ext.Instance.ResetTinyHugeEffect(target.battleentity);
                    entity_Ext.tinyMovesAdded = false;
                    break;
                case NewCondition.Vitiation:
                    entity_Ext.vitiation = false;
                    break;

                case NewCondition.Dizzy:
                    entity_Ext.isDizzy = false;
                    entity_Ext.ResetDizzyAngle();
                    break;

            }
        }

    }
}
