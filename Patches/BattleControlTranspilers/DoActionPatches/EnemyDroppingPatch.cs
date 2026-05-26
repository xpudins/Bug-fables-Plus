using HarmonyLib;
using MonoMod.Cil;

namespace BFPlus.Patches.DoActionPatches
{
    public class PatchEnemyDropping : PatchBaseDoAction
    {
        public PatchEnemyDropping()
        {

            priority = 150851;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchCall(AccessTools.Method(typeof(BattleControl), "EnemyDropping")));
            cursor.GotoNext(MoveType.Before, i => i.MatchLdloc1(), i => i.MatchLdcI4(1), i => i.MatchStfld(typeof(BattleControl).GetField("action")));
            cursor.RemoveRange(3);
            cursor.GotoNext(i => i.MatchCall(AccessTools.Method(typeof(BattleControl), "EnemyDropping")));
            cursor.GotoNext(MoveType.Before, i => i.MatchLdloc1(), i => i.MatchLdcI4(0), i => i.MatchStfld(typeof(BattleControl).GetField("action")));
            cursor.RemoveRange(3);
        }
    }
}
