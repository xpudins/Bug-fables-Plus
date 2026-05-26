using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.MainManagerTranspilers.ShowItemListPatches
{

    public class PatchSettingsList : PatchBaseShowItemList
    {
        public PatchSettingsList()
        {
            priority = 72990;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;
            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(35), i => i.MatchBeq(out label));

            int index = cursor.Index;
            cursor.GotoNext(i => i.MatchLdloc(out _));
            var indexRef = cursor.Next.Operand;
            cursor.Goto(index);

            cursor.Emit(OpCodes.Ldloc, indexRef);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchSettingsList), "HasNoSlider"));
            cursor.Emit(OpCodes.Brtrue, label);
        }

        static bool HasNoSlider(int index)
        {
            return MainManager.settingsindex[MainManager.listvar[index]] == (int)NewMenuText.BalanceChanges;
        }
    }
}
