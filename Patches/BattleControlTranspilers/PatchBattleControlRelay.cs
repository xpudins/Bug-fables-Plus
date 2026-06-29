using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BFPlus.Extensions;
using BFPlus.Extensions.BattleStuff.StatusStuff;
using BFPlus.Patches.DoActionPatches;
using static MainManager;
using static BattleControl;

namespace BFPlus.Patches.BattleControlTranspilers
{
    public class PatchBattleControlRelay : PatchBaseBattleControlRelay
    {
        public PatchBattleControlRelay()
        {
            priority = 16185;
        }

        protected override void ApplyPatch(ILCursor c, ILContext context)
        {
            c.GotoNext(x => x.MatchLdstr("Relay"));
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchBattleControlRelay), nameof(CheckMidRelayEffects)));

            c.GotoNext(i => i.MatchLdcI4(45));
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchBattleControlRelay), nameof(CheckPostRelayEffects)));

            //Add check max charge for status relay with charges
            c.GotoNext(i => i.MatchAdd(), i => i.MatchLdcI4(0), i => i.MatchLdcI4(3));
            c.GotoNext(i => i.MatchLdcI4(3));

            c.Emit(OpCodes.Ldloc_1);
            c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(BattleControl), "option"));
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "CheckMaxCharge"));
            c.Remove();

            c.GotoNext(i => i.MatchCall(AccessTools.Method(typeof(BattleControl), "EndPlayerTurn")));
            c.Prev.OpCode = OpCodes.Nop;
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchBattleControlRelay), nameof(WaitForRelayEffects)));
            Utils.InsertYieldReturn(c);

            ILLabel label = c.DefineLabel();
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchBattleControlRelay), nameof(NoAliveEnemy)));
            c.Emit(OpCodes.Brfalse, label);
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.Ret);

            c.MarkLabel(label);
            c.Emit(OpCodes.Ldloc_1);
        }

        static void CheckMidRelayEffects()
        {
            waitForEffects = 0;
            int relayTarg = battle.option;
            if (HasCondition((BattleCondition)NewCondition.Dizzy, instance.playerdata[relayTarg]) > -1)
            {
                if (BadgeHowManyEquipped((int)Medal.Turnado, instance.playerdata[relayTarg].trueid) > 0 && battle.enemydata.Length > 0)
                    battle.StartCoroutine(Dizzy.DoTurnado(relayTarg));

                if (BadgeHowManyEquipped((int)Medal.Corkscrew, instance.playerdata[relayTarg].trueid) > 0)
                    battle.StartCoroutine(Dizzy.DoCorkscrew(relayTarg));
            }
        }
        static void CheckPostRelayEffects()
        {
            int relayUser = battle.currentturn;
            int relayTarg = battle.option;
            int spiderBaitEquipped = BadgeHowManyEquipped((int)Medal.SpiderBait, instance.playerdata[relayUser].trueid);
            if (HasCondition(BattleCondition.Sticky, instance.playerdata[relayUser]) > -1 && spiderBaitEquipped > 0)
            {
                battle.StartCoroutine(battle.ItemSpinAnim(instance.playerdata[relayUser].battleentity.transform.position + Vector3.up, itemsprites[1, (int)Medal.SpiderBait], true));
                BattleControl_Ext.Instance.ChargeUp(ref instance.playerdata[relayTarg], spiderBaitEquipped, 0.3f);
            }
        }
        static IEnumerator WaitForRelayEffects()
        {
            yield return new WaitUntil(() => waitForEffects <= 0);
        }
        public static int waitForEffects;

        static bool NoAliveEnemy()
        {
            return battle.AliveEnemies() == 0;
        }
    }
}