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

namespace BFPlus.Patches.MainManagerTranspilers.ShowItemListPatches
{
    /// <summary>
    /// Add new maps to map list so we can tp to them more easily
    /// </summary>
    public class PatchMapListValues : PatchBaseShowItemList
    {
        public PatchMapListValues()
        {
            priority = 71108;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(31));
            cursor.GotoNext(MoveType.After, i => i.MatchConvI4());
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchMapListValues), nameof(GetTotalMapsCount)));
        }

        static int GetTotalMapsCount(int vanillaMapCount)
        {
            return vanillaMapCount + Enum.GetValues(typeof(NewMaps)).Length;
        }
    }

    public class PatchMapListNames : PatchBaseShowItemList
    {
        public PatchMapListNames()
        {
            priority = 71996;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdcI4(31));
            cursor.GotoNext(i => i.MatchLdtoken(out _));

            cursor.Emit(OpCodes.Ldloc, 15);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchMapListNames), nameof(GetMapName)));
            Utils.RemoveUntilInst(cursor, i => i.MatchStloc(out _));
        }

        static string GetMapName(int listIndex)
        {
            int mapId = MainManager.listvar[listIndex];
            if(Enum.IsDefined(typeof(MainManager.Maps), mapId))
                return ((MainManager.Maps)mapId).ToString();
            return ((NewMaps)mapId).ToString();
        }
    }
}
