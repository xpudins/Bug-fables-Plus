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

namespace BFPlus.Patches.BattleControlTranspilers.SetItemPatches
{

    public class PatchCanSwapSmallParty : PatchBaseSetItem
    {
        public PatchCanSwapSmallParty()
        {
            priority = 191456;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;
            cursor.GotoNext(MoveType.After,i=>i.MatchBr(out label), i => i.MatchLdstr("Switch"));
            cursor.Prev.OpCode = OpCodes.Nop;
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchCanSwapSmallParty), nameof(BattleControl_Ext.CanSwap)));
            cursor.Emit(OpCodes.Brfalse, label);
            cursor.Emit(OpCodes.Ldstr, "Switch");
        }

        static bool CanSwap()
        {
            if (!BattleControl_Ext.EveryoneCanSwap())
            {
                MainManager.PlayBuzzer();
                MainManager.battle.ReloadStrategy();
                MainManager.battle.Invoke("ReloadStrategy", 0.1f);
                return false;
            }
            return true;
        }

    }
}
