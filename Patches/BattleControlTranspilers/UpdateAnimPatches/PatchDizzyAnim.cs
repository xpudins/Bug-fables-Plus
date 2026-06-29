using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.UpdateAnimPatches
{
    /// <summary>
    /// Check of if the player is dizzy, if so set the corresponding animstate
    /// </summary>
    public class PatchPlayerDizzyAnim : PatchBaseUpdateAnim
    {
        public PatchPlayerDizzyAnim()
        {
            priority = 204;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;
            cursor.GotoNext(MoveType.After,
                i => i.MatchLdcI4(14),
                i => i.MatchStfld(out _),
                i => i.MatchBr(out label),
                i => i.MatchLdsfld(out _));

            cursor.Prev.OpCode = OpCodes.Nop;

            cursor.Emit(OpCodes.Ldloc_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchPlayerDizzyAnim), "CheckDizzyAnim"));
            cursor.Emit(OpCodes.Brtrue, label);
            cursor.Emit(OpCodes.Ldsfld, AccessTools.Field(typeof(MainManager), "instance"));
        }

        static bool CheckDizzyAnim(int playerIndex)
        {
            if (MainManager.HasCondition((MainManager.BattleCondition)NewCondition.Dizzy, MainManager.instance.playerdata[playerIndex]) > -1)
            {
                MainManager.instance.playerdata[playerIndex].battleentity.animstate = (int)MainManager.Animations.Woobly;
                return true;
            }
            return false;
        }

    }

    public class PatchEnemyDizzyAnim : PatchBaseUpdateAnim
    {
        public PatchEnemyDizzyAnim()
        {
            priority = 388;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;
            cursor.GotoNext(MoveType.After,
                i => i.MatchLdcI4(14),
                i => i.MatchStfld(out _),
                i => i.MatchBr(out label),
                i => i.MatchLdarg0());

            cursor.Prev.OpCode = OpCodes.Nop;

            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchEnemyDizzyAnim), "CheckDizzyAnim"));
            cursor.Emit(OpCodes.Brtrue, label);
            cursor.Emit(OpCodes.Ldarg_0);
        }

        static bool CheckDizzyAnim(int enemyId)
        {
            if (MainManager.HasCondition((MainManager.BattleCondition)NewCondition.Dizzy, MainManager.battle.enemydata[enemyId]) > -1
                && MainManager.battle.enemydata[enemyId].position != BattleControl.BattlePosition.Underground
                && !MainManager.battle.enemydata[enemyId].isdefending)
            {
                if (Entity_Ext.GetEntity_Ext(MainManager.battle.enemydata[enemyId].battleentity).hasDizzyAnim)
                    MainManager.battle.enemydata[enemyId].battleentity.animstate = (int)MainManager.Animations.Woobly;
                else
                    MainManager.battle.enemydata[enemyId].battleentity.animstate = (int)MainManager.Animations.Hurt;
                return true;
            }
            return false;
        }

    }


}
