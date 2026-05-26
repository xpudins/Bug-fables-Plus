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
using UnityEngine;

namespace BFPlus.Patches.EntityControlTranspilers
{
    public class PatchTrailSpriteSize : PatchBaseEntityControlRefreshTrail
    {
        public PatchTrailSpriteSize()
        {
            priority = 159;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdcR4(20f), i=>i.MatchStelemR4());
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchTrailSpriteSize), nameof(ChangeTrailSpriteSize)));
        }

        static void ChangeTrailSpriteSize(EntityControl entity)
        {
            entity.traildata.trails[entity.traildata.id].transform.localScale = entity.startscale;
        }
    }
}
