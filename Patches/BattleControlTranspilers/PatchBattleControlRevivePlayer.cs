using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers
{
    public class PatchRemoveLastWindRevivePlayer : PatchBaseBattleControlRevivePlayer
    {
        public PatchRemoveLastWindRevivePlayer()
        {
            priority = 69;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(87));
            cursor.Next.OpCode = OpCodes.Nop;
            cursor.GotoNext(i => i.MatchLdsfld(out _));
            Utils.RemoveUntilInst(cursor, i => i.MatchLdcI4(0));
            cursor.GotoNext(i => i.MatchLdcI4(-1));
            cursor.RemoveRange(2);
        }

    }
}
