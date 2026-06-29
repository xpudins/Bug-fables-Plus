using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.ShowItemListPatches
{
    public class PatchTextSkipOption : PatchBaseShowItemList
    {
        public PatchTextSkipOption()
        {
            priority = 73031;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdstr("slider"));
            int indexSliderComponent = cursor.Index + 4;

            cursor.GotoNext(i => i.MatchLdsfld(typeof(MainManager).GetField("listvar")));
            cursor.GotoNext(MoveType.After, i => i.MatchStloc(out _));

            cursor.Emit(OpCodes.Ldloc, cursor.Next.Operand);
            cursor.Emit(OpCodes.Ldloc, cursor.Body.Instructions[indexSliderComponent].Operand);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "SetOptionsText"));
        }
    }
}
