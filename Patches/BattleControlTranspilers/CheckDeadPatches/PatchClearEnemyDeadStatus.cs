using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.CheckDeadPatches
{
    public class PatchClearEnemyDeadStatus : PatchBaseCheckDead
    {
        public PatchClearEnemyDeadStatus()
        {
            priority = 155876;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchStfld(AccessTools.Field(typeof(MainManager.BattleData), "charge")), i => i.MatchLdloc1());
            cursor.GotoNext(MoveType.After, i => i.MatchStfld(out _));

            int cursorIndex = cursor.Index;


            cursor.GotoNext(i => i.MatchLdloc(out _), i => i.MatchLdelema(out _));
            var indexRef = cursor.Next.Operand;
            cursor.Goto(cursorIndex);

            cursor.Emit(OpCodes.Ldloc, indexRef);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchClearEnemyDeadStatus), "ResetSize"));
            cursor.Emit(OpCodes.Ldloc, indexRef);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchClearEnemyDeadStatus), "ClearEnemyStatus"));
        }

        static void ClearEnemyStatus(int enemyIndex)
        {
            MainManager.battle.ClearStatus(ref MainManager.battle.enemydata[enemyIndex]);
        }

        static void ResetSize(int enemyIndex)
        {
            if (Entity_Ext.GetEntity_Ext(MainManager.battle.enemydata[enemyIndex].battleentity).scaleChanged)
                BattleControl_Ext.Instance.ResetTinyHugeEffect(MainManager.battle.enemydata[enemyIndex].battleentity);
        }

    }
}
