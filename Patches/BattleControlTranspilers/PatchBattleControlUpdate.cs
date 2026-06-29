using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers
{
    /// <summary>
    /// If we are in a first strike, make sure that we dont return out of update otherwise the action will be set to false in start battle. this happens
    /// when an enemy that is supposed to attack gets stopped by an item using an item that first struck
    /// </summary>
    public class PatchFirstStrikeCantMoved : PatchBaseBattleControlUpdate
    {
        public PatchFirstStrikeCantMoved()
        {
            priority = 586;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(1), i => i.MatchStfld(AccessTools.Field(typeof(MainManager.BattleData), "cantmove")));
            int cursorIndex = cursor.Index;
            ILLabel label = null;
            cursor.GotoNext(i => i.MatchBlt(out label));
            cursor.Goto(cursorIndex);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(BattleControl), "firststrike"));
            cursor.Emit(OpCodes.Brtrue, label);
        }
    }

    /// <summary>
    /// Add a set max options call before player turn because we want to remove setmaxoptiosn in playerturn
    /// </summary>
    public class PatchSetMaxOptions : PatchBaseBattleControlUpdate
    {
        public PatchSetMaxOptions()
        {
            priority = 404;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchCall(AccessTools.Method(typeof(BattleControl), nameof(BattleControl.PlayerTurn))));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl), nameof(BattleControl.SetMaxOptions)));
            cursor.Emit(OpCodes.Ldarg_0);
        }
    }
}
