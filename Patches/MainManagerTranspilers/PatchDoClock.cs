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

namespace BFPlus.Patches.MainManagerTranspilers
{
    public class PatchDoClock : PatchBaseDoClock
    {
        public PatchDoClock()
        {
            priority = 36;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchCall(out _));
            
            for(int i=0;i<3;i++)
                cursor.Instrs[cursor.Index+i].OpCode = OpCodes.Nop;
        }
    }
}
