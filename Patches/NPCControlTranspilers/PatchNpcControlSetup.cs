using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine;

namespace BFPlus.Patches.NPCControlTranspilers
{
    public class PatchNpcControlSetup : PatchBaseNPCControlSetup
    {
        public PatchNpcControlSetup()
        {
            priority = 1765;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdarg0(), i => i.MatchLdfld(out _), i => i.MatchLdfld(out _), i => i.MatchLdstr("Audio/Music/"));

            ILLabel label = cursor.DefineLabel();
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchNpcControlSetup), "CheckMusicRangeMusicId"));
            cursor.Emit(OpCodes.Brtrue, label);

            cursor.GotoNext(i => i.MatchLdarg0(), i => i.MatchLdfld(out _), i => i.MatchLdfld(out _), i => i.MatchLdcR4(0.0f));
            cursor.MarkLabel(label);
        }

        static bool CheckMusicRangeMusicId(NPCControl npc)
        {
            if (Enum.IsDefined(typeof(NewMusic), npc.data[2]))
            {
                NewMusic music = (NewMusic)npc.data[2];
                npc.entity.sound.clip = MainManager_Ext.assetBundle.LoadAsset<AudioClip>(music.ToString());
                return true;
            }
            return false;
        }
    }
}
