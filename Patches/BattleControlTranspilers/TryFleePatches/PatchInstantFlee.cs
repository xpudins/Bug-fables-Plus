using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.TryFleePatches
{
    public class PatchInstantFlee : PatchBaseTryFlee
    {
        public PatchInstantFlee()
        {
            priority = 154603;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcR4(0.4f));
            cursor.Prev.OpCode = OpCodes.Nop;

            var label = cursor.DefineLabel();
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchInstantFlee), "CheckInstantFlee"));
            cursor.Emit(OpCodes.Brtrue, label);
            cursor.Emit(OpCodes.Ldarg_0);

            cursor.GotoNext(i => i.MatchStsfld(AccessTools.Field(typeof(MainManager), "battleresult")));
            cursor.GotoPrev().MarkLabel(label);
        }

        static bool CheckInstantFlee()
        {
            return MainManager.instance.inevent && MainManager.lastevent == 42;
        }

    }
}
