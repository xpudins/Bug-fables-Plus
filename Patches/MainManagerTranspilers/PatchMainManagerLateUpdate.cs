using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.MainManagerTranspilers
{
    public class PatchMaxLevel : PatchBaseMainManagerLateUpdate
    {
        public PatchMaxLevel()
        {
            priority = 462;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdfld(AccessTools.Field(typeof(MainManager), "partylevel")));
            cursor.Emit(OpCodes.Ldc_I4, MainManager_Ext.newMaxLevel);
            cursor.Remove();
        }
    }
}
