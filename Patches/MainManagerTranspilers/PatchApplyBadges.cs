using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace BFPlus.Patches.MainManagerTranspilers
{
    public class PatchApplyBadges : PatchBaseApplyBadges
    {
        public PatchApplyBadges()
        {
            priority = 318;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = cursor.DefineLabel();
            ILLabel outLabel = null;
            cursor.GotoNext(i => i.MatchLdloc(out _), i => i.MatchLdcI4(9), i => i.MatchBeq(out _), i => i.MatchBr(out outLabel));

            int cursorIndex = cursor.Index;

            cursor.GotoPrev(i => i.MatchLdloc(out _), i => i.MatchLdcI4(0), i => i.MatchLdelemRef());
            var data = cursor.Next.Operand;

            cursor.Goto(cursorIndex);
            var badgeEffect = cursor.Next.Operand;


            cursor.Emit(OpCodes.Ldloc, badgeEffect);
            cursor.Emit(OpCodes.Ldc_I4, (int)MainManager.BadgeEffects.DefenseUp);
            cursor.Emit(OpCodes.Bne_Un, label);

            cursor.Emit(OpCodes.Ldloc, data);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchApplyBadges), "DoDefenseUpParty"));
            cursor.Emit(OpCodes.Br, outLabel);
            cursor.MarkLabel(label);

        }

        static void DoDefenseUpParty(string[] data)
        {
            for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
            {
                MainManager.instance.playerdata[i].def += Convert.ToInt32(data[1]);
            }
        }
    }
}
