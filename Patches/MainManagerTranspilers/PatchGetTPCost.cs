using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace BFPlus.Patches
{
    public class PatchSetSkillListType : PatchBaseMainManagerGetTPCost
    {
        public PatchSetSkillListType()
        {
            priority = 0;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdfld(typeof(MainManager).GetField("playerdata")));
            cursor.GotoNext(MoveType.After, i => i.MatchLdfld(typeof(MainManager).GetField("playerdata")));
            cursor.Emit(OpCodes.Ldsfld, AccessTools.Field(typeof(MainManager_Ext), "skillListType"));
            cursor.Remove();
        }
    }

    /// <summary>
    /// Change tp cost for some of our new medals for certain skills
    /// </summary>
    public class PatchCheckSkillTPCost : PatchBaseMainManagerGetTPCost
    {
        public PatchCheckSkillTPCost()
        {
            priority = 127;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdloc1(), i => i.MatchLdcI4(25));
            cursor.GotoNext(i => i.MatchLdcI4(25));
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchCheckSkillTPCost), "GetNewTPCost"));
            cursor.Emit(OpCodes.Stloc_1);
            cursor.Emit(OpCodes.Ldloc_1);
        }

        static int GetNewTPCost(int tpCost, int skillId, int playerId)
        {
            if (skillId == (int)MainManager.Skills.Cleanse)
            {
                if (MainManager.BadgeIsEquipped((int)Medal.RinseRegen))
                {
                    tpCost += 1;
                }

                if (MainManager.BadgeIsEquipped((int)Medal.Liquidate))
                {
                    tpCost += 4;
                }
            }

            if (skillId == (int)MainManager.Skills.PeebleToss)
            {
                if (MainManager.BadgeIsEquipped((int)Medal.GrumbleGravel))
                {
                    tpCost += 1;
                }

                if (MainManager.BadgeIsEquipped((int)Medal.SkippingStone))
                {
                    tpCost += 2;
                }

                if (MainManager.BadgeIsEquipped((int)Medal.Avalanche))
                {
                    tpCost += 1;
                }

                if (MainManager.BadgeIsEquipped((int)Medal.TanjyToss))
                {
                    tpCost += 1;
                }

                if (MainManager.BadgeIsEquipped((int)Medal.RockyRampUp) && MainManager.battle != null)
                {
                    tpCost += Mathf.FloorToInt(BattleControl_Ext.Instance.rockyRampUpDmg / 2);
                }
            }
            if (skillId == (int)MainManager.Skills.PebbleTossPlus)
            {
                if (MainManager.BadgeIsEquipped((int)Medal.Avalanche))
                    tpCost += 1;
            }

            if (skillId == (int)MainManager.Skills.NeedleToss ||
                skillId == (int)MainManager.Skills.NeedlePincer ||
                skillId == (int)NewSkill.NeedleSurge)
            {
                if (MainManager.BadgeIsEquipped((int)MainManager.BadgeTypes.PoisonNeedle))
                    tpCost += 1;

                if (MainManager.BadgeIsEquipped((int)Medal.FrostNeedles))
                    tpCost += 1;
            }

            if (MainManager.BadgeIsEquipped((int)Medal.HornRattle))
            {
                if (skillId == (int)MainManager.Skills.HeavyStrike ||
                    skillId == (int)MainManager.Skills.HornDash ||
                    skillId == (int)MainManager.Skills.PebbleTossPlus ||
                    skillId == (int)MainManager.Skills.BeeFly)
                {
                    tpCost += 1;
                }
            }

            if (skillId == (int)MainManager.Skills.HeavyThrow &&
                MainManager.BadgeIsEquipped((int)MainManager.BadgeTypes.Beemerang2))
            {
                tpCost += 1;
            }

            if (skillId == (int)MainManager.Skills.HardCharge)
            {
                tpCost += 3 * MainManager.BadgeHowManyEquipped((int)Medal.Powerbank, playerId);
            }

            if (MainManager.HasCondition(MainManager.BattleCondition.Inked, MainManager.instance.playerdata[playerId]) > -1 && MainManager.BadgeIsEquipped((int)Medal.InvisibleInk))
                tpCost *= 2;

            return tpCost;
        }
    }
}
