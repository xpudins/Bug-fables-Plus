using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.StartBattlePatches
{
    public class PatchAmbusherHitAction : PatchBaseStartBattle
    {
        public PatchAmbusherHitAction()
        {
            priority = 3785;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, j => j.MatchLdcI4(86));
            cursor.GotoNext(MoveType.After, j => j.MatchLdcI4(21));
            cursor.GotoNext(MoveType.After, i => i.MatchLdnull(), i => i.MatchStfld(out _));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchAmbusherHitAction), "SetHitAction"));
        }

        static void SetHitAction()
        {
            for (int i = 0; i < MainManager.battle.enemydata.Length; i++)
            {
                MainManager.battle.enemydata[i].hitaction = false;
            }
        }
    }
}
