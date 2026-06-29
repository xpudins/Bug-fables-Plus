using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace BFPlus.Patches.NPCControlTranspilers
{
    //This is for the seedling minigame, too many lost noise iirc
    public class PatchEnemySound : PatchBaseNPCControlRefreshPlayer
    {
        public PatchEnemySound()
        {
            priority = 57;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchCallvirt(AccessTools.Method(typeof(EntityControl), "PlaySound", new Type[] { typeof(string) })));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "CheckNPCEnemySound"));
            cursor.Remove();
        }
    }
}
