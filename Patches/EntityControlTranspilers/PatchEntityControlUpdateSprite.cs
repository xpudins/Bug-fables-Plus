using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Linq;
using UnityEngine;

namespace BFPlus.Patches.EntityControlTranspilers
{

    public class PatchNewEntityBackSprite : PatchBaseEntityControlUpdateSprite
    {
        static int[] animIds = { (int)NewAnimID.JumpAnt, (int)NewAnimID.Hoaxe, (int)NewAnimID.HoaxeCrown, (int)NewAnimID.BabyHoaxe };
        public PatchNewEntityBackSprite()
        {
            priority = 258;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;
            cursor.GotoNext(MoveType.After, i => i.MatchLdcI4(76), i => i.MatchBeq(out label));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(EntityControl), "originalid"));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchNewEntityBackSprite), "EntityHasBacksprite"));
            cursor.Emit(OpCodes.Brtrue, label);
        }

        static bool EntityHasBacksprite(int originalId)
        {
            return animIds.Contains(originalId);
        }
    }


    public class PatchMaterialAccess : PatchBaseEntityControlUpdateSprite
    {
        public PatchMaterialAccess()
        {
            priority = 416;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdfld(AccessTools.Field(typeof(EntityControl), "hologram")));
            cursor.GotoPrev(i => i.MatchBrfalse(out _));

            cursor.GotoPrev(MoveType.After, i => i.MatchLdarg0());
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchMaterialAccess), nameof(CheckRenderQueue)));
            cursor.Emit(OpCodes.Ret);
            cursor.Emit(OpCodes.Ldarg_0);
        }

        static void CheckRenderQueue(EntityControl entity)
        {
            if (entity.sprite == null)
                return;

            Material mat = entity.sprite.material;
            if (!(entity.sprite != null) || entity.hologram 
                || (!entity.battle && !(entity.npcdata == null) && entity.npcdata.entitytype == NPCControl.NPCType.NPC 
                && entity.npcdata.startlife >= 50f))
            {
                if (entity.hologram && entity.battle)
                {
                    mat.renderQueue = 2500;
                }
                return;
            }
            if (mat.color.a > 0.9f)
            {
                mat.renderQueue = 2450;
                return;
            }
            mat.renderQueue = 3000;
        }
    }

}
