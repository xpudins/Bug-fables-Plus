using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFPlus.Patches.EventControlTranspilers
{
    /// <summary>
    /// Wasp Prison giving item to an ant event
    /// </summary>
    public class PatchWaspPrisonItemsCheck : PatchBaseEvent215
    {
        public PatchWaspPrisonItemsCheck()
        {
            priority = 253968;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchStloc1());

            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Ldloc_2);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchWaspPrisonItemsCheck), "GetItemText"));
            cursor.Emit(OpCodes.Stloc_1);
        }

        static string GetItemText(string text, MainManager.Items item)
        {
            Console.WriteLine((int)item);
            if((int)item < Enum.GetNames(typeof(MainManager.Items)).Length)
            {
                return text;
            }

            return ((NewItem)item).ToString();
        }
    }
}
