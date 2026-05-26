using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using static MainManager;

namespace BFPlus.Patches.ShowItemListPatches
{
    public class PatchNeedlesIcon : PatchBaseShowItemList
    {
        public PatchNeedlesIcon()
        {
            priority = 72421;
        }

        protected override void ApplyPatch(ILCursor c, ILContext context)
        {
            c.GotoNext(x => x.MatchLdcI4(45));
            c.GotoPrev(MoveType.After, x => x.MatchStloc(out _));

            c.Emit(c.Next.OpCode, c.Next.Operand);
            c.Emit(OpCodes.Ldloca, 30);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchNeedlesIcon), "NewSkillIcons"));

            c.GotoNext(MoveType.After, i => i.MatchLdcI4(45));

            c.GotoNext(i => i.MatchLdstr(" |size,0.55,0.6|"));
            c.Next.Operand = "|size,0.45,0.50|";

            ILLabel labelSkip = c.DefineLabel();

            c.GotoNext(x => x.MatchBrfalse(out labelSkip));
            c.GotoPrev(MoveType.After, x => x.MatchStloc(out _));

            c.Emit(OpCodes.Br, labelSkip); // skips the electric needles icon for needle skills

            c.GotoNext(x => x.MatchLdstr("|icon,193|"));
            c.GotoNext(MoveType.After, x => x.MatchStloc(out _));

            var labelIcon = c.DefineLabel();
            var otherLabelIcon = c.DefineLabel();
            labelSkip = c.DefineLabel();

            var badgeIsEquipRef = c.Body.Instructions[c.Index - 6];
            var readText3Ref = c.Body.Instructions[c.Index];
            var concatRef = c.Body.Instructions[c.Index - 2];
            var writeText3Ref = c.Body.Instructions[c.Index - 1];

            c.Body.Instructions[c.Index - 5].Operand = otherLabelIcon;
            c.Emit(OpCodes.Ldc_I4, (int)Medal.FrostNeedles);
            c.Emit(badgeIsEquipRef.OpCode, badgeIsEquipRef.Operand);
            c.Emit(OpCodes.Brfalse, labelIcon);

            c.Emit(readText3Ref.OpCode, readText3Ref.Operand);
            c.Emit(OpCodes.Ldstr, $"|icon,{(int)NewGui.FrostNeedles}|");
            c.Emit(concatRef.OpCode, concatRef.Operand);
            c.Emit(writeText3Ref.OpCode, writeText3Ref.Operand);

            c.Emit(OpCodes.Ldc_I4, (int)Medal.FireNeedles);

            c.Emit(badgeIsEquipRef.OpCode, badgeIsEquipRef.Operand);
            c.Emit(OpCodes.Brfalse, labelSkip);

            c.Emit(readText3Ref.OpCode, readText3Ref.Operand);
            c.Emit(OpCodes.Ldstr, $"|icon,{(int)NewGui.FireNeedles}|");
            c.Emit(concatRef.OpCode, concatRef.Operand);
            c.Emit(writeText3Ref.OpCode, writeText3Ref.Operand);

            c.MarkLabel(labelSkip);
            c.GotoPrev(i => i.MatchLdcI4((int)Medal.FireNeedles)).MarkLabel(labelIcon);
            c.GotoPrev(i => i.MatchLdcI4((int)Medal.FrostNeedles)).MarkLabel(otherLabelIcon);

            //adbp icon
            c.GotoNext(i => i.MatchLdcI4(28));
            c.GotoPrev(i => i.MatchLdstr(" |size,0.55,0.6|"));
            c.Next.Operand = "|size,0.40,0.45|";
        }

        static void NewSkillIcons(int skill, ref string text)
        {
            if (skill == (int)NewSkill.NeedleSurge)
            {
                string addText = null;

                if (BadgeIsEquipped((int)BadgeTypes.NumbNeedle))
                    addText += "|icon,191|";

                if (BadgeIsEquipped((int)BadgeTypes.PoisonNeedle))
                    addText += "|icon,193|";

                if (BadgeIsEquipped((int)Medal.FrostNeedles))
                    addText += $"|icon,{(int)NewGui.FrostNeedles}|";

                if (BadgeIsEquipped((int)Medal.FireNeedles))
                    addText += $"|icon,{(int)NewGui.FireNeedles}|";

                if (addText != null)
                    text = $"|size,0.40,0.45|{addText}";
            }

            if ((skill == (int)MainManager.Skills.HeavyThrow || skill == (int)NewSkill.Steal) 
                && BadgeIsEquipped((int)BadgeTypes.Beemerang2))
            {
                text += "|size,0.40,0.45||icon,194|";
            }
        }
    }
}
