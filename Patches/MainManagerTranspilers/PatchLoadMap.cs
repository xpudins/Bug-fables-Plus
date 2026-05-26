using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.MainManagerTranspilers
{

    public class PatchCheckCustomMap : PatchBaseMainManagerLoadMap
    {
        public PatchCheckCustomMap()
        {
            priority = 0;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdstr("Prefabs/Maps/"));
            cursor.Prev.OpCode = OpCodes.Nop;
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "CheckCustomMap"));
            Utils.RemoveUntilInst(cursor, i => i.MatchLdsfld(out _));
        }
    }
}
