using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine;

namespace BFPlus.Patches.MainManagerTranspilers
{
    public class PatchMaxItemSprites : PatchBaseMainManagerLoadItemSprites
    {
        public PatchMaxItemSprites()
        {
            priority = 18;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(256));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchMaxItemSprites), "GetNewItemMax"));
            cursor.Remove();
        }

        static int GetNewItemMax()
        {
            int totalItems = Enum.GetNames(typeof(MainManager.Items)).Length + Enum.GetNames(typeof(NewItem)).Length;
            int totalMedals = Enum.GetNames(typeof(MainManager.BadgeTypes)).Length + Enum.GetNames(typeof(Medal)).Length;
            return Mathf.Max(totalItems, totalMedals);
        }
    }
}
