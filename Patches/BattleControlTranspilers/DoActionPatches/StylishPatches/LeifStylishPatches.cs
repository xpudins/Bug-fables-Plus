using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BFPlus.Extensions;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using static MainManager;

namespace BFPlus.Patches.DoActionPatches.StylishPatches
{
    public class PatchLeifBasicAttackSetStylish : PatchBaseDoAction
    {
        public PatchLeifBasicAttackSetStylish()
        {
            priority = 45842;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After,
                x => x.MatchLdstr("IceMothHit"),
                x => x.MatchCall(out _),
                x => x.MatchPop());
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchLeifBasicAttackSetStylish), "SetStylishGain"));
        }

        public static void SetStylishGain()
        {
            float stylishGain = battle.selecteditem == (int)Skills.FrigidCoffin ? 0.05f : 0.1f;
            BattleControl_Ext.StartStylishTimer(15, 30, stylishGain: stylishGain);
        }
    }
    public class PatchLeifFrigidEndSetStylish : PatchBaseDoAction
    {
        public PatchLeifFrigidEndSetStylish()
        {
            priority = 46441;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(118), i => i.MatchStfld(out _));
            Utils.InsertStartStylishTimer(cursor, 4f, 20f, stylishID: 1, stylishGain: 0.05f);
        }
    }
    public class PatchLeifBasicAttackWaitStylish : PatchBaseDoAction
    {

        public PatchLeifBasicAttackWaitStylish()
        {
            priority = 46648;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdcR4(0.05f), i => i.MatchStfld(out _), i => i.MatchLdarg(0));
            Utils.InsertWaitStylish(cursor);
            cursor.Emit(OpCodes.Ldarg_0);
        }
    }

    public class PatchLeifIcefallSetStylish : PatchBaseDoAction
    {
        public PatchLeifIcefallSetStylish() : base()
        {
            priority = 50134;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(107), i => i.MatchStfld(out _), i => i.MatchLdarg(0));
            Utils.InsertStartStylishTimer(cursor, 20f, 30f, commandSuccess: false);
        }
    }
    public class PatchLeifIcefallWaitStylish : PatchBaseDoAction
    {
        public PatchLeifIcefallWaitStylish()
        {
            priority = 50363;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchInitobj(out _));
            cursor.Emit(OpCodes.Ldarg_0);
            Utils.InsertWaitStylish(cursor);
        }
    }

    public class PatchLeifBubbleSetStylish : PatchBaseDoAction
    {
        public PatchLeifBubbleSetStylish()
        {
            priority = 61100;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(178));
            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(102), i => i.MatchStfld(out _));
            Utils.InsertStartStylishTimer(cursor, 20f, 30f, commandSuccess: false);
        }
    }
    public class PatchLeifBuffsSetStylish : PatchBaseDoAction
    {
        public PatchLeifBuffsSetStylish()
        {
            priority = 51312;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdstr("Magic"), i => i.MatchCall(out _), i => i.MatchPop());
            Utils.InsertStartStylishTimer(cursor, 20f, 30f, stylishGain: 0.12f, commandSuccess: false);
        }
    }
    public class PatchLeifDebuffsSetStylish : PatchBaseDoAction
    {
        public PatchLeifDebuffsSetStylish()
        {
            priority = 51655;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(111), i => i.MatchStfld(out _), i => i.MatchLdstr("FastWoosh"), i => i.MatchCall(out _), i => i.MatchPop());
            Utils.InsertStartStylishTimer(cursor, 15f, 25f, stylishGain: 0.12f, commandSuccess: false);
        }
    }

    public class PatchLeifIceRainSetStylish : PatchBaseDoAction
    {
        public PatchLeifIceRainSetStylish()
        {
            priority = 49685;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(62));
            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(100), i => i.MatchStfld(out _));
            Utils.InsertStartStylishTimer(cursor, 4f, 12f, stylishGain: 0.067f, commandSuccess: false);
        }
    }

    public class PatchFrostBowlingSetStylish : PatchBaseDoAction
    {
        public PatchFrostBowlingSetStylish()
        {
            priority = 48218;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(42));
            cursor.GotoNext(MoveType.After, i => i.MatchLdnull(), i => i.MatchStfld(out _));
            Utils.InsertStartStylishTimer(cursor, 3f, 15f, stylishGain: 0.35f);
            cursor.Emit(OpCodes.Ldarg_0);
            Utils.InsertWaitStylish(cursor, 0.2f);
        }
    }
}