using BFPlus.Extensions;
using BFPlus.Extensions.BattleStuff.StatusStuff;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections;

namespace BFPlus.Patches.BattleControlTranspilers.CheckDeadPatches
{
    /// <summary>
    /// If a jump ant partner died, we check if we need to swap it so another partner
    /// </summary>
    public class PatchJumpAntPartnerDeath : PatchBaseCheckDead
    {
        public PatchJumpAntPartnerDeath()
        {
            priority = 156019;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After,
                i => i.MatchLdfld(AccessTools.Field(typeof(MainManager.BattleData), "eventondeath")),
                i => i.MatchLdcI4(-1)
            );

            cursor.GotoNext(MoveType.After, i => i.MatchStloc2());
            cursor.GotoNext(i => i.MatchLdelema(out _));

            var enemyIndexRef = cursor.Prev.Operand;

            cursor.GotoNext(MoveType.After, x => x.MatchPop());

            cursor.Emit(OpCodes.Ldloc, enemyIndexRef);
            cursor.Emit(OpCodes.Ldc_I4_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(Dizzy), "CheckDizzyKO"));

            cursor.GotoNext(MoveType.After,
                i => i.MatchLdfld(AccessTools.Field(typeof(BattleControl), "reservedata"))
            );
            cursor.GotoNext(i => i.MatchBr(out _));

            ILLabel label = cursor.DefineLabel();
            cursor.Emit(OpCodes.Ldloc, enemyIndexRef);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchJumpAntPartnerDeath), "CanSwapPartner"));
            cursor.Emit(OpCodes.Brfalse, label);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchJumpAntPartnerDeath), "SwapDeadPartner"));
            Utils.InsertYieldReturn(cursor);
            cursor.MarkLabel(label);
        }

        static void SetDeathPartnerType(int enemyIndex)
        {
            if (CanSwapPartner(enemyIndex))
            {
                MainManager.battle.enemydata[enemyIndex].battleentity.destroytype = (int)NPCControl.DeathType.None;
            }
        }

        static IEnumerator SwapDeadPartner()
        {
            yield return BattleControl_Ext.Instance.jumpAntFightComp.SwapPartner(true, true);
        }

        static bool CanSwapPartner(int enemyIndex)
        {
            bool isJumpAnt = MainManager.battle.enemydata[enemyIndex].animid == (int)NewEnemies.JumpAnt;
            bool isPartner = Entity_Ext.GetEntity_Ext(MainManager.battle.enemydata[enemyIndex].battleentity).isPartner;
            return !isJumpAnt && BattleControl_Ext.Instance.jumpAntFightComp != null && isPartner && BattleControl_Ext.Instance.jumpAntFightComp.HasPartnerAlive();
        }

    }
}
