using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.CheckDeadPatches
{
    public class PatchCotUnknownCheck : PatchBaseCheckDead
    {
        public PatchCotUnknownCheck()
        {
            priority = 155929;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdfld(AccessTools.Field(typeof(EntityControl), "cotunknown")));
            cursor.GotoPrev(i => i.MatchLdloc1());
            Utils.RemoveUntilInst(cursor, i => i.MatchLdsfld(out _));
        }

    }
}
