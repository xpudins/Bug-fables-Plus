using BFPlus.Extensions;
using BFPlus.Extensions.BattleStuff.StatusStuff;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MainManager;
using static BattleControl;

namespace BFPlus.Patches.BattleControlTranspilers.DoActionPatches
{
    public class PatchCheckDizzyKO : PatchBaseDoAction
    {
        public PatchCheckDizzyKO()
        {
            priority = 149991;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After,
                i => i.MatchLdcI4(1),
                i => i.MatchStfld(AccessTools.Field(typeof(BattleControl), "killinput"))
            );

            var entityRef = cursor.Instrs[cursor.Index + 1].Operand;

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, entityRef);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchCheckDizzyKO), nameof(CheckDizzyKO)));
        }

        static void CheckDizzyKO(EntityControl entity)
        {
            if (Dizzy.dizzyKO)
            {
                Entity_Ext extEnt;
                for (int i = 0; i < instance.playerdata.Length; i++)
                {
                    extEnt = Entity_Ext.GetEntity_Ext(instance.playerdata[i].battleentity);
                    if (extEnt?.diedFromDizzy ?? true)
                    {
                        if (extEnt != null)
                            extEnt.dizzyRecoil = 0;
                        continue;
                    }

                    if (instance.playerdata[i].hp > 0)
                    {
                        extEnt.diedFromDizzy = null;
                        extEnt.dizzyRecoil = 0;
                    }
                }

                bool enemyDied = false;
                for (int i = 0; i < battle.enemydata.Length; i++)
                {
                    extEnt = Entity_Ext.GetEntity_Ext(battle.enemydata[i].battleentity);
                    if (extEnt?.diedFromDizzy ?? true)
                    {
                        if (extEnt != null)
                            extEnt.dizzyRecoil = 0;
                        continue;
                    }

                    if (battle.enemydata[i].hp <= 0)
                    {
                        enemyDied = true;
                    }
                    else
                    {
                        extEnt.diedFromDizzy = null;
                        extEnt.dizzyRecoil = 0;
                    }
                }

                if (enemyDied && battle.enemy)
                {
                    battle.selfsacrifice = true;
                    if (battle.enemydata[entity.battleid].hitaction)
                        battle.enemy = false;
                }
            }
            Dizzy.dizzyKO = false;
        }
    }
}