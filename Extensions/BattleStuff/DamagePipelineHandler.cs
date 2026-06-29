using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static BattleControl;
using static MainManager;
using static BFPlus.Extensions.BattleControl_Ext;

namespace BFPlus.Extensions.BattleStuff
{
    public class DamagePipelineHandler
    {
        public static bool targetWeaknessHit;
        public static int finalDefWithoutReductions;
        public static int defDownFlipBug;

        public static int GetBonusATK(BattleData? attacker, BattleData target, AttackProperty? property, List<DamageOverride> overrides, out int defPierce)
        {
            if (property.HasValue &&
                property.Value == AttackProperty.NoExceptions)
            {
                defPierce = 0;
                return 0;
            }

            if (overrides == null)
                overrides = new List<DamageOverride>();

            int bonusATK = 0;

            if (attacker.HasValue &&
                attacker.Value.condition.Count > 0 &&
                (property == null || property.Value != AttackProperty.NoExceptions))
            {
                bonusATK += GetATKFromStatusEffects(attacker.Value, target, overrides);
            }

            defPierce = overrides.Count(o => (int)o == (int)NewDamageOverride.Pierce1);
            if (property.HasValue && (property.Value == AttackProperty.Pierce || property.Value == AttackProperty.Atleast1pierce) &&
                (attacker == null || attacker.Value.battleentity.CompareTag("Player") || attacker.Value.animid >= (int)NewEnemies.Caveling))
            {
                defPierce = int.MaxValue;
            }

            if (attacker.HasValue)
                bonusATK += GetATKFromAttackerBonuses(attacker.Value, target, property, ref defPierce);
            return bonusATK;
        }

        public static void BonusATKForDelProjs(ref DelayedProjectileData delProj, DelayedProjExtra extraData, bool targetIsPlayer)
        {
            int defPierce = 0;
            delProj.damage = GetAttackerBaseDMG(delProj.damage, delProj.calledby, delProj.property);
            
            BattleData? target = null;
            if (targetIsPlayer)
            {
                int targetID = battle.partypointer[delProj.position];
                target = MainManager.instance.playerdata[targetID];
            }
            else if (delProj.position >= 0 && delProj.position < battle.enemydata.Length)
            {
                target = battle.enemydata[delProj.position];
            }

            if (target.HasValue)
            {
                if (delProj.calledby.condition.Count > 0
                && (delProj.property == null || delProj.property.Value != AttackProperty.NoExceptions))
                {
                    delProj.damage += GetATKFromStatusEffects(delProj.calledby, target.Value, null);
                }
                delProj.damage += GetATKFromAttackerBonuses(delProj.calledby, target.Value, delProj.property, ref defPierce);
            }

            if (defPierce > 0)
            {
                List<DamageOverride> overrides = new List<DamageOverride>();
                while (defPierce > 0)
                {
                    defPierce--;
                    overrides.Add((DamageOverride)NewDamageOverride.Pierce1);
                }

                if (extraData != null) 
                    extraData.overrides.AddRange(overrides);
                else 
                    DelayedProjExtra.AddDelayedProjExtra(delProj.obj, null, null, overrides);
            }
        }

        public static int GetATKFromStatusEffects(BattleData attacker, BattleData target, List<DamageOverride> overrides)
        {
            int atk = 0;
            bool attackerIsPlayer = attacker.battleentity.CompareTag("Player");
            foreach (int[] status in attacker.condition)
            {
                if (status[1] == -1) continue;
                switch (status[0])
                {
                    case (int)BattleCondition.AttackUp:
                        atk++;
                        break;

                    case (int)NewCondition.Huge:
                        atk += 2;
                        break;

                    case (int)BattleCondition.AttackDown:
                        atk--;
                        break;

                    case (int)NewCondition.Tiny:
                        atk -= 2;
                        break;

                    case (int)BattleCondition.Poison:
                        if (attackerIsPlayer)
                            atk += BadgeHowManyEquipped((int)BadgeTypes.PoisonAttacker, attacker.trueid);

                        if (!Instance.Invulnerable(target, overrides) && attacker.weakness.Contains(AttackProperty.PoisonDamUp))
                            atk += 2;
                        break;

                    case (int)NewCondition.Dizzy:
                        if (attackerIsPlayer)
                            atk += BadgeHowManyEquipped((int)Medal.Whirliwig, attacker.trueid);
                        break;
                }
            }
            return atk;
        }
        public static int GetATKFromAttackerBonuses(BattleData attacker, BattleData target, AttackProperty? property, ref int defPierce)
        {
            int atk = 0;

            //Tanjy has a base +2 dmg
            if (target.battleentity.CompareTag("Player") && attacker.animid == (int)Enemies.TANGYBUG)
                atk += 2;

            bool useCharge = true;
            bool attackerIsPlayer = attacker.battleentity.CompareTag("Player");
            if (!attackerIsPlayer)
            {
                if (MainManager.instance.flags[614] && MainManager.instance.flags[88])
                    atk++;

                if (target.noexpatstart && !MainManager.instance.flags[162])
                    atk++;

                if (attacker.animid == (int)NewEnemies.JumpAnt && battle.HPPercent(attacker) <= 0.2f)
                    atk += 2;

                if (attacker.animid == (int)NewEnemies.LeafbugShaman || attacker.animid == (int)NewEnemies.MechaJaw)
                    useCharge = false;
            }
            else if (!battle.demomode && battle.chompyattack == null)
            {
                if (MainManager.instance.playerdata[battle.partypointer[0]].trueid == attacker.trueid)
                    atk++;

                if (attacker.hp <= 4)
                    atk += BadgeHowManyEquipped((int)BadgeTypes.AttackUp, attacker.trueid);

                atk += Instance.CheckTeamGleam();
                atk += Instance.CheckOddWarrior(battle, attacker);
                atk += Instance.CheckKineticEnergy(attacker);

                if (battle.currentchoice == Actions.Attack)
                    defPierce += BadgeHowManyEquipped((int)BadgeTypes.AntlionJaws, attacker.trueid);

                useCharge = CanUseCharge(attacker.battleentity.battleid);
            }

            if (useCharge)
                atk += attacker.charge;

            return atk;
        }

        public static int GetFinalDEF(BattleData target, AttackProperty? property, List<DamageOverride> overrides, out int noPrcDEF, out float dmgMult)
        {
            if (overrides == null)
                overrides = new List<DamageOverride>();

            int def = 0;
            noPrcDEF = 0;
            dmgMult = 1f;

            if (property.HasValue && property.Value == AttackProperty.NoExceptions)
            {
                if (HasCondition(BattleCondition.Numb, target) > -1 && !overrides.Contains(DamageOverride.IgnoreNumb))
                {
                    def++;
                    finalDefWithoutReductions++;
                }

                if (!overrides.Contains(DamageOverride.NoIceBreak))
                {
                    if (HasCondition(BattleCondition.Freeze, target) > -1)
                        noPrcDEF--;

                    if (!overrides.Contains((DamageOverride)NewDamageOverride.IgnorePaintball) && HasCondition((BattleCondition)NewCondition.Paintball, target) > -1)
                        noPrcDEF--;

                    if (!overrides.Contains((DamageOverride)NewDamageOverride.IgnoreSlugskin) && HasCondition((BattleCondition)NewCondition.Slugskin, target) > -1)
                    {
                        def++;
                        finalDefWithoutReductions++;
                    }
                }
                return def;
            }


            bool flipProperty =
                (property.HasValue && property.Value == AttackProperty.Flip) ||
                overrides.Contains((DamageOverride)NewDamageOverride.FlipNoPierce);

            if (HasCondition(BattleCondition.Flipped, target) == -1)
            {
                int naturalDef = target.def;

                if (battle.HardMode())
                    naturalDef += target.harddef;

                if (target.isdefending && !flipProperty)
                    naturalDef += target.defenseonhit;

                def += naturalDef;
                finalDefWithoutReductions += naturalDef;
            }

            if (target.condition.Count > 0)
                def += GetDefFromStatusEffects(target, property, overrides, ref noPrcDEF, ref dmgMult);

            def += GetDefFromTargetBonuses(target);

            if (def < 0 && !target.battleentity.CompareTag("Player"))
                def = 0;

            if (Instance.Invulnerable(target, overrides))
            {
                noPrcDEF = int.MaxValue;
                return def;
            }

            return def;
        }
        public static int GetDefFromStatusEffects(BattleData target, AttackProperty? property, List<DamageOverride> overrides, ref int noPrcDEF, ref float DMGMult)
        {
            if (overrides == null)
                overrides = new List<DamageOverride>();

            int positiveDef = 0;
            int negativeDef = 0;
            bool targetIsPlayer = target.battleentity.CompareTag("Player");
            bool flipPierce =
                (property.HasValue && property.Value == AttackProperty.Flip)
                || (overrides.Contains((DamageOverride)NewDamageOverride.FlipNoPierce) && overrides.Contains((DamageOverride)NewDamageOverride.Pierce1));
            foreach (int[] status in target.condition)
            {
                if (status[1] == -1) 
                    continue;

                switch (status[0])
                {
                    case (int)BattleCondition.DefenseUp:
                        positiveDef++;
                        break;

                    case (int)NewCondition.Slugskin:
                        if (!overrides.Contains((DamageOverride)NewDamageOverride.IgnoreSlugskin))
                            positiveDef++;
                        break;

                    case (int)NewCondition.Huge:
                        positiveDef += 2;
                        break;

                    case (int)BattleCondition.Reflection:
                        positiveDef += status[1];
                        break;

                    case (int)BattleCondition.Sturdy:
                        if (targetIsPlayer && BadgeIsEquipped((int)BadgeTypes.Tardigrade, target.trueid))
                            positiveDef += 2;
                        break;

                    case (int)BattleCondition.DefenseDown:
                        negativeDef++;
                        if (flipPierce && defDownFlipBug < 1)
                            defDownFlipBug++;
                        break;

                    case (int)NewCondition.Tiny:
                        negativeDef += 2;
                        if (flipPierce && defDownFlipBug < 1)
                            defDownFlipBug++;
                        break;

                    case (int)BattleCondition.Poison:
                        if (targetIsPlayer)
                        {
                            positiveDef += BadgeHowManyEquipped((int)BadgeTypes.PoisonDefender, target.trueid);
                            negativeDef += BadgeHowManyEquipped((int)BadgeTypes.ReversePoison, target.trueid);
                        }
                        break;

                    case (int)BattleCondition.Sleep:
                        if (!overrides.Contains(DamageOverride.IgnoreNumb)) // yes, heavy sleeper checks for this too
                        {
                            if (targetIsPlayer)
                                DMGMult /= BadgeHowManyEquipped((int)BadgeTypes.HeavySleeper, target.trueid) + 1f;
                        }
                        break;

                    case (int)BattleCondition.Numb:
                        if (!overrides.Contains(DamageOverride.IgnoreNumb))
                        {
                            positiveDef++;
                            if (targetIsPlayer)
                                DMGMult /= BadgeHowManyEquipped((int)BadgeTypes.ShockTrooper, target.trueid) + 1f;
                        }
                        break;

                    case (int)BattleCondition.Freeze:
                        if (!overrides.Contains(DamageOverride.NoIceBreak))
                            noPrcDEF -= 1;
                        break;

                    case (int)NewCondition.Paintball:
                        if (!overrides.Contains((DamageOverride)NewDamageOverride.IgnorePaintball))
                            noPrcDEF -= 1;
                        break;

                    case (int)BattleCondition.Fire:
                        noPrcDEF -= BadgeHowManyEquipped((int)Medal.IgnitedMite);
                        break;
                }
            }
            finalDefWithoutReductions += positiveDef;
            return positiveDef - negativeDef;
        }
        public static int GetDefFromTargetBonuses(BattleData target)
        {
            int positiveDef = 0;
            bool targetIsPlayer = target.battleentity.CompareTag("Player");
            if (targetIsPlayer)
            {
                // front support
                if (IsPlayerInPos(0, target.battleentity.transform))
                    positiveDef += BadgeHowManyEquipped((int)BadgeTypes.FrontSupport);

                // back support
                if (IsPlayerInPos(MainManager.instance.playerdata.Length - 1, target.battleentity.transform))
                    positiveDef += BadgeHowManyEquipped((int)BadgeTypes.BackSupport);

                // last stand
                if (target.hp <= 4)
                    positiveDef += BadgeHowManyEquipped((int)BadgeTypes.DefenseUp, target.trueid);

                // charge guard - with and without recharge
                if (target.charge > 0 && BadgeIsEquipped((int)Medal.ChargeGuard, target.trueid))
                    positiveDef += target.charge;
            }
            else
            {
                // Leafbug Shaman's charge guard
                if (target.animid == (int)NewEnemies.LeafbugShaman && target.charge > 0)
                    positiveDef += target.charge;

                // Jump Ant's last stands
                if (target.animid == (int)NewEnemies.JumpAnt && battle.HPPercent(target) <= 0.2f)
                    positiveDef += 2;

                // Caveling's Being In The Ground
                if (target.animid == (int)NewEnemies.Caveling && target.position == BattlePosition.Underground)
                    positiveDef++;
            }
            finalDefWithoutReductions += positiveDef;
            return positiveDef;
        }

        public static int GetDmgFromTargetWeakness(BattleData? attacker, BattleData target, AttackProperty? property, List<DamageOverride> overrides)
        {
            if (overrides == null)
                overrides = new List<DamageOverride>();

            if (property == null)
                property = AttackProperty.None;

            int bonusDMG = 0;
            bool attackerIsPlayer = attacker.HasValue && attacker.Value.battleentity.CompareTag("Player");

            if (HasWeakness(AttackProperty.Magic, target))
            {
                if (property.Value == AttackProperty.Magic || overrides.Contains((DamageOverride)NewDamageOverride.Magic) || (
                        attackerIsPlayer && battle.currentchoice == Actions.Skill && (
                            battle.lastskill == (int)MainManager.Skills.Icefall ||
                            battle.lastskill == (int)MainManager.Skills.FrigidCoffin ||
                            battle.lastskill == (int)MainManager.Skills.IceRain)))
                {
                    bonusDMG++;
                    if(battle.commandsuccess)
                        targetWeaknessHit = true;
                }
            }

            if (HasWeakness(AttackProperty.HornExtraDamage, target))
            {
                if (property.Value == AttackProperty.Flip || overrides.Contains((DamageOverride)NewDamageOverride.FlipNoPierce) || 
                    (attackerIsPlayer && battle.commandsuccess && battle.currentchoice == Actions.Skill 
                                      && battle.lastskill == (int)MainManager.Skills.BeeFly))
                {
                    bonusDMG++;
                    targetWeaknessHit = true;
                }
            }

            if (HasWeakness((AttackProperty)NewProperty.Beemerang, target))
            {
                if (property.Value == (AttackProperty)NewProperty.Beemerang || overrides.Contains((DamageOverride)NewDamageOverride.Beemerang) || (
                        attackerIsPlayer && battle.commandsuccess && ((
                            battle.currentchoice == Actions.Attack && attacker.Value.trueid == 0) || (
                            battle.currentchoice == Actions.Skill && (
                                battle.lastskill == (int)MainManager.Skills.BeeRangMultiHit ||
                                battle.lastskill == (int)MainManager.Skills.HurricaneBeemerang ||
                                battle.lastskill == (int)MainManager.Skills.HeavyThrow)))))
                {
                    bonusDMG++;
                    targetWeaknessHit = true;
                }
            }

            if (battle.HasWeakness(target, AttackProperty.DefDownOnFlyHard) &&
                target.position == BattlePosition.Flying && battle.HardMode())
            {
                bonusDMG++;
            }
            return bonusDMG;
        }

        public static int GetAttackerBaseDMG(int baseDMG, BattleData attacker, AttackProperty? property)
        {
            if (property == null || property.Value != AttackProperty.NoExceptions)
                baseDMG -= attacker.tired;

            if (!attacker.battleentity.CompareTag("Player"))
            {
                if (battle.HardMode())
                    baseDMG += attacker.hardatk;

                if (BadgeIsEquipped((int)BadgeTypes.DoublePainReal)) // hard hits dmg clamp applies BEFORE atk bonuses
                    baseDMG = GetHardHitsClampDMG(baseDMG);
            }
            return baseDMG;
        }

        public static void GetExtraOverrides(ref List<DamageOverride> overrides, AttackProperty property)
        {
            if (property == AttackProperty.Flip)
            {
                overrides.Add((DamageOverride)NewDamageOverride.Pierce1);
            }
        }

        public static int GetFinalDMG(int baseDMG, BattleData? attacker, ref BattleData target, AttackProperty? property, ref DamageOverride[] overridesArray)
        {
            bool attackerIsPlayer = attacker.HasValue && attacker.Value.battleentity.CompareTag("Player");
            List<DamageOverride> overrides = overridesArray?.ToList() ?? new List<DamageOverride>();

            if (property.HasValue) 
                GetExtraOverrides(ref overrides, property.Value);


            int finalDEF = GetFinalDEF(target, property, overrides, out int noPrcDEF, out float DMGMult);

            int bonusATK = GetBonusATK(attacker, target, property, overrides, out int defPierce) + GetDmgFromTargetWeakness(attacker, target, property, overrides);

            int pierceBug = Mathf.Min(defDownFlipBug, finalDefWithoutReductions);
            if (finalDEF + pierceBug > 0 && defPierce > 0)
                finalDEF -= Mathf.Min(finalDEF + pierceBug, defPierce);
            defDownFlipBug = finalDefWithoutReductions = 0;

            finalDEF += noPrcDEF;

            if (attacker.HasValue)
                baseDMG = GetAttackerBaseDMG(baseDMG, attacker.Value, property);

            baseDMG += bonusATK;
            baseDMG = Mathf.FloorToInt(baseDMG * DMGMult);

            if (HasWeakness(AttackProperty.Flip, target) && HasCondition(BattleCondition.Flipped, target) == -1 &&
                (!HasWeakness(AttackProperty.ToppleFirst, target) || HasCondition(BattleCondition.Topple, target) > -1))
                baseDMG = Mathf.Clamp(baseDMG, 1, 99); // guarantees at least 1 DMG when flipping a foe, before DEF is taken into account

            Instance.realDamage = baseDMG;

            if (attacker.HasValue && !attackerIsPlayer) // hard hits dmg bonus applies AFTER all other atk boosts
            {
                if (BadgeIsEquipped((int)BadgeTypes.DoublePainReal))
                    baseDMG = GetHardHitsDMG(baseDMG);
            }
            baseDMG -= finalDEF;

            if (finalDEF < 0)
                Instance.realDamage -= finalDEF;

            if (target.charge > 0)
            {
                bool targetIsPlayer = target.battleentity.CompareTag("Player");
                bool chargeGuard = targetIsPlayer ?
                    BadgeIsEquipped((int)Medal.ChargeGuard, target.trueid) :
                    target.animid == (int)NewEnemies.LeafbugShaman;

                if (chargeGuard)
                {
                    Vector3 iconPos = target.battleentity.transform.position + Vector3.up;
                    battle.StartCoroutine(battle.ItemSpinAnim(iconPos, itemsprites[1, (int)Medal.ChargeGuard], true));
                    target.charge = 0;
                }
            }

            overridesArray = overrides.ToArray();

            return baseDMG;
        }

        static int GetHardHitsDMG(int baseValue)
        {
            Instance.realDamage = Mathf.FloorToInt((float)Instance.realDamage * 1.25f) + 1;
            return Mathf.FloorToInt((float)baseValue * 1.25f) + 1;
        }
        static int GetHardHitsClampDMG(int baseValue)
        {
            Instance.realDamage = Mathf.Clamp(Instance.realDamage, 2, 99);
            return Mathf.Clamp(baseValue, 2, 99);
        }


        public static bool FlipTarget(ref BattleData target)
        {
            bool actuallyFlipped = false;
            if (HasWeakness(AttackProperty.Flip, target) && HasCondition(BattleCondition.Flipped, target) == -1)
            {
                actuallyFlipped = true;
                if (!HasWeakness(AttackProperty.ToppleFirst, target))
                {
                    target.condition.Add(new int[2] { (int)BattleCondition.Flipped, 1 });
                }
                else if (HasCondition(BattleCondition.Topple, target) > -1)
                {
                    target.condition.Add(new int[2] { (int)BattleCondition.Flipped, 1 });
                    RemoveCondition(BattleCondition.Topple, target);
                }
                else
                {
                    if (target.position == BattlePosition.Ground)
                    {
                        target.battleentity.basestate = 21;
                    }
                    target.condition.Add(new int[2] { (int)BattleCondition.Topple, 1 });
                }
            }
            if (!target.battleentity.CompareTag("Player"))
            {
                if (target.holditem != -1)
                {
                    actuallyFlipped = true;
                    battle.DropItem(ref target, additem: true);
                }
                if (target.isdefending)
                {
                    actuallyFlipped = true;
                    target.isdefending = false;
                }
                if (target.animid == (int)NewEnemies.MechaJaw && MainManager.HasCondition(BattleCondition.Flipped, target) != -1)
                {
                    target.battleentity.GetComponent<MechaJawComp>().CheckFlippedRes(ref target);
                }
            }
            return actuallyFlipped;
        }
    }
}