using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.AdvanceTurnEntityPatches
{
    public class PatchEatenDamage : PatchBaseAdvanceTurnEntity
    {
        public PatchEatenDamage()
        {
            priority = 282;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdfld(AccessTools.Field(typeof(MainManager.BattleData), "eatenby")));
            ILLabel label = null;
            cursor.GotoNext(MoveType.After, i => i.MatchBrfalse(out label));

            cursor.Emit(OpCodes.Ldarg_1);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchEatenDamage), "IsNotLifeStealEater"));
            cursor.Emit(OpCodes.Brtrue, label);
        }

        static bool IsNotLifeStealEater(ref MainManager.BattleData target)
        {
            if (target.eatenby != null)
            {
                int enemyID = MainManager.battle.GetEnemyID(target.eatenby.transform);

                if (enemyID < MainManager.battle.enemydata.Length)
                {
                    switch (MainManager.battle.enemydata[enemyID].animid)
                    {
                        case (int)NewEnemies.Jester:
                            return true;
                    }
                }
            }
            return false;
        }
    }

    public class PatchSpitOutEventOnDeath : PatchBaseAdvanceTurnEntity
    {
        public PatchSpitOutEventOnDeath()
        {
            priority = 383;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(19));
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchSpitOutEventOnDeath), "GetSpitOutEvent"));
            cursor.Remove();
        }

        static int GetSpitOutEvent(ref MainManager.BattleData target)
        {
            if (target.eatenby != null)
            {
                int enemyID = MainManager.battle.GetEnemyID(target.eatenby.transform);

                if (enemyID < MainManager.battle.enemydata.Length)
                {
                    switch (MainManager.battle.enemydata[enemyID].animid)
                    {
                        case (int)NewEnemies.Jester:
                            return (int)NewEventDialogue.JesterSpitout;
                    }
                }
            }
            return 19; //devourer event, default
        }


    }
}
