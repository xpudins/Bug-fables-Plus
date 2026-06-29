using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.DoActionPatches
{

    public class YieldUseCharmHP : PatchBaseDoAction
    {
        public YieldUseCharmHP()
        {
            priority = 150794;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After,
                i => i.MatchLdcI4(3),
                i => i.MatchCall(AccessTools.Method(typeof(BattleControl), "UseCharm"))
            );
            cursor.Remove();
            cursor.Remove();
            Utils.InsertYieldReturn(cursor);
            int cursorIndex = cursor.Index;

            cursor.GotoPrev(i => i.MatchLdloc1(), i => i.MatchLdloc1(), i => i.MatchLdloc1());
            cursor.Remove();
            cursor.Remove();
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Goto(cursorIndex);
        }
    }

    public class YieldUseCharmDef : PatchBaseDoAction
    {
        public YieldUseCharmDef()
        {
            priority = 62804;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After,
                i => i.MatchLdcI4(0),
                i => i.MatchCall(AccessTools.Method(typeof(BattleControl), "UseCharm"))
            );
            cursor.Remove();
            cursor.Remove();
            Utils.InsertYieldReturn(cursor);
            int cursorIndex = cursor.Index;

            cursor.GotoPrev(i => i.MatchLdloc1(), i => i.MatchLdloc1(), i => i.MatchLdloc1());
            cursor.Remove();
            cursor.Remove();
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Goto(cursorIndex);
        }
    }
}
