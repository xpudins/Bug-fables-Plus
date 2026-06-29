using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.GetChoiceInput
{
    public class PatchDoubleDip : PatchBaseGetChoiceInput
    {
        public PatchDoubleDip()
        {
            priority = 288;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(19));

            //Flavorless adhesive item use check
            Utils.RemoveUntilInst(cursor, i => i.MatchBneUn(out _));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "CantUseItemSticky"));
            cursor.Next.OpCode = OpCodes.Brfalse;

            cursor.GotoNext(MoveType.After, i => i.MatchStfld(AccessTools.Field(typeof(BattleControl), "excludeself")));
            int cursorIndex = cursor.Index;
            ILLabel brLabel = null;
            cursor.GotoNext(i => i.MatchBr(out brLabel));
            cursor.Goto(cursorIndex);

            ILLabel label = cursor.DefineLabel();
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchDoubleDip), "HasGourmetStomach"));
            cursor.Emit(OpCodes.Brfalse, label);

            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchDoubleDip), "SetupGourmetList"));
            cursor.Emit(OpCodes.Br, brLabel);
            cursor.MarkLabel(label);
        }

        static bool HasGourmetStomach()
        {
            return MainManager.BadgeIsEquipped((int)Medal.GourmetStomach, MainManager.instance.playerdata[MainManager.battle.currentturn].trueid);
        }

        static void SetupGourmetList()
        {
            MainManager.SetUpList((int)NewListType.GourmetItem, true, false);
            MainManager.listammount = 5;
            MainManager.ShowItemList((int)NewListType.GourmetItem, MainManager.defaultlistpos, true, false);
            AccessTools.Method(typeof(BattleControl), "UpdateText").Invoke(MainManager.battle, null);
        }

    }
}
