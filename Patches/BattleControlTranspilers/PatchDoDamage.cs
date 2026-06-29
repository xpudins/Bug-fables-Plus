using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Linq;
using UnityEngine;
using static BattleControl;

namespace BFPlus.Patches.BattleControlTranspilers
{
    public class PatchCheckSpiderBait : PatchBaseBattleControlDoDamage
    {
        public PatchCheckSpiderBait()
        {
            priority = 137;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            int blockRef = -1;

            ILLabel label = cursor.DefineLabel();
            cursor.GotoNext(MoveType.After, i => i.MatchStarg(out blockRef), i => i.MatchLdcI4(0));
            cursor.Prev.OpCode = OpCodes.Nop;

            cursor.Emit(OpCodes.Ldarg_2);
            cursor.Emit(OpCodes.Ldarg, blockRef);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchCheckSpiderBait), "CheckSpiderBait"));
            cursor.Emit(OpCodes.Brfalse, label);
            cursor.Emit(OpCodes.Ldc_I4_0);
            cursor.Emit(OpCodes.Starg, blockRef).MarkLabel(label);
            cursor.Emit(OpCodes.Ldc_I4_0);
        }

        static bool CheckSpiderBait(ref MainManager.BattleData target, bool block)
        {
            var getSuperBlockRef = AccessTools.Method(typeof(BattleControl), "GetSuperBlock");
            bool superblocked = (bool)getSuperBlockRef.Invoke(MainManager.battle, new object[] { target.battleentity.animid });
            return block && MainManager.HasCondition(MainManager.BattleCondition.Sticky, target) > -1 
                && MainManager.BadgeIsEquipped((int)Medal.SpiderBait, target.trueid) && !superblocked;
        }
    }

    public class PatchCheckSlugskinSpikyBod : PatchBaseBattleControlDoDamage
    {
        public PatchCheckSlugskinSpikyBod()
        {
            priority = 428;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdstr("Damage0"));
            cursor.GotoNext(i => i.MatchLdcI4(1));
            cursor.Emit(OpCodes.Ldarg_2);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchCheckSlugskinSpikyBod), "CheckSlugskin"));
            cursor.Remove();
        }

        static int CheckSlugskin(ref MainManager.BattleData target)
        {
            if (Entity_Ext.GetEntity_Ext(target.battleentity).slugskinActive)
                return 2;
            return 1;
        }
    }

    public class PatchFlipNoPierceFlip : PatchBaseBattleControlDoDamage
    {
        public PatchFlipNoPierceFlip()
        {
            priority = 583;
        }

        protected override void ApplyPatch(ILCursor c, ILContext context)
        {
            ILLabel label = null;
            c.GotoNext(MoveType.After, x => x.MatchStloc(22));
            c.GotoNext(x => x.MatchBle(out label));
            c.GotoNext(x => x.MatchLdarg(2));
            int returnHere = c.Index;
            c.GotoPrev(x => x.MatchCall(AccessTools.Method(typeof(BattleControl), "CalculateBaseDamage")));
            object overridesRef = c.Prev.Operand;
            c.Goto(returnHere);

            c.Emit(OpCodes.Ldarg, overridesRef);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchFlipNoPierceFlip), nameof(HasFlipNoPierce)));
            c.Emit(OpCodes.Brtrue, label);
        }

        static bool HasFlipNoPierce(DamageOverride[] overrides) => 
            overrides?.Contains((DamageOverride)NewDamageOverride.FlipNoPierce) ?? false;
    }

    public class PatchRemoveHoloViPoisonBonus : PatchBaseBattleControlDoDamage
    {
        public PatchRemoveHoloViPoisonBonus()
        {
            priority = 805;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;
            cursor.GotoNext(x => x.MatchLdcI4(32));
            cursor.GotoPrev(x => x.MatchBrtrue(out label));
            cursor.GotoPrev(MoveType.After, x => x.MatchLdarg(0));
            cursor.Prev.OpCode = OpCodes.Nop;
            cursor.Emit(OpCodes.Br, label);
            cursor.Emit(OpCodes.Ldarg_0);
        }
    }

    public class PatchFlipNoPierceFlyOnFlip : PatchBaseBattleControlDoDamage
    {
        public PatchFlipNoPierceFlyOnFlip()
        {
            priority = 1062;
        }

        protected override void ApplyPatch(ILCursor c, ILContext context)
        {
            ILLabel label = null;
            c.GotoNext(x => x.MatchLdcI4(19));
            c.GotoNext(x => x.MatchBrfalse(out label));
            c.GotoNext(MoveType.After, x => x.MatchLdarga(out _));
            int returnHere = c.Index;

            c.GotoPrev(x => x.MatchCall(AccessTools.Method(typeof(BattleControl), "CalculateBaseDamage")));
            object overridesRef = c.Prev.Operand;
            c.Goto(returnHere);

            c.RemoveRange(6);

            c.Prev.OpCode = OpCodes.Ldarg;
            c.Emit(OpCodes.Ldarg, overridesRef);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchFlipNoPierceFlyOnFlip), nameof(HasFlip)));
            c.Emit(OpCodes.Brfalse, label);
        }

        static bool HasFlip(AttackProperty? property, DamageOverride[] overrides)
        {
            if ((property != null && property.Value == AttackProperty.Flip) 
                || (overrides?.Contains((DamageOverride)NewDamageOverride.FlipNoPierce) ?? false))
            {
                return true;
            }
            return false;
        }
    }

    public class PatchFixSleepCantMove : PatchBaseBattleControlDoDamage
    {
        public PatchFixSleepCantMove()
        {
            priority = 1176;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(1), i => i.MatchStfld(AccessTools.Field(typeof(MainManager.BattleData), "cantmove")));

            int cursorIndex = cursor.Index;
            cursor.GotoPrev(i => i.MatchBrfalse(out _));
            var playerFlag = cursor.Prev.Operand;
            cursor.Goto(cursorIndex);

            cursor.Emit(OpCodes.Ldloc, playerFlag);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchFixSleepCantMove), "CheckSleepCantMove"));
            cursor.Remove();
        }

        static int CheckSleepCantMove(bool isPlayer)
        {
            if (isPlayer)
                return 1;
            return 0;
        }
    }

    public class PatchSkipDizzyLastDamage : PatchBaseBattleControlDoDamage
    {
        public PatchSkipDizzyLastDamage()
        {
            priority = 1426;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(
                i => i.MatchLdarg0(),
                i => i.MatchLdloc(out _),
                i => i.MatchStfld(AccessTools.Field(typeof(BattleControl), "lastdamage"))
                );

            ILLabel label = cursor.DefineLabel();

            int index = cursor.Index;
            int overridesRef = 0;
            cursor.GotoPrev(i => i.MatchLdcI4(10), i => i.MatchSub());
            cursor.GotoPrev(i => i.MatchLdarg(out overridesRef));
            cursor.Goto(index);


            cursor.Emit(OpCodes.Ldarg, overridesRef);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchSkipDizzyLastDamage), nameof(CanUpdateLastDamage)));
            cursor.Emit(OpCodes.Brfalse, label);

            cursor.GotoNext(MoveType.After,i => i.MatchStfld(AccessTools.Field(typeof(BattleControl), "lastdamage")));
            cursor.MarkLabel(label);
        }

        static bool CanUpdateLastDamage(DamageOverride[] overrides) 
            => overrides == null || !overrides.Contains((DamageOverride)NewDamageOverride.StatusDamage);
    }
}
