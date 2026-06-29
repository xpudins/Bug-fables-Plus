using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine;

namespace BFPlus.Patches.CardGameTranspilers
{

    public class PatchCardOrder : PatchBaseCardGameStartCard
    {
        public PatchCardOrder()
        {
            priority = 300;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdstr("Data/CardOrder"));
            cursor.GotoNext(MoveType.After, i => i.MatchStloc2());
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchCardOrder), "GetNewCardOrder"));
            cursor.Emit(OpCodes.Stloc_2);
        }

        static string[] GetNewCardOrder()
        {
            return MainManager_Ext.assetBundle.LoadAsset<TextAsset>("CardOrder").ToString().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }

    public class PatchCardMusic : PatchBaseCardGameStartCard
    {
        public PatchCardMusic()
        {
            priority = 548;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdstr("Miniboss"));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchCardMusic), "CheckCardMusic"));
        }

        static string CheckCardMusic(string baseMusic)
        {
            if (MainManager.instance.cardgame.entities[3].animid == (int)NewAnimID.JumpAnt)
                return NewMusic.JumpAntTheme.ToString();
            return baseMusic;
        }
    }
}
