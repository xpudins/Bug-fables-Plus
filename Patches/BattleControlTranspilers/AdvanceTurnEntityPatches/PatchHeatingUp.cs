using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace BFPlus.Patches.BattleControlTranspilers.AdvanceTurnEntityPatches
{
    public class PatchHeatingUp : PatchBaseAdvanceTurnEntity
    {
        public PatchHeatingUp()
        {
            priority = 220;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdstr("Flame"));

            // This is for jester fire healing him instead of damaging
            ILLabel label = null;
            cursor.GotoNext(i => i.MatchBrtrue(out label));
            cursor.GotoPrev(i => i.MatchLdarg0());

            cursor.Emit(OpCodes.Ldarg_1);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchHeatingUp), "CheckFireDamage"));
            cursor.Emit(OpCodes.Brfalse, label);

            //this is for heating up and supporting fire
            cursor.GotoNext(i => i.MatchLdfld(out _));
            cursor.Emit(OpCodes.Ldobj, typeof(MainManager.BattleData));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "DoHeatingUp"));
            Utils.RemoveUntilInst(cursor, i => i.MatchLdcI4(13));

            //add status damage overide to overrides
            cursor.GotoNext(i => i.MatchLdcI4(0));
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "GetStatusDamageOverrides"));
        }

        static bool CheckFireDamage(ref MainManager.BattleData target)
        {
            if (target.animid == (int)NewEnemies.Jester)
            {
                MainManager.battle.Heal(ref target, BattleControl_Ext.DoHeatingUp(target));
                return false;
            }

            if (target.animid == (int)NewEnemies.Moeruki)
            {
                int newCharge = BattleControl_Ext.DoHeatingUp(target);
                BattleControl_Ext.Instance.ChargeUp(ref target, newCharge, 0.25f);
                return false;
            }

            return true;
        }
    }
}
