using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.DoActionPatches.EnemyPatches
{
    public class PatchMantidFlyKidnapProperty : PatchBaseDoAction
    {
        public PatchMantidFlyKidnapProperty()
        {
            priority = 143240;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(1557));
            cursor.GotoNext(i => i.MatchLdfld(AccessTools.Field(typeof(BattleControl), "commandsuccess")));
            cursor.GotoPrev(i => i.MatchLdloca(out _));
            cursor.RemoveRange(3);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(Utils), "GetDizzyProperty"));
        }
    }
}
