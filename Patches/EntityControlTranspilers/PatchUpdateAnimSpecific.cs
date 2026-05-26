using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace BFPlus.Patches.EntityControlTranspilers
{
    /// <summary>
    /// Call a method after instantiating the beerang in animstate 13, to check if its darkVi, if it is, change the mat color to black
    /// </summary>
    public class PatchDarkViBeerang : PatchBaseEntityControlUpdateAnimSpecific
    {
        public PatchDarkViBeerang()
        {
            priority = 254;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdstr("Prefabs/AnimSpecific/BeeBIdle"));
            cursor.GotoNext(MoveType.After, i => i.MatchStfld(out _));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchDarkViBeerang), "ChangeDarkViBeerangColor"));
        }

        static void ChangeDarkViBeerangColor(EntityControl entity)
        {
            if (MainManager_Ext.IsNewEnemy(entity, NewEnemies.DarkVi))
            {
                entity.animspecific[0].GetComponent<ParticleSystemRenderer>().material.color = Color.black;
            }
        }
    }
}
