using BFPlus.Patches.DoActionPatches;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.MainManagerTranspilers.GetEnemyDataPatches
{
    public class PatchTanjyHolo : PatchBaseMainManagerGetEnemyData
    {
        public PatchTanjyHolo()
        {
            priority = 819;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdloc0(), i => i.MatchLdfld(out _), i => i.MatchLdcI4(110), i => i.MatchBneUn(out _));
            cursor.RemoveRange(3);
            cursor.Next.OpCode = OpCodes.Br;
        }
    }
}
