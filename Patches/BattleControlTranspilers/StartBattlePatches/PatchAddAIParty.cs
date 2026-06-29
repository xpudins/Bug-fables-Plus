using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;

namespace BFPlus.Patches.BattleControlTranspilers.StartBattlePatches
{
    public class PatchAddAIParty : PatchBaseStartBattle
    {
        public PatchAddAIParty()
        {
            priority = 3184;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(189));
            cursor.GotoNext(i => i.MatchLdloc(out _));

            var listRef = cursor.Next.Operand;

            cursor.GotoNext(i => i.MatchLdcI4(0), i => i.MatchLdloc(out _));
            cursor.Emit(OpCodes.Ldloc, listRef);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchAddAIParty), "CheckNewAi"));
            cursor.Emit(OpCodes.Stloc, listRef);
        }

        static List<int[]> CheckNewAi(List<int[]> aiList)
        {
            if (MainManager.BadgeIsEquipped((int)Medal.EverlastingFlame))
                return new List<int[]> { new int[] { (int)NewAnimID.Hoaxe, (int)MainManager.Animations.Flustered } };

            if (MainManager.instance.flags[982])
            {
                aiList.Add(new int[] { (int)NewAnimID.JumpAnt, (int)MainManager.Animations.BattleIdle });
            }

            return aiList;
        }

    }
}
