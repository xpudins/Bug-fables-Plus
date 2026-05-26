using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.ShowItemListPatches
{
    public class PatchBossNames : PatchBaseShowItemList
    {
        public PatchBossNames()
        {
            priority = 72090;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After,
                i => i.MatchLdcI4(77),
                i => i.MatchLdelemRef(),
                i => i.MatchStloc(out _),
                i => i.MatchBr(out _)
            );

            cursor.Next.OpCode = OpCodes.Nop;
            cursor.GotoNext(MoveType.After,
                i => i.MatchLdsfld(out _),
                i => i.MatchLdloc(out _),
                i => i.Match(OpCodes.Ldelem_I4)
            );
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "GetBossNames"));
            cursor.Remove();
        }
    }
}
