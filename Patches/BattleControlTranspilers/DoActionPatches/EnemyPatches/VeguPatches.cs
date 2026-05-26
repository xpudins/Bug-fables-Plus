using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.DoActionPatches.EnemyPatches
{
    public class PatchVeGuSwipeProperty : PatchBaseDoAction
    {
        public PatchVeGuSwipeProperty()
        {
            priority = 84714;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(584));
            cursor.GotoNext(i => i.MatchLdfld(AccessTools.Field(typeof(BattleControl), "commandsuccess")));
            cursor.GotoPrev(i => i.MatchInitobj(out _));
            cursor.Prev.OpCode = OpCodes.Nop;
            cursor.RemoveRange(2);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(Utils), "GetDizzyProperty"));
        }
    }
}
