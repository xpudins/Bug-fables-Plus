using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.MainManagerTranspilers.ShowItemListPatches
{
    public class PatchMusicList : PatchBaseShowItemList
    {
        public PatchMusicList()
        {
            priority = 74234;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;
            ILLabel jumpLabel = cursor.DefineLabel();
            cursor.GotoNext(i => i.MatchLdcI4(18), i => i.MatchBneUn(out label));
            cursor.GotoNext();

            cursor.Emit(OpCodes.Beq, jumpLabel);
            cursor.Remove();
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldc_I4, (int)NewListType.MusicPlayer);
            cursor.Emit(OpCodes.Bne_Un, label);
            cursor.MarkLabel(jumpLabel);
        }
    }
}
