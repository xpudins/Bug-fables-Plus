using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers
{

    public class PatchCheckCanUseSkill : PatchBaseBattleControlSetMaxOptions
    {
        public PatchCheckCanUseSkill()
        {
            priority = 113;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(18));
            Utils.RemoveUntilInst(cursor, i => i.MatchBle(out _));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "CantUseSkillInked"));
            cursor.Next.OpCode = OpCodes.Brtrue;
        }

    }

    public class PatchCheckCanUseItem : PatchBaseBattleControlSetMaxOptions
    {
        public PatchCheckCanUseItem()
        {
            priority = 165;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(19));
            Utils.RemoveUntilInst(cursor, i => i.MatchBle(out _));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "CantUseItemSticky"));
            cursor.Next.OpCode = OpCodes.Brtrue;
        }

    }

    public class PatchCheckCanUseAttack : PatchBaseBattleControlSetMaxOptions
    {
        public PatchCheckCanUseAttack()
        {
            priority = 207;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;

            cursor.GotoNext(MoveType.After, i => i.MatchLdfld(AccessTools.Field(typeof(MainManager.BattleData), "haspassed")), i => i.MatchBrtrue(out label));

            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "CantUseRelayDizzy"));
            cursor.Emit(OpCodes.Brtrue, label);
        }

    }
}
