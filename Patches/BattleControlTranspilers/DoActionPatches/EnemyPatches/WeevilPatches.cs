using BFPlus.Extensions;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace BFPlus.Patches.DoActionPatches.EnemyPatches
{
    public class PatchWeevilCheckEnemy : PatchBaseDoAction
    {
        public PatchWeevilCheckEnemy()
        {
            priority = 86427;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(612));
            cursor.GotoNext(MoveType.After,
                i => i.MatchCall(AccessTools.Method(typeof(BattleControl), "EnemyInField", new Type[] { typeof(int), typeof(int[]) })),
                i => i.MatchStfld(out _)
            );
            Instruction stfldInst = cursor.Prev;

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchWeevilCheckEnemy), "CheckEnemyWeevilRef"));
            cursor.Emit(stfldInst.OpCode, stfldInst.Operand);
        }

        static int CheckEnemyWeevilRef()
        {
            var newEnemies = new int[] { (int)MainManager.Enemies.Seedling, (int)MainManager.Enemies.AngryPlant, (int)MainManager.Enemies.FlyTrap, (int)NewEnemies.Caveling };
            return MainManager.battle.EnemyInField(BattleControl_Ext.actionID, newEnemies);
        }
    }

    public class PatchWeevilBuffCheck : PatchBaseDoAction
    {
        public PatchWeevilBuffCheck()
        {
            priority = 86824;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(617));

            //change weevil bite property
            cursor.GotoNext(MoveType.After, i => i.MatchLdfld(AccessTools.Field(typeof(BattleControl), "playertargetID")), i => i.MatchLdcI4(3));
            cursor.RemoveRange(3);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "GetStickyProperty"));

            cursor.GotoNext(MoveType.After,
                i => i.MatchCall(AccessTools.Method(typeof(BattleControl), "Heal", new Type[] { typeof(MainManager.BattleData).MakeByRefType(), typeof(int?) })));
            Instruction ldfldRef = cursor.Body.Instructions[cursor.Index + 3];
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(ldfldRef.OpCode, ldfldRef.Operand);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchWeevilBuffCheck), "CheckWeevilEatBuff"));
        }

        static void CheckWeevilEatBuff(int enemyEaten)
        {
            if (MainManager.battle.enemydata[enemyEaten].animid == (int)NewEnemies.Caveling)
            {
                int actionId = BattleControl_Ext.actionID;
                MainManager.PlaySound("StatUp");
                MainManager.battle.dontusecharge = true;
                MainManager.battle.enemydata[actionId].charge =
                    BattleControl_Ext.Instance.GetMaxEnemyCharge(MainManager.battle.enemydata[actionId].battleentity);
                MainManager.battle.StartCoroutine(MainManager.battle.StatEffect(MainManager.battle.enemydata[actionId].battleentity, 4));
            }
        }
    }
}
