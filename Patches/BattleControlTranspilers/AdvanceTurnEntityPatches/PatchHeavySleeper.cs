using BFPlus.Patches.DoActionPatches;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.AdvanceTurnEntityPatches
{
    /// <summary>
    /// Reduces Heavy sleeper healing from *3 to *2
    /// </summary>
    public class PatchHeavySleeper : PatchBaseAdvanceTurnEntity
    {
        public PatchHeavySleeper()
        {
            priority = 472;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(47));
            cursor.GotoNext(i => i.MatchLdcI4(3));
            cursor.Emit(OpCodes.Ldc_I4, 2);
            cursor.Remove();
        }
    }
}
