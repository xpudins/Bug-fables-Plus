using BFPlus.Extensions;
using BFPlus.Extensions.BattleStuff.StatusStuff;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.MainManagerTranspilers.UpdatePatches
{
    public class PatchDestinyDreamHUD : PatchBaseMainManagerUpdate
    {
        public PatchDestinyDreamHUD()
        {
            priority = 2100;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;
            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(48), i => i.MatchBeq(out label));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(Sleep), "DestinyDreamChangeHud"));
            cursor.Emit(OpCodes.Brtrue, label);
        }
    }
}
