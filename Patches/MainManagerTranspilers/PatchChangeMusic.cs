using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.MainManagerTranspilers
{
    /// <summary>
    /// Add a call to our own music check function to correctly parse new songs ids
    /// </summary>
    public class PatchChangeMusic : PatchBaseMainManagerChangeMusic
    {
        public PatchChangeMusic()
        {
            priority = 34;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdsfld(out _), i => i.MatchLdarg2());
            cursor.RemoveRange(2);
            cursor.GotoNext(MoveType.After, i => i.MatchCallvirt(out _));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "CheckMusicId"));
            cursor.RemoveRange(2);
        }
    }

    public class PatchMusicParse : PatchBaseMainManagerCheckSamira
    {
        public PatchMusicParse()
        {
            priority = 22;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdtoken(out _));
            cursor.RemoveRange(2);
            cursor.GotoNext(MoveType.After, i => i.MatchCallvirt(out _));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(MainManager_Ext), "CheckMusicId"));
            cursor.RemoveRange(2);
        }
    }
}
