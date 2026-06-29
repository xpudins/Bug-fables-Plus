using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.WSA;
using static BattleControl;
using static MainManager;

namespace BFPlus.Patches.BattleControlTranspilers.AdvanceMainTurnPatches
{
    /// <summary>
    /// Dont move the proj if the position target is -1
    /// </summary>
    public class PatchDelProjMovement : PatchBaseAdvanceMainTurn
    {
        public PatchDelProjMovement()
        {
            priority = 10231;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchPop(), i => i.MatchLdloc1());
            cursor.Prev.OpCode = OpCodes.Nop;
            ILLabel label = cursor.DefineLabel();
            int cursorIndex = cursor.Index;

            cursor.GotoNext(MoveType.After, i => i.MatchLdarg0(), i => i.MatchLdfld(out _));
            var indexRef = cursor.Prev.Operand;
            cursor.Goto(cursorIndex);

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, indexRef);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchDelProjMovement), "CheckSkipMovement"));
            cursor.Emit(OpCodes.Brtrue, label);
            cursor.Emit(OpCodes.Ldloc_1);

            cursor.GotoNext(i => i.MatchLdfld(AccessTools.Field(typeof(BattleControl.DelayedProjectileData), "whilesound")));
            cursor.GotoPrev(i => i.MatchLdloc1());
            cursor.MarkLabel(label);

        }

        static bool CheckSkipMovement(int index)
        {
            return MainManager.battle.delprojs[index].areadamage > 0;
        }

    }

    public class PatchDelProjExtraEffects : PatchBaseAdvanceMainTurn
    {
        public PatchDelProjExtraEffects()
        {
            priority = 10360;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(x => x.MatchCall(AccessTools.Method(typeof(UnityEngine.Object), "Destroy", new Type[] { typeof(UnityEngine.Object) })));
            cursor.GotoPrev(MoveType.After, x => x.MatchLdarg(0));
            var currentDelProjIndex = cursor.Next.Operand;
            cursor.GotoPrev(MoveType.After, x => x.MatchLdloc(1));
            int destroyIndex = cursor.Index;

            cursor.GotoNext(x => x.MatchCall(AccessTools.Method(typeof(BattleControl), nameof(BattleControl.RemoveDelayedProjectile))));
            cursor.GotoPrev(x => x.MatchLdloc(1));
            ILLabel skipToLabel = cursor.MarkLabel();
            cursor.Goto(destroyIndex);

            cursor.Prev.OpCode = OpCodes.Nop;
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, currentDelProjIndex);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchDelProjExtraEffects), "DoExtraEffect"));
            Utils.InsertYieldReturn(cursor);
            cursor.Emit(OpCodes.Br, skipToLabel);
            cursor.Emit(OpCodes.Ldloc_1);
        }

        static IEnumerator DoExtraEffect(int projIndex)
        {
            GameObject obj = battle.delprojs[projIndex].obj;
            BattleControl_Ext.Instance.currentDelayedProj = obj.GetComponentInChildren<DelayedProjExtra>();
            if (BattleControl_Ext.Instance.currentDelayedProj != null)
                BattleControl_Ext.Instance.currentDelayedProj.transform.parent = battle.battlemap.transform;

            int mainTargetID = battle.partypointer[battle.delprojs[projIndex].position];
            int mainTargetDamage = 0;
            DamageOverride[] delProjOverrides = BattleControl_Ext.Instance.currentDelayedProj?.overrides?.ToArray();

            for (int i = 0; i < instance.playerdata.Length; i++)
            {
                if (instance.playerdata[i].hp <= 0 || instance.playerdata[i].eatenby != null)
                {
                    continue;
                }

                if (i == mainTargetID)
                {
                    mainTargetDamage = battle.delprojs[projIndex].damage + battle.delprojs[projIndex].areadamage;
                    mainTargetDamage = battle.DoDamage(null, ref instance.playerdata[i], mainTargetDamage, battle.delprojs[projIndex].property, delProjOverrides, battle.commandsuccess);
                }
                else if (battle.delprojs[projIndex].areadamage > 0)
                {
                    battle.DoDamage(null, ref instance.playerdata[i], battle.delprojs[projIndex].areadamage, battle.delprojs[projIndex].property, delProjOverrides, battle.commandsuccess);
                }

                if (instance.playerdata[i].hp <= 0)
                    instance.playerdata[i].turnssincedeath = -1;
            }
            UnityEngine.Object.Destroy(battle.delprojs[projIndex].obj);
            if (BattleControl_Ext.Instance.currentDelayedProj != null)
            {
                yield return BattleControl_Ext.Instance.currentDelayedProj.DoExtraEffect(mainTargetDamage, projIndex);
                UnityEngine.Object.Destroy(BattleControl_Ext.Instance.currentDelayedProj.gameObject);
            }
        }

    }
}
