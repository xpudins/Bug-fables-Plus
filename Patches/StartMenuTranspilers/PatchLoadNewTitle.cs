using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.StartMenuTranspilers
{
    public class PatchLoadNewTitle : PatchBaseStartMenuIntro
    {
        public PatchLoadNewTitle()
        {
            priority = 131;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdstr(out _));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "LoadNewTitle"));
            cursor.RemoveRange(5);
        }
    }
}
