using BFPlus.Patches.DoActionPatches;
using Mono.Cecil;
using MonoMod.Cil;

namespace BFPlus.Patches.CardGameTranspilers
{
    public class PatchLateUpdate : PatchBaseCardGameLateUpdate
    {
        public PatchLateUpdate()
        {
            priority = 201;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcR4(-0.5f));

            cursor.GotoPrev(i => i.MatchCallvirt(out _));
            int cursorIndex = cursor.Index;

            MethodReference getPosition = null;
            cursor.GotoPrev(i => i.MatchCallvirt(out getPosition), i => i.MatchLdloc(out _));
            cursor.Goto(cursorIndex);

            cursor.Next.Operand = getPosition;
        }
    }
}
