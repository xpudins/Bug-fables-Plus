using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Linq;
using static BattleControl;
using static MainManager;

namespace BFPlus.Patches.BattleControlTranspilers.CalculateBaseDamagePatches
{
    // COMPLETELY cuts out all instances of damage/defense modifiers in CalculateBaseDamage, aside from blocking and DefaultDamageCalc.
    // DefaultDamageCalc is set to Not Run in BattleControl.DefaultDamageCalc.Prefix.
    // damage and defense logic is replaced in BattleControl_Ext.GetFinalDMG
    public class PatchAtkDefCalcs : PatchBaseCalculateBaseDamage
    {
        public PatchAtkDefCalcs()
        {
            priority = 0;
        }

        public void AddSkip(ILCursor c, ILLabel label, Instruction prevInstr)
        {
            OpCode op = prevInstr.OpCode;
            var operand = prevInstr.Operand;
            c.Prev.OpCode = OpCodes.Nop;
            c.Emit(OpCodes.Br, label);
            c.Emit(op, operand);
        }

        protected override void ApplyPatch(ILCursor c, ILContext context)
        {
            ILLabel label = null;

            // skips whatever tf the first +2 dmg boost is
            c.GotoNext(x => x.MatchLdcI4(2));
            c.GotoPrev(x => x.MatchBneUn(out label));
            c.GotoPrev(MoveType.After, x => x.MatchLdloc(0));
            AddSkip(c, label, c.Prev);

           // skips vanilla stuff for exhaustion, poison attacker, last attack, front-of-party atk bonus, hard mode and hard hits dmg bonuses, charge, and some flags-related thing i don't know!!!
            c.GotoNext(x => x.MatchLdfld<BattleData>(nameof(BattleData.tired)));
            c.GotoPrev(x => x.MatchBrfalse(out label));
            c.GotoPrev(MoveType.After, x => x.MatchLdarga(out _));
            AddSkip(c, label, c.Prev);

            // skips frostbite dmg reduction against ranged dmg
            c.GotoNext(x => x.MatchDiv());
            c.GotoNext(x => x.MatchBr(out label));
            c.GotoPrev(MoveType.After, x => x.MatchLdarg(3));
            AddSkip(c, label, c.Prev);

            // skips freeze dmg increase
            c.GotoNext(MoveType.After, x => x.MatchLdarg(3));
            AddSkip(c, label, c.Prev);

            // skips numb defense
            c.GotoNext(x => x.MatchLdcI4(7));
            c.GotoPrev(x => x.MatchBle(out label));
            c.GotoPrev(MoveType.After, x => x.MatchLdcI4(2));
            AddSkip(c, label, c.Prev);

            // skips... whatever the hell the noexpatstart atk boost is.
            c.GotoNext(x => x.MatchLdfld<BattleData>(nameof(BattleData.noexpatstart)));
            c.GotoPrev(x => x.MatchBrtrue(out label));
            c.GotoPrev(x => x.MatchLdloc(0));
            c.Emit(OpCodes.Br, label);

            // skips flip logic entirely; now instead done right after damage pipeline
            c.GotoNext(x => x.MatchStloc(15));
            int returnHere = c.Index;
            c.GotoNext(x => x.MatchLdarga(out _));
            label = c.MarkLabel();
            c.Goto(returnHere);
            AddSkip(c, label, c.Prev);

            // skips the bonus damage from ice magic
            c.GotoNext(x => x.MatchCall<BattleControl>(nameof(BattleControl.AttackMagicProperty)));
            c.GotoPrev(x => x.MatchBrfalse(out label));
            c.GotoPrev(MoveType.After, x => x.MatchLdarga(out _));
            AddSkip(c, label, c.Prev);

            // skips EVERYTHING from atk/def up/down stuff to sturdy and reflection.
            c.GotoNext(x => x.MatchStloc(18));
            c.GotoPrev(x => x.MatchLdarga(out _));
            c.GotoPrev(MoveType.After, x => x.MatchLdarga(out _));
            int cursor = c.Index;
            c.GotoNext(x => x.MatchLdcI4(20));
            c.GotoPrev(x => x.MatchBrfalse(out label));
            c.Goto(cursor);
            AddSkip(c, label, c.Prev);

            c.Goto(0);
        }
    }
}