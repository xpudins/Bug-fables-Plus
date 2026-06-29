using BFPlus.Patches.DoActionPatches;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.DoActionPatches.EnemyPatches
{
    /// <summary>
    /// add sticky property to wasp needles instead of poison
    /// </summary>
    public class PatchWaspScoutNeedlesProperty : PatchBaseDoAction
    {
        public PatchWaspScoutNeedlesProperty()
        {
            priority = 76805;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(449));
            cursor.GotoNext(i => i.MatchLdloca(out _), i => i.MatchLdcI4(3));
            cursor.GotoNext();
            cursor.Remove();
            cursor.Emit(OpCodes.Ldc_I4, 26);
        }
    }
}
