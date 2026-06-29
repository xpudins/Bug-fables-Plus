using BFPlus.Extensions;
using BFPlus.Extensions.Events;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BFPlus.Patches
{

    [HarmonyPatch(typeof(EventControl), "ChompyRibbons")]
    public class PatchEventControlChompyRibbons
    {
        static void Postfix(EventControl __instance, ref int[] __result)
        {
            if (MainManager.instance.items[1].Contains((int)NewItem.WingRibbon))
            {
                __result = __result.AddToArray((int)NewItem.WingRibbon);
            }
        }
    }

    [HarmonyPatch(typeof(EventControl), "StartEvent")]
    public class PatchEventControlStartEvent
    {
        static void Postfix(EventControl __instance, int id, NPCControl caller)
        {
            if (id == 82)
            {
                MainManager_Ext.CheckWellRestedAchievement(true);
            }

            if (Enum.IsDefined(typeof(NewEvents), id))
            {
                __instance.StartCoroutine(EventFactory.GetEventClass((NewEvents)id).StartEvent(caller, __instance));
            }

            ///Idk why but they set the emoticonoffset in startevent in relation to animid so hoaxe gets a fucked up emoticon offset
            if (MainManager.instance.playerdata != null && MainManager.instance.playerdata.Length == 1 && MainManager.instance.playerdata[0].entity != null)
            {
                if (MainManager.instance.playerdata[0].entity.animid == (int)NewAnimID.Hoaxe)
                    MainManager.instance.playerdata[0].entity.emoticonoffset = new Vector3(0, 2.2f, -0.1f);
            }

            if (MainManager.player != null)
            {
                for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                {
                    if (MainManager.instance.playerdata[i].entity != null)
                    {
                        MainManager.instance.playerdata[i].entity.overrridejump = false;
                        MainManager.instance.playerdata[i].entity.overrideanim = false;
                        MainManager.instance.playerdata[i].entity.animstate = 0;
                        MainManager.instance.playerdata[i].entity.SetAnimForce();
                    }
                }
            }

            if(MainManager.map != null && MainManager.map.chompy != null)
            {
                MainManager.map.chompy.overrridejump = false;
                MainManager.map.chompy.overrideanim = false;
                MainManager.map.chompy.animstate = 0;
                MainManager.map.chompy.SetAnimForce();
            }
        }
    }

    //mr tester event
    [HarmonyPatch(typeof(EventControl), "Event42")]
    public class PatchEventControlEvent42
    {
        static IEnumerator AddPostfix(EventControl __instance, IEnumerator __result)
        {
            int tp = MainManager.instance.tp;
            float stylishAmount = BattleControl_Ext.stylishBarAmount;
            StylishReward reward = BattleControl_Ext.stylishReward;
            int[] hpValues = new int[MainManager.instance.playerdata.Length];

            for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
            {
                hpValues[i] = MainManager.instance.playerdata[i].hp;
            }

            int[] items = MainManager.instance.items[0].ToArray();

            while (__result.MoveNext())
            {
                yield return __result.Current;
            }

            if (MainManager.battleresult && !MainManager.battlefled)
            {
                MainManager.instance.flags[954] = true;
            }

            for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
            {
                MainManager.instance.playerdata[i].hp = hpValues[i];
            }
            BattleControl_Ext.stylishBarAmount = stylishAmount;
            BattleControl_Ext.stylishReward = reward;
            MainManager.instance.items[0] = new List<int>(items);

            MainManager.instance.tp = tp;
        }

        static void Postfix(EventControl __instance, ref IEnumerator __result)
        {
            __result = AddPostfix(__instance, __result);
        }
    }

    //Aria event
    [HarmonyPatch(typeof(EventControl), "Event58")]
    public class PatchEventControlEvent58
    {
        static IEnumerator AddPostfix(EventControl __instance, IEnumerator __result)
        {
            while (__result.MoveNext())
            {
                yield return __result.Current;
            }

            MainManager.AddPrizeMedal((int)NewPrizeFlag.Aria);
        }

        static void Postfix(EventControl __instance, ref IEnumerator __result)
        {
            __result = AddPostfix(__instance, __result);
        }
    }
}