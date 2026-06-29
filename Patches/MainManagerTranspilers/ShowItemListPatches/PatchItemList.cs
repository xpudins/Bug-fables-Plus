using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace BFPlus.Patches.MainManagerTranspilers.ShowItemListPatches
{
    public class PatchItemList : PatchBaseShowItemList
    {
        public PatchItemList()
        {
            priority = 71909;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchBgt(out _), i => i.MatchLdstr("itemsprite"));
            cursor.GotoNext(i => i.MatchLdcR4(-2.5f));

            cursor.GotoPrev(MoveType.After, i => i.MatchLdloc(out _), i => i.MatchCallvirt(out _), i => i.MatchLdloc(out _));

            var barRef = cursor.Prev.Operand;

            cursor.GotoPrev(MoveType.After, i => i.MatchLdsfld(out _), i => i.MatchLdloc(out _));
            var indexRef = cursor.Prev.Operand;

            cursor.GotoNext(MoveType.After, i => i.MatchCallvirt(out _), i => i.MatchCallvirt(out _));

            cursor.Emit(OpCodes.Ldloc, barRef);
            cursor.Emit(OpCodes.Ldloc, indexRef);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchItemList), "CheckMiniMushSprite"));
        }

        static void CheckMiniMushSprite(SpriteRenderer bar, int index)
        {
            if (MainManager.instance.inbattle && index >= 0 && index < MainManager.listvar.Length)
            {
                int mushType = BattleControl_Ext.Instance.GetMushroomTinyHuge(MainManager.listvar[index]);
                if (MainManager.BadgeIsEquipped((int)Medal.MiniMegaMush) && mushType != -1)
                {
                    mushType = mushType == (int)NewCondition.Huge ? (int)NewGui.HugeMush : (int)NewGui.TinyMush;
                    MainManager.NewUIObject("miniMush", bar.transform, new Vector3(2.55f, 0f), new Vector3(0.7f, 0.75f, 1f), MainManager.guisprites[mushType], 10).GetComponent<SpriteRenderer>();
                }
            }
        }
    }
}
