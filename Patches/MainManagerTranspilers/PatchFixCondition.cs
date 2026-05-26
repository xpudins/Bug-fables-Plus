using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using static MainManager;
namespace BFPlus.Patches.MainManagerTranspilers
{
    /// <summary>
    /// We Skip bubble shield getting set to 1 in fix condition to allow stacking
    /// </summary>
    public class PatchBubbleShieldTurn : PatchBaseMainManagerFixCondition
    {
        public PatchBubbleShieldTurn()
        {
            priority = 57;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdloc3(), i => i.MatchLdcI4(10), i => i.MatchBneUn(out _));
            Utils.RemoveUntilInst(cursor, i => i.MatchBneUn(out _));
            cursor.Next.OpCode = OpCodes.Br;

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchBubbleShieldTurn), nameof(CheckBubbleShieldTurn)));
        }

        static void CheckBubbleShieldTurn(MainManager.BattleData data, int conditionIndex)
        {
            if (data.condition[conditionIndex][0] == (int)BattleCondition.Shield)
            {
                int BUBBLE_SHIELD_MAX = 3;
                data.condition[conditionIndex][1] = Mathf.Clamp(data.condition[conditionIndex][1], 0, BUBBLE_SHIELD_MAX);
            }
        }
    }
}
