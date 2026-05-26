using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.AdvanceTurnEntityPatches
{
    public class PatchRemoveLastWindAdvanceTurnEntity : PatchBaseAdvanceTurnEntity
    {
        public PatchRemoveLastWindAdvanceTurnEntity()
        {
            priority = 867;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(87));
            Utils.RemoveUntilInst(cursor, i => i.MatchBr(out _));
        }
    }
}
