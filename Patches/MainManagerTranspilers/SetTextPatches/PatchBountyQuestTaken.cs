using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Linq;

namespace BFPlus.Patches.MainManagerTranspilers.SetTextPatches
{
    /// <summary>
    /// Add the new bounties to the take all quests exception
    /// </summary>
    public class PatchBountyQuestTaken : PatchBaseSetText
    {
        public PatchBountyQuestTaken()
        {
            priority = 30035;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;
            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(23), i => i.MatchBeq(out label));

            var questsArrayRef = cursor.Instrs[cursor.Index - 5].Operand;
            var indexRef = cursor.Instrs[cursor.Index - 4].Operand;
            cursor.Emit(OpCodes.Ldloc, questsArrayRef);
            cursor.Emit(OpCodes.Ldloc, indexRef);
            cursor.Emit(OpCodes.Ldelem_I4);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchBountyQuestTaken), "IsBountyQuest"));
            cursor.Emit(OpCodes.Brtrue, label);
        }

        static bool IsBountyQuest(int questId)
        {
            return MainManager_Ext.GetNewBounties().Contains(questId);
        }
    }
}
