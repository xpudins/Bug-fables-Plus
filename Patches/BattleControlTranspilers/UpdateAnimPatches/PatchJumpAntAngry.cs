using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.UpdateAnimPatches
{

    public class PatchJumpAntAngry : PatchBaseUpdateAnim
    {
        public PatchJumpAntAngry()
        {
            priority = 435;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdfld(AccessTools.Field(typeof(EntityControl), "basestate")));
            cursor.GotoPrev(i => i.MatchLdarg0());
            cursor.GotoPrev(MoveType.After, i => i.MatchLdarg0());

            var jumpLabel = cursor.Instrs[cursor.Index - 2].Operand;
            cursor.Prev.OpCode = OpCodes.Nop;
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchJumpAntAngry), "CheckAngryAnim"));
            cursor.Emit(OpCodes.Brtrue, jumpLabel);
            cursor.Emit(OpCodes.Ldarg_0);
        }

        static bool CheckAngryAnim(int enemyIndex)
        {
            if (MainManager.battle.enemydata[enemyIndex].animid == (int)NewEnemies.JumpAnt && MainManager.battle.HPPercent(MainManager.battle.enemydata[enemyIndex]) <= 0.2f)
            {
                MainManager.battle.enemydata[enemyIndex].battleentity.animstate = (int)MainManager.Animations.Angry;
                return true;
            }
            return false;
        }

    }
}
