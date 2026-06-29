using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers
{
    /// <summary>
    /// Remove the shock trooper check in invulnerable
    /// </summary>
    public class PatchBattleControlInvulnerable : PatchBaseBattleControlInvulnerable
    {
        public PatchBattleControlInvulnerable()
        {
            priority = 30;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdloc0());
            Utils.RemoveUntilInst(cursor, i => i.MatchLdcI4(0), i => i.MatchRet());
        }
    }
}
