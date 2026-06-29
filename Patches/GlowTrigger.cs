using BFPlus.Extensions;
using BFPlus.Extensions.MiscStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFPlus.Patches
{
    [HarmonyPatch(typeof(GlowTrigger), "Start")]
    public class PatchGlowTriggerStart
    {
        static void Postfix(GlowTrigger __instance)
        {
            MaterialCache.SetupCache(__instance.glowparts);
        }
    }
}
