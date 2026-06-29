using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.StartBattlePatches
{

    public class PatchRemoveLastWindStartBattle : PatchBaseStartBattle
    {
        public PatchRemoveLastWindStartBattle()
        {
            priority = 2842;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(87));
            cursor.Next.OpCode = OpCodes.Nop;
            cursor.GotoNext(i => i.MatchLdsfld(out _));
            Utils.RemoveUntilInst(cursor, i => i.MatchStfld(out _));
            cursor.Remove();
        }

    }
}
