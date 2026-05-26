using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.NPCControlTranspilers
{
    /// <summary>
    /// Make sure that everlasting flame doesnt get randomized
    /// </summary>
    public class PatchCheckItem : PatchBaseNPCControlCheckItem
    {
        public PatchCheckItem()
        {
            priority = 5401;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;
            cursor.GotoNext(i => i.MatchLdcI4(59), i => i.MatchBneUn(out label));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchCheckItem), "CheckMedalId"));
            cursor.Emit(OpCodes.Brfalse, label);
            cursor.RemoveRange(2);
        }

        static bool CheckMedalId(int id)
        {
            return id == (int)Medal.EverlastingFlame || id == (int)MainManager.BadgeTypes.FreezeTime;
        }
    }
}
