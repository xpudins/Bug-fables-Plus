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

namespace BFPlus.Patches.BattleControlTranspilers.StartBattlePatches
{
    public class PatchStartMusic : PatchBaseStartBattle
    {
        public PatchStartMusic()
        {
            priority = 4523;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i=>i.MatchLdcI4(1), 
                i => i.MatchStfld(AccessTools.Field(typeof(BattleControl), "saveddata")));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchStartMusic), nameof(SetBattleMusic)));
        }

        static void SetBattleMusic()
        {
            MainManager.battle.sdata.music = BattleControl_Ext.Instance.battleMusic;
        }

    }
}
