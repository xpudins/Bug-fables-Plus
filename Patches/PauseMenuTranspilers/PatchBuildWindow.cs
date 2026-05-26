using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine;

namespace BFPlus.Patches.PauseMenuTranspilers
{
    public class PatchNewChallengeIcons : PatchBasePauseMenuBuildWindow
    {
        public PatchNewChallengeIcons()
        {
            priority = 1435;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdsfld(out _), i => i.MatchLdfld(out _), i => i.MatchLdcI4(616));
            cursor.Next.OpCode = OpCodes.Nop;
            cursor.GotoNext();

            cursor.Emit(OpCodes.Ldloc, cursor.Instrs[cursor.Index + 4].Operand);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PauseMenu_Ext), "SetBigFableIcon"));
            cursor.Emit(OpCodes.Ldsfld, AccessTools.Field(typeof(MainManager), "instance"));
        }
    }

    public class PatchMedalListType : PatchBasePauseMenuBuildWindow
    {
        public PatchMedalListType()
        {
            priority = 2472;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(3), i => i.MatchLdcI4(1), i => i.MatchLdcI4(0), i => i.MatchCall(AccessTools.Method(typeof(MainManager), "SetUpList", new Type[] { typeof(int), typeof(bool), typeof(bool) })));
            cursor.Next.OpCode = OpCodes.Ldc_I4;
            cursor.Next.Operand = (int)NewListType.MedalCategories;
        }
    }

    public class PatchBuildNewWindow : PatchBasePauseMenuBuildWindow
    {
        public PatchBuildNewWindow()
        {
            priority = 482;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchStfld(AccessTools.Field(typeof(PauseMenu), "secondoption")));

            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchBuildNewWindow), "BuildWindow"));
        }
        static void BuildWindow()
        {
            PauseMenu pauseMenu = MainManager.pausemenu;
            switch (pauseMenu.windowid)
            {
                case (int)NewWindowId.BalanceChanges:
                    MainManager.ResetList();
                    pauseMenu.boxes = new DialogueAnim[3];
                    pauseMenu.boxes[0] = MainManager.Create9Box(new Vector3(3.5f, -1f, 10f), new Vector2(10f, 7f), 1, -20, Color.white, true).GetComponent<DialogueAnim>();
                    pauseMenu.boxes[1] = MainManager.Create9Box(new Vector3(0f, 3.75f, 10f), new Vector2(12.5f, 2f), 4, -10, Color.white, true).GetComponent<DialogueAnim>();
                    pauseMenu.boxes[2] = MainManager.Create9Box(new Vector3(-5.25f, -1f, 10f), new Vector2(6f, 7f), 4, -20, Color.white, true).GetComponent<DialogueAnim>();

                    pauseMenu.StartCoroutine(MainManager.SetText("|single|" + MainManager.menutext[99], new Vector3(-2.35f, 2.5f), pauseMenu.boxes[2].transform));
                    new GameObject("returnbutton").AddComponent<ButtonSprite>().SetUp(5, -1, "|single|" + MainManager.menutext[45], new Vector3(-1.75f, 0f), Vector3.one * 0.5f, 5, pauseMenu.boxes[1].transform);

                    pauseMenu.option = 0;
                    pauseMenu.secondoption = -1;
                    break;
            }
        }
    }
}
