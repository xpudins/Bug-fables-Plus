using BFPlus.Extensions;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static MainManager;
using static BattleControl;
using BFPlus.Extensions.EnemyAI;
using BFPlus.Extensions.BattleStuff;
using BFPlus.Extensions.BattleStuff.StatusStuff;

namespace BFPlus.Patches
{
    [HarmonyPatch(typeof(BattleControl), "EndEnemyTurn")]
    public class PatchBattleControlEndEnemyTurn
    {
        static void Prefix(BattleControl __instance, int id)
        {
            BattleControl_Ext.enemyUsedItem = false;

            //If we are HUGE, action cost one more turn
            if (!__instance.enemydata[id].hitaction)
            {
                BattleControl_Ext.Instance.CheckHugeAction(ref __instance.enemydata[id]);
            }
        }
    }

    [HarmonyPatch(typeof(BattleControl), "ClearStatus")]
    public class PatchBattleControlClearStatus
    {
        static int conditionAmount = 0;
        static void Prefix(BattleControl __instance, ref MainManager.BattleData target)
        {
            conditionAmount = BattleControl_Ext.Instance.CalculateCleanseDamage(target);
        }

        static void Postfix(BattleControl __instance, ref MainManager.BattleData target)
        {
            int amountCleared = conditionAmount - target.condition.Count;

            if (BattleControl_Ext.actionID == (int)MainManager.Skills.Cleanse)
            {
                BattleControl_Ext.Instance.DealCleanseDamage(__instance, ref target);
            }

            var entityExt = Entity_Ext.GetEntity_Ext(target.battleentity);
            if (MainManager.HasCondition(MainManager.BattleCondition.Inked, target) == -1 && entityExt.inkDebuffed)
            {
                entityExt.CheckInkDebuff(ref target);
            }

            entityExt.slugskinActive = false;
            entityExt.vitiation = false;
            entityExt.healedThisTurn = 0;
            entityExt.inkBubbleEnabled = false;
            entityExt.isDizzy = false;
            entityExt.ResetDizzyAngle();

            if (target.hp <= 0)
            {
                if (entityExt.scaleChanged)
                    BattleControl_Ext.Instance.ResetTinyHugeEffect(target.battleentity, entityExt);
                entityExt.tinyMovesAdded = false;
            }

            if (target.battleentity.playerentity && conditionAmount > target.condition.Count)
            {
                if (amountCleared > 0 && target.hp > 0)
                {
                    BattleControl_Ext.Instance.DoPurifyingPulseCheck(ref target, amountCleared);
                    BattleControl_Ext.Instance.DoRevitalizingRippleCheck(ref target, amountCleared);
                }
            }

        }
    }

    [HarmonyPatch(typeof(BattleControl), "SetItem")]
    public class PatchBattleControlSetItem
    {
        static bool Prefix(BattleControl __instance, int id)
        {
            if ((id == 50 || id == 51 || id == 52) && (MainManager.listtype < 0 && __instance.currentaction == BattleControl.Pick.SkillList && MainManager.BadgeIsEquipped((int)Medal.HoloSkill, MainManager.instance.playerdata[__instance.currentturn].trueid)))
            {
                __instance.StartCoroutine(BattleControl_Ext.Instance.GetSkillList(id));
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(BattleControl), "HPBarOnOther")]
    public class PatchBattleControlHPBarOnOther
    {
        static void Postfix(int thisid, ref bool __result)
        {
            switch (thisid)
            {
                case (int)NewEnemies.MarsSprout:
                    __result = MainManager.instance.librarystuff[1, (int)NewEnemies.Mars];
                    break;
                case (int)NewEnemies.RedSeedling:
                case (int)NewEnemies.BlueSeedling:
                    __result = true;
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(BattleControl), "RevivePlayer", new Type[] { typeof(int), typeof(int), typeof(bool) })]
    public class PatchBattleControlRevivePlayer
    {
        static bool Prefix(int id, int hp, bool showcounter)
        {
            BattleControl_Ext.Instance.InVengeance = false;
            var ext = Entity_Ext.GetEntity_Ext(instance.playerdata[id].battleentity); 
            ext.diedFromDizzy = null;
            ext.ResetCarousel();
            return true;
        }
    }

    [HarmonyPatch(typeof(BattleControl), "EndPlayerTurn")]
    public class PatchBattleControlEndPlayerTurn
    {
        static bool Prefix(BattleControl __instance)
        {
            __instance.StartCoroutine(BattleControl_Ext.Instance.ResetHoloID(__instance));
            int currentTurn = __instance.currentturn;

            for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
            {
                if (MainManager.instance.playerdata[i].hp > 0)
                {
                    BattleControl_Ext.Instance.CheckHDWGHConditionAmount(MainManager.instance.playerdata[i], MainManager.instance.playerdata[i].battleentity.GetComponent<Entity_Ext>());
                }
            }

            if (BattleControl_Ext.Instance.gourmetItemUse > 0)
            {
                bool isStopped = __instance.IsStoppedLite(MainManager.instance.playerdata[currentTurn]);
                int aliveEnemies = __instance.AliveEnemies();
                if (MainManager.instance.items[0].Count > 0 && !isStopped && aliveEnemies > 0 && !__instance.inevent)
                {
                    if (__instance.action)
                    {
                        __instance.StartCoroutine(BattleControl_Ext.Instance.WaitForActionGourmet(currentTurn));
                    }
                    else
                    {
                        BattleControl_Ext.Instance.gourmetItemUse--;
                        BattleControl_Ext.Instance.GoToItemList();
                    }
                    return false;
                }
            }
            BattleControl_Ext.Instance.gourmetItemUse = -1;
            BattleControl_Ext.Instance.stylishCountThisAction = 0;

            //If we are HUGE, every action cost one more turn
            BattleControl_Ext.Instance.CheckHugeAction(ref MainManager.instance.playerdata[currentTurn]);

            return true;
        }
    }


    [HarmonyPatch(typeof(BattleControl), "PlayerTurn")]
    public class PatchBattleControlPlayerTurn
    {
        static bool Prefix(BattleControl __instance)
        {
            if (MainManager.instance.flags[(int)NewCode.EVEN] && (__instance.turns + 1) % 2 != 0)
            {
                for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                    MainManager.instance.playerdata[i].cantmove = 1;

                __instance.chompyattacked = true;
                __instance.aiattacked = true;
                __instance.enemy = true;
                __instance.currentturn = -1;
                __instance.UpdateAnim();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(BattleControl), "GetChoiceInput")]
    public class PatchBattleControlGetChoiceInput
    {
        static void Prefix(BattleControl __instance)
        {
            if (__instance.currentaction == BattleControl.Pick.BaseAction && BattleControl_Ext.Instance.holoSkillID != -1)
            {
                BattleControl_Ext.Instance.holoSkillID = -1;
            }
        }
    }

    [HarmonyPatch(typeof(BattleControl), "Retry")]
    public class PatchBattleControlRetry
    {
        static void Prefix(BattleControl __instance)
        {
            BattleControl_Ext.stylishBarAmount = BattleControl_Ext.startStylishAmount;
            BattleControl_Ext.stylishReward = BattleControl_Ext.startStylishReward;
        }
    }

    [HarmonyPatch(typeof(BattleControl), "StartData")]
    public class PatchBattleControlStartData
    {
        static void Prefix(BattleControl __instance)
        {
            BattleControl_Ext.startStylishAmount = BattleControl_Ext.stylishBarAmount;
            BattleControl_Ext.startStylishReward = BattleControl_Ext.stylishReward;

            BattleControl_Ext.Instance.ResetStuff();
        }


        //attempt at fixing some music fuckery
        static void Postfix(BattleControl __instance, string music)
        {
            __instance.sdata.music = music;
        }
    }

    [HarmonyPatch(typeof(BattleControl), "AddDelayedCondition")]
    public class PatchBattleControlAddDelayedCondition
    {
        static bool Prefix(BattleControl __instance, int enid)
        {
            if (MainManager.HasCondition(MainManager.BattleCondition.Sturdy, MainManager.battle.enemydata[enid]) > -1)
            {
                return false;
            }
            return true;
        }
    }

    //randomize commands but data field isnt right for each command
    [HarmonyPatch(typeof(BattleControl), "DoCommand")]
    public class PatchBattleControlDoCommand
    {
        static void FillRandomCommandData(float[] data, float[][] maxData, int[] dataType)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (i >= maxData[0].Length)
                    break;

                if (dataType[i] == 0)
                {
                    data[i] = UnityEngine.Random.Range((int)maxData[0][i], (int)maxData[1][i]);
                }
                else
                {
                    data[i] = UnityEngine.Random.Range(maxData[0][i], maxData[1][i]);
                }
            }
        }
        static float[] GetRandomCommandData(object commandType, ref float timer)
        {
            float[] data = new float[1];
            float[][] minMaxData;
            switch (commandType)
            {
                case 4:
                case 10:
                    data = new float[] { 0, 0, 0, 1f };
                    minMaxData = new float[][]
                    {
                        new float[] { 4, 6, 0.1f },
                        new float[] { 7, 8, 1.25f }
                    };

                    FillRandomCommandData(data, minMaxData, new int[] { 0, 1, 1 });

                    if (UnityEngine.Random.Range(0, 2) == 0 && (int)commandType != 10)
                        data[0] = -1;

                    timer = UnityEngine.Random.Range(160f, 220f);
                    break;

                case 5:
                    data = new float[] { 0, 1f };
                    minMaxData = new float[][]
                    {
                        new float[] { 1, 0 },
                        new float[] { 10, 7 }
                    };
                    FillRandomCommandData(data, minMaxData, new int[] { 0, 0 });
                    timer = UnityEngine.Random.Range(20f, 30f);
                    break;

                case 9:
                    data = new float[] { 0, 1f, 1, 2f };
                    minMaxData = new float[][]
                    {
                        new float[] { 25, 45,25 },
                        new float[]{ 35,70,35}
                    };
                    FillRandomCommandData(data, minMaxData, new int[] { 1, 1, 1 });
                    timer = UnityEngine.Random.Range(70f, 120f);
                    break;

                case 11:
                    data = new float[] { 0, 1f };
                    minMaxData = new float[][]
                    {
                        new float[] { 0, 1 },
                        new float[] { 3, 6 }
                    };
                    FillRandomCommandData(data, minMaxData, new int[] { 0, 0 });
                    timer = UnityEngine.Random.Range(25f, 50f);
                    break;

                case 13:
                    data = new float[] { 0, 1f };
                    minMaxData = new float[][]
                    {
                        new float[] { 1,0},
                        new float[] { 5, 2.0625f }
                    };
                    FillRandomCommandData(data, minMaxData, new int[] { 0, 1 });
                    timer = 0.01f;
                    break;

                case 14:
                    data = new float[UnityEngine.Random.Range(1, 3)];
                    minMaxData = new float[][]
                    {
                        new float[] { 2,0},
                        new float[] { 8, 0 }
                    };
                    FillRandomCommandData(data, minMaxData, new int[] { 0, 0 });
                    timer = UnityEngine.Random.Range(120f, 300f);
                    break;

                case 15:
                    data = new float[] { 0, 1f, 0, 0 };
                    minMaxData = new float[][]
                    {
                        new float[] { 1,0,1,0},
                        new float[] { 5, 0,3.25f,10 }
                    };
                    FillRandomCommandData(data, minMaxData, new int[] { 0, 0, 1, 0 });
                    timer = UnityEngine.Random.Range(60f, 120f);
                    break;

                case 16:
                    data = new float[] { 0 };
                    minMaxData = new float[][]
                    {
                        new float[] { 0},
                        new float[] { 7 }
                    };
                    FillRandomCommandData(data, minMaxData, new int[] { 0 });
                    timer = UnityEngine.Random.Range(60f, 120f);
                    break;
            }
            return data;
        }

        static void Prefix(BattleControl __instance, ref object commandtype, ref float[] data, ref float timer)
        {
            if (__instance.enemy)
            {
                BattleControl_Ext.mashSuperblockThreshold = timer * 0.3f;
                BattleControl_Ext.mashSuperblockTimer = timer;
            }

            if (MainManager.instance.flags[(int)NewCode.COMMAND])
            {
                int commandsAmount = Enum.GetNames(typeof(BattleControl.ActionCommands)).Length;
                List<int> commands = Enumerable.Range(0, commandsAmount).ToList();
                List<int> unusedCommands = new List<int> { 0, 1, 2, 3, 6, 7, 8, 12 };
                commands.RemoveAll(c => unusedCommands.Contains(c));

                commandtype = commands[UnityEngine.Random.Range(0, commands.Count)];
                data = GetRandomCommandData(commandtype, ref timer);
            }
        }
    }

    [HarmonyPatch(typeof(BattleControl), "MultiSkillMove")]
    public class PatchBattleControlMultiSkillMove
    {
        static void Prefix(BattleControl __instance, int[] ids)
        {
            BattleControl_Ext.Instance.attackedThisTurn.AddRange(ids);
        }
    }

    [HarmonyPatch(typeof(BattleControl), "GetMultiDamage")]
    public class PatchBattleControlGetMultiDamage
    {
        static void Postfix(BattleControl __instance, int[] ids, ref int __result)
        {
            foreach (int id in ids)
            {
                var attacker = MainManager.GetPlayerData(id);
                __result += BattleControl_Ext.Instance.CheckKineticEnergy(attacker);
                __result += BattleControl_Ext.Instance.CheckTeamGleam();
                __result += BattleControl_Ext.Instance.CheckOddWarrior(__instance, attacker);
                __result += BattleControl_Ext.Instance.GetTinyHugeStat(attacker);
            }
        }
    }

    [HarmonyPatch(typeof(BattleControl), "DefaultDamageCalc")]
    public class PatchBattleControlDefaultDamageCalc
    {
        static bool Prefix(BattleControl __instance, BattleData target, ref int basevalue, bool pierce, bool blocked, int def)
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(BattleControl), "AddDelayedProjectile")]
    public class PatchBattleControlAddDelayedProjectile
    {
        static void Postfix(BattleControl __instance)
        {
            // lets delayed attacks benefit from attacker-based dmg bonuses
            if (BattleControl_Ext.Instance.enemyDelProjUsesAtkBonuses)
                DamagePipelineHandler.BonusATKForDelProjs(ref __instance.delprojs[__instance.delprojs.Length - 1], null, true);
        }
    }


    [HarmonyPatch(typeof(BattleControl), "AdvanceTurnEntity")]
    public class PatchBattleControlAdvanceTurnEntity
    {
        public static bool wasDizzy;
        static void Prefix(BattleControl __instance, ref MainManager.BattleData t)
        {
            wasDizzy = HasCondition((BattleCondition)NewCondition.Dizzy, t) > -1;
        }
        static void Postfix(BattleControl __instance, ref MainManager.BattleData t)
        {
            if (!wasDizzy)
                wasDizzy = HasCondition((BattleCondition)NewCondition.Dizzy, t) > -1;

            Sleep.CheckSweetDreams(t);
            var entityExt = Entity_Ext.GetEntity_Ext(t.battleentity);

            if (t.cantmove != 0 && t.moves > 1)
            {
                t.cantmove -= t.moves - 1;
            }

            if (MainManager.HasCondition(MainManager.BattleCondition.Taunted, t) == -1)
            {
                entityExt.tauntedBy = -1;
            }

            if (MainManager.HasCondition(MainManager.BattleCondition.Inked, t) == -1 && entityExt.inkDebuffed)
            {
                if (MainManager.BadgeIsEquipped((int)Medal.PermanentInk) && !entityExt.permanentInkTriggered)
                {
                    entityExt.permanentInkTriggered = true;
                }
                else
                {
                    entityExt.CheckInkDebuff(ref t);
                    entityExt.permanentInkTriggered = false;
                }
            }

            Dizzy.DoAfterEffect(ref t, entityExt, wasDizzy);
        }
    }

    [HarmonyPatch(typeof(BattleControl), "StatEffect")]
    public class PatchBattleControlStatEffect
    {
        static void Prefix(BattleControl __instance, EntityControl target, int type)
        {
            if (type == 4 && target.CompareTag("Player") && MainManager.BadgeIsEquipped((int)Medal.BugBattery, MainManager.instance.playerdata[target.battleid].trueid) && MainManager.HasCondition(MainManager.BattleCondition.Sturdy, MainManager.instance.playerdata[target.battleid]) == -1)
            {
                MainManager.PlaySound("Numb");
                MainManager.SetCondition(MainManager.BattleCondition.Numb, ref MainManager.instance.playerdata[target.battleid], 2);
                MainManager.instance.playerdata[target.battleid].isnumb = true;

                //fix if you roll charge on random start and get numbed
                if (MainManager.battle.chompyattack == null && !BattleControl_Ext.Instance.inAiAttack && !MainManager.battle.enemy && MainManager.battle.currentturn == -1)
                {
                    MainManager.battle.SetLastTurns();
                }
            }

        }
    }

    [HarmonyPatch(typeof(BattleControl), "GetRandomAvaliablePlayer", new Type[] { })]
    public class PatchBattleControlGetRandomAvaliablePlayer
    {
        static bool Prefix(BattleControl __instance, ref int __result)
        {
            return BattleControl_Ext.Instance.GetTauntedBy(ref __result, __instance);
        }
    }

    [HarmonyPatch(typeof(BattleControl), "GetRandomAvaliablePlayer", new Type[] { typeof(bool) })]
    public class PatchBattleControlGetRandomAvaliablePlayerBool
    {
        static bool Prefix(BattleControl __instance, ref int __result)
        {
            return BattleControl_Ext.Instance.GetTauntedBy(ref __result, __instance);
        }
    }

    [HarmonyPatch(typeof(BattleControl), "CameraFocusTarget", new Type[] { })]
    public class PatchBattleControlCameraFocusTarget
    {
        static bool Prefix(BattleControl __instance)
        {
            return __instance.playertargetID >= 0;
        }
    }


    [HarmonyPatch]
    public class PatchBattleControlDoDamage
    {
        static MethodBase TargetMethod()
        {
            IEnumerable methods = typeof(BattleControl).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(method => method.Name == "DoDamage").Cast<MethodBase>();
            return typeof(BattleControl).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(method => method.Name == "DoDamage" && method.GetParameters().Length == 6).FirstOrDefault();
        }
        static bool superBlocked = false;

        static bool Prefix(BattleControl __instance, MethodBase __originalMethod, BattleData? attacker, ref BattleData target, ref int damageammount, BattleControl.AttackProperty? property, ref BattleControl.DamageOverride[] overrides, bool block)
        {
            bool isDotDamage = BattleControl_Ext.Instance.IsDotDamage(overrides);
            bool targetIsPlayer = target.battleentity.CompareTag("Player");

            if (targetIsPlayer && !isDotDamage)
            {
                superBlocked = (__instance.GetSuperBlock(target.battleentity.animid) || __instance.superblockedthisframe > 0f) && !__instance.IsStopped(target);
            }

            int twinedfateBug = BattleControl_Ext.GetEquippedMedalBug(Medal.TwinedFate, (i) => MainManager.instance.playerdata[i].hp > 0 && MainManager.instance.playerdata[i].eatenby == null);
            if (targetIsPlayer && !isDotDamage && twinedfateBug != -1 && !BattleControl_Ext.Instance.twinedFateUsed && target.trueid != twinedfateBug && target.hp <= 4)
            {
                BattleControl_Ext.Instance.twinedFateUsed = true;
                __instance.DoDamage(attacker, ref MainManager.instance.playerdata[twinedfateBug], damageammount, property, overrides, block);
                return false;
            }

            var entityExt = Entity_Ext.GetEntity_Ext(target.battleentity);

            if (targetIsPlayer && MainManager.BadgeIsEquipped((int)Medal.Revengarang) && attacker != null && !isDotDamage && target.trueid == 0 && block && __instance.nonphyscal && !battle.IsStopped(target))
            {
                BattleControl_Ext.Instance.revengarangIsActive = true;
                BattleControl_Ext.Instance.revengarangDMG = 1 + MainManager.BadgeHowManyEquipped((int)Medal.Revengarang);
                if (superBlocked)
                {
                    BattleControl_Ext.Instance.revengarangDMG += MainManager.BadgeHowManyEquipped((int)MainManager.BadgeTypes.SuperBlock, 0);
                }

                if (entityExt.slugskinActive)
                    BattleControl_Ext.Instance.revengarangDMG++;

            }

            BattleControl_Ext.Instance.realDamage = 0;

            entityExt.beforeDoDamageHp = target.hp;

            if (__instance.chompyattack == null && MainManager.BadgeIsEquipped((int)Medal.Blightfury) && !BattleControl_Ext.Instance.inAiAttack && !isDotDamage)
            {
                if (attacker == null)
                {
                    if (BattleControl_Ext.Instance.entityAttacking != null && BattleControl_Ext.Instance.entityAttacking.CompareTag("Player") && BattleControl_Ext.Instance.entityAttacking.animid == 2 && BattleControl_Ext.Instance.leifSkillIds.Contains(BattleControl_Ext.actionID))
                    {
                        BattleControl_Ext.Instance.DoPoison(ref target);
                    }
                }
                else
                {
                    if (attacker.Value.battleentity.CompareTag("Player") && attacker.Value.battleentity.animid == 2 && BattleControl_Ext.Instance.leifSkillIds.Contains(BattleControl_Ext.actionID))
                    {
                        BattleControl_Ext.Instance.DoPoison(ref target);
                    }
                }
            }

            if (!isDotDamage &&
                !BattleControl_Ext.Instance.firstHitMulti &&
                MainManager.BadgeIsEquipped((int)Medal.IgnitedMite) &&
                MainManager.HasCondition(MainManager.BattleCondition.Fire, target) > -1)
            {
                if (__instance.chompyattack == null &&
                    (BattleControl_Ext.actionID == (int)Skills.BeeRangMultiHit || BattleControl_Ext.actionID == (int)Skills.HurricaneBeemerang) &&
                    BattleControl_Ext.Instance.entityAttacking != null &&
                    BattleControl_Ext.Instance.entityAttacking.CompareTag("Player") &&
                    BattleControl_Ext.Instance.entityAttacking.animid == 0)
                {
                    BattleControl_Ext.Instance.firstHitMulti = true;
                }
            }

            if (!__instance.enemy && !isDotDamage && __instance.chompyattack == null && BattleControl_Ext.Instance.entityAttacking != null)
            {
                BattleControl_Ext.Instance.attackedThisTurn.Add(BattleControl_Ext.Instance.entityAttacking.battleid);
            }

            bool attackerIsInked = attacker != null && !isDotDamage && MainManager.HasCondition(MainManager.BattleCondition.Inked, attacker.Value) > -1;
            //Smearcharge check
            if (targetIsPlayer && attackerIsInked && MainManager.BadgeIsEquipped((int)Medal.Smearcharge, target.trueid))
            {
                entityExt.smearchargeActive = true;
            }
            return true;
        }

        static void Postfix(BattleControl __instance, MethodBase __originalMethod, BattleData? attacker, ref BattleData target, ref int damageammount, AttackProperty? property, DamageOverride[] overrides, bool block, int __result)
        {
            bool isDotDamage = BattleControl_Ext.Instance.IsDotDamage(overrides);
            bool targetIsPlayer = target.battleentity.CompareTag("Player");

            if (!isDotDamage)
                Dizzy.CheckRecoilDamage(attacker, BattleControl_Ext.Instance.realDamage);

            if (!isDotDamage && targetIsPlayer && target.hp - __result > 0 && MainManager.BadgeIsEquipped((int)Medal.FlashFreeze, target.trueid) && target.hp > 4 && MainManager.HasCondition(MainManager.BattleCondition.Sturdy, target) == -1)
            {
                MainManager.RemoveCondition(MainManager.BattleCondition.Freeze, target);
                MainManager.SetCondition(MainManager.BattleCondition.Freeze, ref target, 3);
                target.battleentity.inice = true;
                target.weakness = new List<BattleControl.AttackProperty>(new BattleControl.AttackProperty[] { BattleControl.AttackProperty.HornExtraDamage });
                if(target.battleentity.icecube == null)
                    target.battleentity.Freeze();
            }
            var entityExt = Entity_Ext.GetEntity_Ext(target.battleentity);
            if (!targetIsPlayer && BadgeIsEquipped((int)Medal.Perkfectionist) && entityExt.beforeDoDamageHp - __result == 0 && __result != 0 && entityExt.beforeDoDamageHp != 0 && target.hp == 0)
            {
                BattleControl_Ext.Instance.perfectKill = true;
                BattleControl_Ext.Instance.perfectKillAmount++;
            }

            if (targetIsPlayer && attacker != null && !isDotDamage && target.trueid == 2)
            {
                BattleControl_Ext.Instance.CheckMothflower(superBlocked, block, target);
            }

            if (targetIsPlayer && target.hp > 0)
            {
                if (__result > 0)
                    BattleControl_Ext.Instance.PotentialEnergyCheck(ref target);
            }

            if (!isDotDamage)
            {
                for (int i = 0; i < target.condition.Count; i++)
                {
                    // Nerfs Bubble shield by limiting how many attacks it can block
                    // Duration lowers by 1 per hit taken; shield is removed when duration runs out
                    if (target.condition[i][0] == (int)BattleCondition.Shield)
                    {
                        CheckBubbleShieldDamage(ref target, i);
                        continue;
                    }

                    if (target.condition[i][0] == (int)NewCondition.Vitiation &&
                        BattleControl_Ext.Instance.realDamage - __result > 0)
                    {
                        CheckVitiationDamage(ref target, i, ref attacker, __result, entityExt, targetIsPlayer);
                        continue;
                    }

                    if (!(overrides?.Contains((DamageOverride)NewDamageOverride.IgnorePaintball) ?? false) && target.condition[i][0] == (int)NewCondition.Paintball && entityExt.inkBubbleEnabled && entityExt.inkBubble != null && Vector3.Distance(entityExt.inkBubble.transform.localScale, entityExt.extraData.inkBubbleScale) < 0.01f)
                    {
                        CheckInkBubbleDamage(ref target, i, ref attacker, __result, overrides?.ToList(), targetIsPlayer);
                        continue;
                    }

                    if (!(overrides?.Contains((DamageOverride)NewDamageOverride.IgnoreSlugskin) ?? false) && target.condition[i][0] == (int)NewCondition.Slugskin)
                    {
                        CheckSlugskinDamage(ref target, i, ref attacker);
                        continue;
                    }
                }


                if (targetIsPlayer && target.hp > 0)
                {
                    if (MainManager.BadgeIsEquipped((int)Medal.Slugskin, target.trueid) & superBlocked && MainManager.HasCondition(MainManager.BattleCondition.Sticky, target) != -1)
                    {
                        MainManager.SetCondition((BattleCondition)NewCondition.Slugskin, ref target, 1);
                        MainManager.PlaySound("Shield", 1.4f, 1);
                    }
                }

                if (!targetIsPlayer)
                {
                    BattleControl_Ext.Instance.CheckStrikeBlasters(__instance, target, entityExt.beforeDoDamageHp, entityExt);
                }

                if (!__instance.enemy && !targetIsPlayer && __result > BattleControl_Ext.Instance.trustFallDamage)
                {
                    if (__instance.turns == BattleControl_Ext.Instance.trustFallTurn + 1 && BattleControl_Ext.Instance.trustFallTurn != -1)
                        BattleControl_Ext.Instance.trustFallDamage = __result;
                }

                if (__instance.chompyattack == null)
                {
                    BattleControl_Ext.Instance.DoInkWellCheck(__result, ref target, targetIsPlayer);
                    BattleControl_Ext.Instance.DoWebsheetCheck(attacker, ref target, targetIsPlayer);
                }

                bool attackerIsInked = attacker != null && !isDotDamage && MainManager.HasCondition(MainManager.BattleCondition.Inked, attacker.Value) > -1;
                if (attackerIsInked && MainManager.BadgeIsEquipped((int)Medal.Inkblot))
                {
                    var attackerExt = Entity_Ext.GetEntity_Ext(attacker.Value.battleentity);

                    if (!attackerExt.inkblotActive)
                    {
                        Vector3 particlePos = target.battleentity.transform.position + Vector3.up + target.battleentity.height * Vector3.up;
                        BattleControl_Ext.Instance.ApplyStatus(BattleCondition.Inked, ref target, 2, "WaterSplash2", 0.8f, 1, "InkGet", particlePos, Vector3.one);
                        attackerExt.inkblotActive = true;
                    }
                }
            }

            if (targetIsPlayer && __result > 0 && MainManager.BadgeIsEquipped((int)Medal.NoPainNoGain))
            {
                BattleControl_Ext.Instance.RecoverPlayerTP(1, target);
            }

            if (!isDotDamage && target.animid == (int)NewEnemies.FireAnt && !__instance.IsStopped(target))
            {
                FireAntAI.FireAntHustle(ref target);
            }

            if(!__instance.enemy && !isDotDamage && __instance.chompyattack == null 
                && BattleControl_Ext.Instance.entityAttacking != null 
                && BattleControl_Ext.Instance.entityAttacking.CompareTag("Player") && __instance.currentturn != -1)
            {
                if(BadgeIsEquipped((int)Medal.Carousel, instance.playerdata[__instance.currentturn].trueid))
                {
                    var ext = Entity_Ext.GetEntity_Ext(BattleControl_Ext.Instance.entityAttacking);
                    ext?.ResetCarousel();
                }
            }
        }

        static void CheckBubbleShieldDamage(ref BattleData target, int c)
        {
            int zommothId = battle.EnemyInField((int)Enemies.Zommoth);
            if (lastevent == 182 &&
                zommothId != -1 &&
                battle.enemydata[zommothId].data != null &
                battle.enemydata[zommothId].data.Length >= 0 &&
                battle.enemydata[zommothId].data[0] == 0)
            {
                return;
            }
            target.condition[c][1]--;
            if (target.condition[c][1] < 1)
            {
                RemoveCondition(BattleCondition.Shield, target);
                target.battleentity.shieldenabled = false;
            }
        }
        static void CheckVitiationDamage(ref BattleData target, int conditionIndex, ref BattleData? attacker,
            int vitiationDamage, Entity_Ext extEnt, bool targetIsPlayer)
        {
            vitiationDamage = Mathf.Min(BattleControl_Ext.Instance.realDamage - vitiationDamage, target.condition[conditionIndex][1]);

            target.condition[conditionIndex][1] -= vitiationDamage;
            if (target.condition[conditionIndex][1] < 1)
                RemoveCondition((BattleCondition)target.condition[conditionIndex][0], target);

            if (extEnt.slugskinActive)
                vitiationDamage++;

            if ((attacker.HasValue || BattleControl_Ext.Instance.entityAttacking) && !BattleControl_Ext.Instance.inAiAttack && !battle.chompyaction)
            {
                if (targetIsPlayer)
                {
                    int id = -1;

                    if(BattleControl_Ext.Instance.entityAttacking != null)
                        id = BattleControl_Ext.Instance.entityAttacking.battleid;

                    if (attacker.HasValue)
                        id = attacker.Value.battleentity.battleid;

                    if(id != -1)
                        BattleControl_Ext.Instance.DoFakeDamage(id, vitiationDamage);
                }
                else
                {
                    BattleControl_Ext.Instance.DoFakeDamage(ref instance.playerdata[battle.currentturn], vitiationDamage);
                }
            }

            BattleControl_Ext.Instance.realDamage = 0;
        }
        static void CheckInkBubbleDamage(ref BattleData target, int conditionIndex, ref BattleData? attacker, int incomingDMG, List<DamageOverride> overrides, bool targetIsPlayer)
        {
            target.condition[conditionIndex][1]--;
            if (target.condition[conditionIndex][1] < 1)
            {
                PlaySound("BubbleBurst", 0.8f, 1);
                RemoveCondition((BattleCondition)target.condition[conditionIndex][0], target);
            }

            if (incomingDMG > 0)
            {
                int tpRecover = Mathf.CeilToInt(incomingDMG * 0.5f);
                if (attacker.HasValue)
                {
                    if (attacker.Value.battleentity.CompareTag("Player"))
                        battle.HealTP(tpRecover);
                    else
                        BattleControl_Ext.Instance.RecoverEnemyTp(tpRecover, attacker.Value.battleentity.battleid);
                }
                else if (overrides != null && overrides.Contains((DamageOverride)NewDamageOverride.DelayedDamage))
                {
                    if (BattleControl_Ext.Instance.inPlayerDelayedProjs > -1)
                        battle.HealTP(tpRecover);
                    else
                        BattleControl_Ext.Instance.RecoverEnemyTp(tpRecover, battle.delprojs[battle.delprojs.Length - 1].calledby.battleentity.battleid);
                }
                else
                {
                    if (battle.chompyattack != null || BattleControl_Ext.Instance.inAiAttack || !targetIsPlayer)
                        battle.HealTP(tpRecover);
                    else if (battle.enemydata.Length > 0)
                        BattleControl_Ext.Instance.RecoverEnemyTp(tpRecover, 0);
                }
            }
        }
        static void CheckSlugskinDamage(ref BattleData target, int conditionIndex, ref BattleData? attacker)
        {
            target.condition[conditionIndex][1]--;
            if (target.condition[conditionIndex][1] <= 0)
            {
                RemoveCondition((BattleCondition)NewCondition.Slugskin, target);
            }

            if (attacker != null)
            {
                int recoilDamage = 2;
                if (attacker.Value.battleentity.CompareTag("Player"))
                    BattleControl_Ext.Instance.DoFakeDamage(ref instance.playerdata[attacker.Value.trueid], recoilDamage);
                else
                    BattleControl_Ext.Instance.DoFakeDamage(ref battle.enemydata[attacker.Value.battleentity.battleid], recoilDamage);
            }
        }
    }

    [HarmonyPatch(typeof(BattleControl), "CalculateBaseDamage")]
    public class PatchBattleControlCalculateBaseDamage
    {
        static void Prefix(BattleControl __instance, BattleData? attacker, ref BattleData target, ref int basevalue, bool block, AttackProperty? property, ref bool weaknesshit, DamageOverride[] overrides)
        {
            if (!__instance.doingaction &&
                BattleControl_Ext.mashSuperblockThreshold > 0)
            {
                if (__instance.barfill >= 1f &&
                    BattleControl_Ext.mashSuperblockTimer > BattleControl_Ext.mashSuperblockThreshold)
                {
                    __instance.superblockedthisframe = 3f;
                }
                BattleControl_Ext.mashSuperblockThreshold = BattleControl_Ext.mashSuperblockTimer = -1;
            }

            if (target.battleentity.CompareTag("Player"))
            {
                if (HasCondition(BattleCondition.Sticky, target) > -1 && (block || __instance.superblockedthisframe > 0 || battle.GetSuperBlock(target.battleentity.animid)))
                {
                    basevalue -= BadgeHowManyEquipped((int)Medal.ThickSilk, target.trueid);
                }
            }

            BattleControl_Ext.Instance.realDamage = basevalue;
            if (property == null || property.Value != AttackProperty.Raw)
            {
                basevalue = DamagePipelineHandler.GetFinalDMG(basevalue, attacker, ref target, property, ref overrides);
                weaknesshit = DamagePipelineHandler.targetWeaknessHit;
                DamagePipelineHandler.targetWeaknessHit = false;

                int flips = overrides.Count(o => o == (DamageOverride)NewDamageOverride.FlipNoPierce);
                if (property.HasValue && property.Value == AttackProperty.Flip)
                    flips++;

                // add multiple flips to one attack to make wasp bombers miserable!!!
                while (flips > 0)
                {
                    flips--;
                    if(DamagePipelineHandler.FlipTarget(ref target));
                        weaknesshit = true;
                }
            }
        }


        static void Postfix(BattleControl __instance, BattleData? attacker, ref BattleData target, ref bool superguarded, ref int __result)
        {
            if (BattleControl_Ext.Instance.inEndOfTurnDamage)
            {
                target.cantmove = 1;
            }

            if (target.battleentity.CompareTag("Player"))
            {
                BattleControl_Ext.Instance.DoLoomLegsCheck(ref target, superguarded);
                if (HasCondition(BattleCondition.Sticky, target) > -1)
                {
                    BattleControl_Ext.Instance.DoHoneyWebCheck(ref target, superguarded);
                }
            }
        }
    }

    [HarmonyPatch(typeof(BattleControl), "TryCondition")]
    public class PatchBattleControlTryCondition
    {
        static bool Prefix(BattleControl __instance, ref BattleData data, BattleCondition condition)
        {
            if (BattleControl_Ext.Instance.IsStatusImmune(data, condition))
            {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(BattleControl), "ReturnToMainSelect")]
    public class PatchBattleControlReturnToMainSelect
    {
        static bool Prefix(BattleControl __instance)
        {
            if (BattleControl_Ext.Instance.gourmetItemUse >= 0)
            {
                if (BattleControl_Ext.Instance.gourmetItemUse == 1)
                {
                    MainManager.instance.tp = Mathf.Clamp(MainManager.instance.tp + MainManager_Ext.GetDoubleDipCost(), 0, MainManager.instance.maxtp);
                }
                else
                {
                    __instance.StartCoroutine(WaitForDestroyListGourmet());
                    return false;
                }
            }

            BattleControl_Ext.Instance.gourmetItemUse = -1;
            return true;
        }

        static IEnumerator WaitForDestroyListGourmet()
        {
            yield return new WaitUntil(() => BattleControl_Ext.Instance.destroyedList);
            MainManager.PlaySound("Cancel", 10);
            BattleControl_Ext.Instance.GoToItemList();
        }
    }

    [HarmonyPatch(typeof(BattleControl), "AddNewEnemy", new Type[] { typeof(int), typeof(EntityControl) })]
    public class PatchBattleControlAddNewEnemy
    {
        public static void CheckHoloEnemy(Transform enemy)
        {
            if (MainManager.instance.flags[162] && enemy != null)
            {
                enemy.GetComponent<EntityControl>().hologram = MainManager_Ext.IsHolo();
            }
        }

        static void Postfix(BattleControl __instance, ref Transform __result)
        {
            CheckHoloEnemy(__result);
        }
    }

    [HarmonyPatch(typeof(BattleControl), "AddNewEnemy", new Type[] { typeof(int), typeof(Vector3) })]
    public class PatchBattleControlAddNewEnemy2
    {
        static void Postfix(BattleControl __instance, ref Transform __result)
        {
            PatchBattleControlAddNewEnemy.CheckHoloEnemy(__result);
        }
    }

    [HarmonyPatch(typeof(BattleControl), "UndergroundCheck")]
    public class PatchBattleControlUndergroundCheck
    {
        static void Postfix(BattleControl __instance, BattleControl.BattlePosition pos, int attackid, ref bool __result)
        {
            if (pos == BattleControl.BattlePosition.Underground)
            {
                if (attackid == (int)NewSkill.Steal)
                    __result = true;

                if (attackid == (int)Skills.BeeFly)
                    __result = true;

                if (attackid == (int)Skills.HurricaneBeemerang && MainManager_Ext.Instance.balanceChanges[(int)NewMenuText.HurricaneToss])
                    __result = false;

                if (__instance.currentaction == Pick.ItemList)
                    __result = BattleControl_Ext.CanItemHitUnderground(__instance.selecteditem);
            }
        }
    }

    [HarmonyPatch(typeof(BattleControl), "TrueDef")]
    public class PatchBattleControlTrueDef
    {
        static void Postfix(BattleControl __instance, MainManager.BattleData data, ref int __result)
        {
            __result = DamagePipelineHandler.GetFinalDEF(data, null, null, out _, out _);
        }
    }

    /// <summary>
    /// Jump ant revive on start of next turn
    /// </summary>
    [HarmonyPatch(typeof(BattleControl), "CheckEvent")]
    public class PatchBattleControlCheckEvent
    {
        static void Prefix(BattleControl __instance)
        {
            if (!__instance.inevent && !__instance.action && __instance.enemydata.Length != 0)
            {
                if (__instance.enemy && __instance.reservedata.Count > 0)
                {
                    int? index = __instance.reservedata.Select((e, idx) => e.animid == (int)NewEnemies.JumpAnt ? (int?)idx : null).FirstOrDefault(i => i != null);

                    if (index != null && __instance.reservedata[index.Value].turnssincedeath >= 1)
                    {
                        __instance.calleventnext = (int)NewEventDialogue.JumpAntRevive;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(BattleControl), "SeedlingTackle")]
    public class PatchBattleControlSeedlingTackle
    {
        static void Prefix(BattleControl __instance, ref int damage, int attackerid)
        {
            if (__instance.enemydata[attackerid].animid == (int)NewEnemies.RedSeedling || __instance.enemydata[attackerid].animid == (int)NewEnemies.BlueSeedling)
                damage = 2;
        }
    }

    [HarmonyPatch(typeof(BattleControl), "CleanKill")]
    public class PatchBattleControlCleanKill
    {
        static void Prefix(BattleControl __instance, ref MainManager.BattleData target, Vector3 startp)
        {
            BattleControl_Ext.Instance.cleanKilledEnemyPos.Add(startp);
        }
    }

    [HarmonyPatch(typeof(BattleControl), "CreateHelpBox", new Type[] { })]
    public class PatchBattleControlCreateHelpBox
    {
        static void Postfix(BattleControl __instance)
        {
            if (__instance.helpbox != null)
            {
                __instance.helpbox.targetscale = Vector3.one * 0.8f;
                __instance.helpbox.transform.localPosition = new Vector3(-4.2f, -4.45f, 10f);
            }
        }
    }

    [HarmonyPatch(typeof(BattleControl), "AddAI")]
    public class PatchBattleControlAddAI
    {
        static bool Prefix(BattleControl __instance, int id, int basestate)
        {
            return __instance.aiparty == null;
        }
    }

    /// <summary>
    /// Skip pincer status, we have our own checks for needle medals in the hooks.
    /// </summary>
    [HarmonyPatch(typeof(BattleControl), "PincerStatus")]
    public class PatchBattleControlPincerStatus
    {
        static bool Prefix(BattleControl __instance)
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(BattleControl), "CheckItemUse")]
    public class PatchBattleControlCheckItemUse
    {
        static bool Prefix(BattleControl __instance, int id, ref bool __result)
        {
            if(__instance.currentaction == Pick.SelectPlayer && __instance.currentchoice == Actions.Item 
                && id == (int)NewItem.Spiroll && (Entity_Ext.GetEntity_Ext(instance.playerdata[__instance.option].battleentity).cantSwap 
                || Entity_Ext.GetEntity_Ext(instance.playerdata[__instance.currentturn].battleentity).cantSwap) && __instance.currentturn != __instance.option)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}



