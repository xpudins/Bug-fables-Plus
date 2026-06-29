using BFPlus.Patches.DoActionPatches;
using MonoMod.Cil;

namespace BFPlus.Patches.EventControlTranspilers
{
    //Kabbu's request event
    public class PatchKabbuRequestMusic : PatchBaseEvent213
    {
        public PatchKabbuRequestMusic()
        {
            priority = 252952;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdstr("Moth"));
            cursor.Next.Operand = "KabbuTheme";
        }
    }
}
