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
    /// Old ant in war room quest event
    /// </summary>
    public class PatchOldAntEventPositions : PatchBaseEvent222
    {
        public PatchOldAntEventPositions()
        {
            priority = 259377;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            //camera x
            cursor.GotoNext(i => i.MatchLdcR4(9f));
            cursor.Emit(OpCodes.Ldc_R4, 11f);
            cursor.Remove();

            //camera z
            cursor.GotoNext(i => i.MatchLdcR4(5.15f));
            cursor.Emit(OpCodes.Ldc_R4, 4.15f);
            cursor.Remove();

            //lieutenant forcemove x
            cursor.GotoNext(i => i.MatchLdcR4(12.65f));
            cursor.Emit(OpCodes.Ldc_R4, 14f);
            cursor.Remove();

            //lieutenant forcemove z
            cursor.GotoNext(i => i.MatchLdcR4(2.85f));
            cursor.Emit(OpCodes.Ldc_R4, 1.15f);
            cursor.Remove();
        }
    }
}
