using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine;

namespace BFPlus.Patches.MainManagerTranspilers
{
    public class PatchLoopPoint : PatchBaseMainManagerLoopPoint
    {
        public PatchLoopPoint()
        {
            priority = 26;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchStloc0());
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchLoopPoint), "AddNewLoopPoints"));
        }

        static string[] AddNewLoopPoints(string[] oldLoopPoints)
        {
            return oldLoopPoints.AddRangeToArray(MainManager_Ext.assetBundle.LoadAsset<TextAsset>("LoopPoints").ToString().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
