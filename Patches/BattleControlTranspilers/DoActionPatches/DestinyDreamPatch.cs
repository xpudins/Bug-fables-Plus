using BFPlus.Extensions;
using BFPlus.Extensions.BattleStuff.StatusStuff;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
namespace BFPlus.Patches.DoActionPatches
{
    public class PatchDestinyDreamLifecast : PatchBaseDoAction
    {
        public PatchDestinyDreamLifecast()
        {
            priority = 44304;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(72));
            var label = cursor.DefineLabel();
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(Sleep), "DestinyDreamSetHP"));
            cursor.Emit(OpCodes.Brtrue, label);
            cursor.GotoNext(MoveType.Before, i => i.MatchLdarg0(), i => i.MatchLdsfld(out _), i => i.MatchLdfld(typeof(MainManager).GetField("flagvar")));
            cursor.MarkLabel(label);
        }
    }
}
