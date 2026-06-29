using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers
{
    /// <summary>
    /// omegalul probably dont need to remove it and instead fix clean kill, but lazy ;)
    /// </summary>
    public class PatchBattleControlAddNewEnemy : PatchBaseBattleControlAddNewEnemy
    {
        public PatchBattleControlAddNewEnemy()
        {
            priority = 0;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            Utils.RemoveUntilInst(cursor, i => i.MatchBge(out _));
            cursor.Next.OpCode = OpCodes.Nop;
        }

    }
}
