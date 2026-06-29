using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.EventControlTranspilers
{
    //PATTONS EVENT
    //Sub the MP Plus Medal bonuses from the max mp in ruigee, preventing a infinite amount of mp gains with pattons
    public class PatchPattonsMPPlus : PatchBaseEvent197
    {
        public PatchPattonsMPPlus()
        {
            priority = 0;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            while (cursor.TryGotoNext(MoveType.After, i => i.MatchLdfld(AccessTools.Field(typeof(MainManager), "maxbp"))))
            {
                cursor.Emit(OpCodes.Ldsfld, AccessTools.Field(typeof(MainManager_Ext), "mpPlusBonus"));
                cursor.Emit(OpCodes.Sub);
            }
            cursor.Goto(0);
        }
    }

    /// <summary>
    /// Patch the correct dialogue id, if you have the quest or not
    /// </summary>
    public class PatchPattonsQuestDialogue : PatchBaseEvent197
    {
        public PatchPattonsQuestDialogue()
        {
            priority = 228218;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(6));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchPattonsQuestDialogue), "GetDialogueId"));
            cursor.Remove();
        }

        static int GetDialogueId()
        {
            if (MainManager.instance.flags[933] && !MainManager.instance.flags[934])
            {
                return 23;
            }
            return 6;
        }
    }

    /// <summary>
    /// If the third option is selected and we are in a pattons quest state, start the patton quest event
    /// </summary>
    public class PatchPattonsQuest : PatchBaseEvent197
    {
        public PatchPattonsQuest()
        {
            priority = 228245;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = cursor.DefineLabel();
            cursor.GotoNext(i => i.MatchLdsfld(out _), i => i.MatchLdfld(AccessTools.Field(typeof(MainManager), "option")));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchPattonsQuest), "CheckQuestEvent"));
            cursor.Emit(OpCodes.Brfalse, label);

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(EventControl_Ext), "PattonsQuestEvent"));
            Utils.InsertYieldReturn(cursor);
            cursor.Emit(OpCodes.Ldc_I4_0);
            cursor.Emit(OpCodes.Ret);
            cursor.MarkLabel(label);
        }

        static bool CheckQuestEvent()
        {
            return MainManager.instance.option == 2 && MainManager.instance.flags[933] && !MainManager.instance.flags[934];
        }
    }

    /// <summary>
    /// Insert a for loop that adds hp pots for each the hp boost stat value
    /// </summary>
    public class PatchHPPotsAmount : PatchBaseEvent197
    {
        public PatchHPPotsAmount()
        {
            priority = 228608;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;
            cursor.GotoNext(MoveType.After,
                i => i.MatchLdcI4(135), 
                i=>i.MatchCallvirt(out _), 
                i=>i.MatchBr(out label), 
                i=>i.MatchLdsfld(out _)
            );

            int cursorIndex = cursor.Index;
            FieldReference statsRef = null;
            cursor.GotoPrev(i => i.MatchLdarg0(), i => i.MatchLdfld(out statsRef));
            cursor.Goto(cursorIndex);

            cursor.Prev.OpCode = OpCodes.Nop;
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, statsRef);
            cursor.Emit(OpCodes.Ldloc, 4);
            cursor.Emit(OpCodes.Ldelem_Ref);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchHPPotsAmount), "CheckHPPotAmount"));
            cursor.Emit(OpCodes.Br, label);

            cursor.Emit(OpCodes.Ldsfld, AccessTools.Field(typeof(MainManager), "instance"));
        }

        static void CheckHPPotAmount(int[] hpBoost)
        {
            for(int i=0; i< hpBoost[1]; i++)
            {
                MainManager.instance.items[1].Add((int)MainManager.Items.HPPotion);
            }
        }
    }
}
