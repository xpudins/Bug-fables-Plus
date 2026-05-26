using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.MapControlTranspilers
{
    /// <summary>
    /// Add a check if we are in a hoaxe intermission, dont spawn chompy
    /// </summary>
    public class PatchChompyIntermission : PatchBaseMapControlLateUpdate
    {
        public PatchChompyIntermission()
        {
            priority = 171;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;
            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(402), i => i.MatchLdelemU1(), i => i.MatchBrfalse(out label));

            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchChompyIntermission), "CheckInIntermission"));
            cursor.Emit(OpCodes.Brtrue, label);
        }

        static bool CheckInIntermission()
        {
            return MainManager.instance.flags[916];
        }
    }
}
