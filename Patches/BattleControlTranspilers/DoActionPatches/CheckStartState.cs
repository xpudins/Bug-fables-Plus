using BFPlus.Extensions;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.DoActionPatches
{
    public class PatchCheckStartState : PatchBaseDoAction
    {
        public PatchCheckStartState()
        {
            priority = 150051;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdfld(typeof(EntityControl).GetField("walkstate")), i => i.MatchLdarg0(), i => i.MatchLdfld(out _));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "CheckStartState"));
        }
    }
}
