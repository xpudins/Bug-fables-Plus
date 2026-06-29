using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.DoActionPatches.EnemyPatches
{

    public class PatchBanditShurikenProperty : PatchBaseDoAction
    {
        public PatchBanditShurikenProperty()
        {
            priority = 68818;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(317));
            cursor.GotoNext(i => i.MatchLdloca(out _));
            cursor.RemoveRange(3);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "GetStickyProperty"));
        }
    }
}
