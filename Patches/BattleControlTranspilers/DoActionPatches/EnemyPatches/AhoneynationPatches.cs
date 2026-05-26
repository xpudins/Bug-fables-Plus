using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.DoActionPatches.EnemyPatches
{
    public class PatchAhoneynationBigPunchProperty : PatchBaseDoAction
    {
        public PatchAhoneynationBigPunchProperty()
        {
            priority = 73613;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(394));
            cursor.GotoNext(i => i.MatchLdfld(AccessTools.Field(typeof(BattleControl), "commandsuccess")));
            cursor.GotoPrev(i => i.MatchLdloca(out _));
            cursor.RemoveRange(3);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(Utils), "GetDizzyProperty"));
        }
    }
}
