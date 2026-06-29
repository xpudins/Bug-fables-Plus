using BFPlus.Extensions;
using BFPlus.Extensions.BattleStuff.Skills;
using BFPlus.Extensions.Stylish;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections;
using UnityEngine;

namespace BFPlus.Patches.DoActionPatches.StylishPatches
{
    public class PatchViBasicAttackSetStylish : PatchBaseDoAction
    {
        public PatchViBasicAttackSetStylish()
        {
            priority = 44847;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdcR4(0.1f), i => i.MatchCall(out _));

            ILLabel label = cursor.DefineLabel();
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "CheckTutorialStylish"));
            cursor.Emit(OpCodes.Brfalse, label);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchViBasicAttackSetStylish), "DoViStylishTutorial"));
            Utils.InsertYieldReturn(cursor);
            cursor.MarkLabel(label);
            Utils.InsertStartStylishTimer(cursor, 3f, 12f);
        }

        static IEnumerator DoViStylishTutorial()
        {
            yield return BattleControl_Ext.Instance.DoStylishTutorial(new ViStylish());
        }
    }
    public class PatchViBasicAttackWaitStylish : PatchBaseDoAction
    {
        public PatchViBasicAttackWaitStylish()
        {
            priority = 44877;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdfld(out _), i => i.MatchLdcI4(13));
            Utils.InsertWaitStylish(cursor);
            cursor.Emit(OpCodes.Ldarg_0);
        }
    }

    public class PatchViTornadoTossHitsSetStylish : PatchBaseDoAction
    {
        public PatchViTornadoTossHitsSetStylish()
        {
            priority = 59737;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(163));
            cursor.GotoNext(MoveType.After, i => i.MatchBlt(out _), i => i.MatchLdarg(0));
            cursor.Prev.OpCode = OpCodes.Nop;
            Utils.InsertStartStylishTimer(cursor, 3f, 20f, stylishID: 1, stylishGain: 0.02f);
            cursor.Emit(OpCodes.Ldarg_0);
        }
    }
    public class PatchViTornadoTossEndSetStylish : PatchBaseDoAction
    {
        public PatchViTornadoTossEndSetStylish()
        {
            priority = 60004;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(165));
            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(8), i => i.MatchCall(out _));
            Utils.InsertStartStylishTimer(cursor, 4f, 15f);
        }
    }
    public class PatchViTornadoTossEndWaitStylish : PatchBaseDoAction
    {
        public PatchViTornadoTossEndWaitStylish()
        {
            priority = 60051;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(166));
            cursor.GotoNext(i => i.MatchLdnull(), i => i.MatchStfld(out _));
            Utils.InsertWaitStylish(cursor);
            cursor.Emit(OpCodes.Ldarg_0);
        }
    }

    public class PatchViFlyJumpSetStylish : PatchBaseDoAction
    {
        public PatchViFlyJumpSetStylish()
        {
            priority = 56089;
        }

        protected override void ApplyPatch(ILCursor c, ILContext context)
        {
            c.GotoNext(i => i.MatchLdcI4(117));
            c.GotoNext(MoveType.After, i => i.MatchCall(AccessTools.Method(typeof(BattleControl), "MultiSkillMove")));

            Utils.InsertStartStylishTimer(c, 4f, 12f);
            c.Emit(OpCodes.Ldarg_0);
            Utils.InsertWaitStylish(c, 0.2f);
        }
    }

    public class PatchViStashSetStylish : PatchBaseDoAction
    {
        public PatchViStashSetStylish()
        {
            priority = 60912;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(176));
            cursor.GotoNext(MoveType.After, i => i.MatchConvI4(), i => i.MatchBlt(out _), i => i.MatchLdarg(0));
            cursor.Prev.OpCode = OpCodes.Nop;
            Utils.InsertStartStylishTimer(cursor, 4f, 14f, commandSuccess: false);
            cursor.Emit(OpCodes.Ldarg_0);
        }
    }

    public class PatchViHurricaneTossSetStylish : PatchBaseDoAction
    {
        public PatchViHurricaneTossSetStylish()
        {
            priority = 59337;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(156));
            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(8), i => i.MatchCall(out _));
            Utils.InsertStartStylishTimer(cursor, 4f, 12f, commandSuccess: false);
        }
    }
    public class PatchViHurricaneTossWaitStylish : PatchBaseDoAction
    {
        public PatchViHurricaneTossWaitStylish()
        {
            priority = 59386;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(158));
            cursor.GotoNext(i => i.MatchLdnull());

            Utils.InsertWaitStylish(cursor);
            cursor.Emit(OpCodes.Ldarg_0);
        }
    }

    public class PatchViFrostRelaySetStylish : PatchBaseDoAction
    {
        public PatchViFrostRelaySetStylish()
        {
            priority = 49266;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(57));
            cursor.GotoNext(MoveType.After, i => i.MatchCall(AccessTools.Method(typeof(UnityEngine.Object), 
                "Destroy", new Type[] { typeof(GameObject) })));
            Utils.InsertStartStylishTimer(cursor, 3f, 15f);
        }
    }
    public class PatchViFrostRelayWaitStylish : PatchBaseDoAction
    {
        public PatchViFrostRelayWaitStylish()
        {
            priority = 49325;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(58));
            cursor.GotoNext(MoveType.After, i => i.MatchCall(out _));
            cursor.Emit(OpCodes.Ldarg_0);
            Utils.InsertWaitStylish(cursor);
        }
    }

    public class PatchViHeavyThrowSetStylish : PatchBaseDoAction
    {
        public PatchViHeavyThrowSetStylish()
        {
            priority = 58550;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdstr("DLGammaStep"));
            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(100), i => i.MatchStfld(out _));
            Utils.InsertStartStylishTimer(cursor, 3f, 15f);
        }
    }
}