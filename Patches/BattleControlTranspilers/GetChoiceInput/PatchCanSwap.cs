using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFPlus.Patches.BattleControlTranspilers.GetChoiceInput
{
    public class PatchCanSwitchPos : PatchBaseGetChoiceInput
    {
        public PatchCanSwitchPos()
        {
            priority = 850;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;
            cursor.GotoPrev(i => i.MatchCall(AccessTools.Method(typeof(BattleControl), "SwitchPos")));
            cursor.GotoPrev(MoveType.After, i => i.MatchBneUn(out label));

            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchCanSwitchPos), nameof(PlayerCanSwap)));
            cursor.Emit(OpCodes.Brfalse, label);
        }

        static bool PlayerCanSwap()
        {
            if (!BattleControl_Ext.CanSwap(MainManager.battle.option) || !BattleControl_Ext.CanSwap(MainManager.battle.currentturn))
            {
                MainManager.PlayBuzzer();
                return false;
            }
            return true;
        }

    }

    public class PatchCanSwitchParty : PatchBaseGetChoiceInput
    {
        public PatchCanSwitchParty()
        {
            priority = 482;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;
            cursor.GotoNext(i => i.MatchCall(AccessTools.Method(typeof(MainManager), "AllPartyFree")), i=>i.MatchBrtrue(out label));

            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), nameof(BattleControl_Ext.CanBypassSwapRestrictions)));
            cursor.Emit(OpCodes.Brtrue, label);

            cursor.GotoPrev(MoveType.After, i => i.MatchBrfalse(out label));
            cursor.GotoNext(MoveType.After, i => i.MatchLdarg0());

            cursor.Prev.OpCode = OpCodes.Nop;

            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchCanSwitchParty), nameof(CheckCanSwitchParty)));
            cursor.Emit(OpCodes.Brfalse, label);
            cursor.Emit(OpCodes.Ldarg_0);
        }

        static bool CheckCanSwitchParty()
        {
            List<int> canSwap = new List<int>();

            for(int i = 0; i < MainManager.instance.playerdata.Length; i++)
            {
                if(BattleControl_Ext.CanSwap(i))
                    canSwap.Add(i);
            }

            if(canSwap.Count == MainManager.instance.playerdata.Length)
                return true;

            MainManager.battle.StartCoroutine(MainManager.battle.SwitchPos(canSwap[0], canSwap.Count == 1 ? canSwap[0] : canSwap[1]));
            return false;
        }
    }
}
