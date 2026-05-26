using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.DoActionPatches.SkillPatches
{
    /// <summary>
    /// Ice rain hits 4 => 3
    /// </summary>
    public class PatchLeifIceRainHits : PatchBaseDoAction
    {
        public PatchLeifIceRainHits()
        {
            priority = 49495;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdstr("crosshair"));
            cursor.GotoNext(i => i.MatchLdcI4(4));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchLeifIceRainHits), "GetIceRainHits"));
            cursor.Remove();
        }

        static int GetIceRainHits()
        {
            if (MainManager_Ext.Instance.GetBalanceChangeState((int)NewMenuText.IceRain))
            {
                return 3;
            }
            return 4;
        }
    }


    /// <summary>
    /// we make ice rain cost 2 turns
    /// </summary>
    public class PatchLeifResetIceRainHits : PatchBaseDoAction
    {
        public PatchLeifResetIceRainHits()
        {
            priority = 49796;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(102), i => i.MatchStfld(out _));
            cursor.GotoNext(i => i.MatchLdcI4(65));
            cursor.GotoNext(i => i.MatchLdnull(), i => i.MatchStfld(out _));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchLeifResetIceRainHits), "ResetIceRainHits"));

        }

        static void ResetIceRainHits()
        {
            if (MainManager_Ext.Instance.GetBalanceChangeState((int)NewMenuText.IceRain))
            {
                MainManager.instance.playerdata[MainManager.battle.currentturn].cantmove++;
            }
        }
    }
}
