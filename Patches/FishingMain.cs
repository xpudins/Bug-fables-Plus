using BFPlus.Extensions;
using HarmonyLib;

namespace BFPlus.Patches
{
    [HarmonyPatch(typeof(FishingMain), "Start")]
    public class PatchFishingMainStart
    {
        static void Prefix(FishingMain __instance)
        {
            if (!MainManager.instance.flags[616])
            {
                for (int i = 0; i < __instance.party.Length; i++)
                {
                    if (i <= 2)
                    {
                        if (MainManager.BadgeIsEquipped((int)Medal.Switcheroo))
                        {
                            int animId = 0;

                            if (i > 0)
                            {
                                animId = i == 1 ? 2 : 1;
                            }

                            __instance.party[i].runtimeAnimatorController = MainManager_Ext.Instance.GetSwitcherooAnim(animId);
                        }
                    }
                }
            }
        }

        static void Postfix(FishingMain __instance) 
        {
            FishingItems items = __instance.fishingItems;
            items.groups[0].items = items.groups[0].items.AddRangeToArray(GetNewBombItems());
            items.groups[3].items = items.groups[3].items.AddRangeToArray(GetNewNumbItems());
            items.groups[4].items = items.groups[4].items.AddRangeToArray(GetNewSleepItems());
        }

        static MainManager.Items[] GetNewBombItems()
        {
            return new MainManager.Items[] 
            { 
                (MainManager.Items)NewItem.CherryBomb2,
                (MainManager.Items)NewItem.FlameBomb,
                (MainManager.Items)NewItem.InkBomb,
                (MainManager.Items)NewItem.MysteryBomb,
                (MainManager.Items)NewItem.StickyBomb,
                (MainManager.Items)NewItem.WhirlyBomb,
            };
        }

        static MainManager.Items[] GetNewNumbItems()
        {
            return new MainManager.Items[]
            {
                (MainManager.Items)NewItem.BeeBattery,
                (MainManager.Items)NewItem.EnergyBar,
                (MainManager.Items)NewItem.SurgingSpud,
                (MainManager.Items)NewItem.JoltMush,
                (MainManager.Items)NewItem.DynamoDish
            };
        }

        static MainManager.Items[] GetNewSleepItems()
        {
            return new MainManager.Items[]
            {
                (MainManager.Items)NewItem.SleepingSquash,
                (MainManager.Items)NewItem.Napcap
            };
        }

    }
}
