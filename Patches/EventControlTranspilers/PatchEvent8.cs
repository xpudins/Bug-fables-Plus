using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BFPlus.Patches.EventControlTranspilers
{
    //Adds a call to CheckNewCodeString, which checks if the filename = one of the new codes. return to the top of the loop if there is.
    public class PatchCheckNewCodes : PatchBaseEvent8
    {
        public PatchCheckNewCodes()
        {
            priority = 15506;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdsfld(out _), i => i.MatchLdfld(out _), i => i.MatchLdcI4(613));
            cursor.Next.OpCode = OpCodes.Nop;
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "CheckNewCodeString"));
            Utils.InsertYieldReturn(cursor);

            int cursorIndex = cursor.Index;
            ILLabel label = null;
            cursor.GotoNext(i => i.MatchBr(out label));
            cursor.Goto(cursorIndex);

            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "NewCodeUsed"));
            cursor.Emit(OpCodes.Brtrue, label);
            cursor.Emit(OpCodes.Ldsfld, AccessTools.Field(typeof(MainManager), "instance"));
            cursor.Remove();
        }
    }

    //Makes a new prompt for the new codes after the vanilla prompt.
    public class PatchNewCodePrompt : PatchBaseEvent8
    {
        public PatchNewCodePrompt()
        {
            priority = 15445;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdstr("ATKSuccess"));
            cursor.GotoNext(MoveType.After, i => i.MatchLdsfld(AccessTools.Field(typeof(MainManager), "instance")));
            cursor.Prev.OpCode = OpCodes.Nop;
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "DoNewChallengesPrompt"));
            Utils.InsertYieldReturn(cursor);
            cursor.Emit(OpCodes.Ldsfld, AccessTools.Field(typeof(MainManager), "instance"));
        }
    }

    /// <summary>
    /// Add new medals to mystery pool and remove mighty pebble from it
    /// </summary>
    public class PatchRemoveMightyPebbleMystery : PatchBaseEvent8
    {
        public PatchRemoveMightyPebbleMystery()
        {
            priority = 16116;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdfld(AccessTools.Field(typeof(EventControl), "mysterymedals")));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchRemoveMightyPebbleMystery), "AddMysteryMedals"));
        }

        static string AddMysteryMedals(string medals)
        {
            medals = medals.Replace(",13", "");
            medals += ',' + MainManager_Ext.GetNewMedalsString();

            return medals;
        }
    }

    public class PatchResetStylishOnNewSave : PatchBaseEvent8
    {
        public PatchResetStylishOnNewSave()
        {
            priority = 16203;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(694), i => i.MatchLdcI4(1), i => i.MatchStelemI1());
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchResetStylishOnNewSave), "ResetStylish"));
        }

        static void ResetStylish()
        {
            BattleControl_Ext.stylishBarAmount = 0;

            //save is up to date to 1.1 bf plus
            MainManager.instance.flags[983] = true;
        }
    }

}
