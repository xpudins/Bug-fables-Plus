using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace BFPlus.Patches.BattleControlTranspilers.AdvanceTurnEntityPatches
{
    public class PatchPoisonDMG : PatchBaseAdvanceTurnEntity
    {
        public PatchPoisonDMG()
        {
            priority = 146;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(27));
            cursor.GotoNext(i => i.MatchLdfld(AccessTools.Field(typeof(MainManager.BattleData), "maxhp")));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchPoisonDMG), "CheckPoisonDamage"));
            Utils.RemoveUntilInst(cursor, i => i.MatchLdcI4(13));

            //add status damage overide to overrides
            cursor.GotoNext(i => i.MatchLdcI4(0));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "GetStatusDamageOverrides"));
        }

        static int CheckPoisonDamage(ref MainManager.BattleData target)
        {
            int poisonDamage = 2;
            bool isPlayer = target.battleentity.CompareTag("Player");
            if (isPlayer)
            {
                int poisonAttackerBuff = MainManager.BadgeHowManyEquipped((int)MainManager.BadgeTypes.PoisonAttacker, target.trueid);
                if (MainManager.BadgeIsEquipped((int)MainManager.BadgeTypes.ReversePoison, target.trueid))
                    poisonDamage = Mathf.Max(0, poisonDamage - poisonAttackerBuff);
                else
                    poisonDamage += poisonAttackerBuff;

                return Mathf.Clamp(Mathf.CeilToInt((float)target.maxhp / 10f) - 1 + poisonDamage, poisonDamage, 99);
            }
            return Mathf.Clamp(Mathf.CeilToInt((float)target.maxhp / 10f) - 1 + poisonDamage, poisonDamage, poisonDamage + 2);
        }
    }
}
