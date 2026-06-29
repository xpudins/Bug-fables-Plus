using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.CheckDeadPatches
{
    public class PatchCleanKilledEnemyPos : PatchBaseCheckDead
    {
        public PatchCleanKilledEnemyPos()
        {
            priority = 155687;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchStfld(AccessTools.Field(typeof(BattleControl), "deadenemypos")));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchCleanKilledEnemyPos), "AddCleanKilledEnemyPos"));
        }

        static void AddCleanKilledEnemyPos()
        {
            MainManager.battle.deadenemypos.AddRange(BattleControl_Ext.Instance.cleanKilledEnemyPos);
        }

    }
}
