using BFPlus.Extensions;
using BFPlus.Extensions.MiscStuff;
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

namespace BFPlus.Patches.MiscsTranspilers
{
    public class PatchGlowTriggerLateUpdate : PatchBaseGlowTriggerLateUpdate
    {
        public PatchGlowTriggerLateUpdate()
        {
            priority = 0;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchStloc2(), i=>i.MatchLdarg0());

            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchGlowTriggerLateUpdate), nameof(GetMaterial)));
            Utils.RemoveUntilInst(cursor, i => i.MatchLdloc2());

            cursor.GotoNext(MoveType.After, i => i.MatchLdloc0());

            ILLabel label = null;
            int index = cursor.Index;
            cursor.GotoNext(i => i.MatchBr(out label));
            cursor.Goto(index);

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchGlowTriggerLateUpdate), nameof(NewMaterialColor)));
            cursor.Emit(OpCodes.Br, label);
            cursor.Emit(OpCodes.Ldloc_0);
        }

        static Material GetMaterial(GlowTrigger instance, int index)
        {
            return MaterialCache.Get(instance.glowparts[index]).mats[instance.materialid];
        }

        static void NewMaterialColor(bool flag, GlowTrigger instance, int index)
        {
            Material mat = GetMaterial(instance,index);
            if (flag)
            {
                if (instance.getactivecolorfromstart)
                {
                    mat.color = Color.Lerp(mat.color, instance.tcolor[0], MainManager.TieFramerate(instance.glowspeed));
                    mat.SetColor(MaterialCache.emission, Color.Lerp(mat.GetColor(MaterialCache.emission), instance.tcolor[1], MainManager.TieFramerate(instance.glowspeed)));
                }
                else
                {
                    mat.color = Color.Lerp(mat.color, instance.activecolor, MainManager.TieFramerate(instance.glowspeed));
                    mat.SetColor(MaterialCache.emission, mat.color);
                }
            }
            else
            {
                instance.sound?.Stop();
                mat.color = Color.Lerp(mat.color, instance.deactivatedcolor, MainManager.TieFramerate(instance.glowspeed));
                mat.SetColor(MaterialCache.emission, Color.Lerp(mat.GetColor(MaterialCache.emission), Color.black, MainManager.TieFramerate(instance.glowspeed)));
            }
        }
    }
}
