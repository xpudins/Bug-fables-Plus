using BFPlus.Extensions;
using HarmonyLib;
using UnityEngine;

namespace BFPlus.Patches
{
    [HarmonyPatch(typeof(MusicSpinner), "OnTriggerEnter")]
    public class PatchMusicSpinnerOnTriggerEnter
    {
        static void Postfix(MusicSpinner __instance, Collider other)
        {
            if ((int)MainManager.map.mapid == (int)NewMaps.GiantLairPlayroomBoss)
            {
                if (__instance.spin + __instance.spinhit > __instance.spinlimit)
                {
                    MainManager.events.StartEvent((int)NewEvents.PlayroomBoss, null);
                    UnityEngine.Object.Destroy(__instance);
                }
            }
        }
    }
}
