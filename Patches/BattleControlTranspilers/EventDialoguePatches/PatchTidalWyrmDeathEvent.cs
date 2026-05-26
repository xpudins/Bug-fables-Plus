using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.EventDialoguePatches
{
    public class PatchTidalWyrmDeathEvent : PatchBaseEventDialogue
    {
        public PatchTidalWyrmDeathEvent()
        {
            priority = 7859;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdloc1(), i => i.MatchLdfld(AccessTools.Field(typeof(BattleControl), "cancelupdate")), i => i.MatchBrtrue(out _));
            cursor.GotoPrev(i => i.MatchLdarg0());
            cursor.RemoveRange(2);
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl), "AliveEnemies"));
            cursor.Emit(OpCodes.Ldc_I4_0);
            cursor.Next.OpCode = OpCodes.Bne_Un;
        }

    }
}
