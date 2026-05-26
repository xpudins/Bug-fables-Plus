using BFPlus.Extensions.BattleStuff;
using BFPlus.Extensions.BattleStuff.Skills;
using BFPlus.Extensions.BattleStuff.StatusStuff;
using BFPlus.Extensions.EnemyAI;
using BFPlus.Extensions.Stylish;
using HarmonyLib;
using InputIOManager;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using static BattleControl;
using static FlappyBee;
using static MainManager;

namespace BFPlus.Extensions
{
    public enum StylishReward
    {
        None,
        HPRegen,
        TPRegen,
        Berries,
        Buff,
        Debuff
    }

    public enum DelProjType
    {
        StickyBomb,
        InkTrap,
        NeedleToss
    }

    public enum NewEventDialogue
    {
        MarsDeath = 20,
        JesterSpitout,
        PattonDeath,
        StylishTutorial,
        JumpAntDeath,
        JumpAntRevive
    }

    public enum NewCondition
    {
        Tiny = 21,
        Huge,
        Dizzy,
        Paintball,
        Slugskin,
        Vitiation
    }

    public enum NewProperty
    {
        Tiny = 34,
        Huge,
        Dizzy,
        Beemerang, // :worm:
    }

    public enum NewDamageOverride
    {
        Pierce1 = 21, // add this multiple times to the same attack to pierce more!!!!
        Beemerang, // for attacks that already have other properties
        Magic, // for attacks that already have other properties
        FlipNoPierce,
        ExtraAirTopple,
        DelayedDamage,
        IgnorePaintball,
        IgnoreSlugskin,
        IgnoreInvulnerable,
        StatusDamage
    }

    public class BattleControl_Ext : MonoBehaviour
    {
        public bool InVengeance = false;
        public List<int> leifSkillIds = new List<int>() { -1, 4, 21, 25, 27, 26, 31, 17, 7, 8, 22, 14, 23, 15, 30, 28, 29, 12, 13, (int)NewSkill.VitiationLite, (int)NewSkill.Vitiation, (int)NewSkill.CordycepsLeech };
        public List<int> leifBuffSkillIds = new List<int>() { 17, 7, 47, 8, 22, 14, 23, 15, 30, 28, 29, 12, 13, 54, 55 };
        public EntityControl entityAttacking;
        public static int actionID;
        public bool destroyedList = false;
        public int damageDeepCleanse = 0;
        public int tpRegenCleanse = 0;
        public const int DAMAGE_DEEPCLEANSE = 3;
        public const int TP_REGEN_CLEANSE = 3;
        public bool firstHitMulti = false;
        public int holoSkillID = -1;
        public List<int> attackedThisTurn = new List<int>();
        public bool revengarangIsActive = false;
        public int revengarangDMG = 0;
        public bool perfectKill = false;
        public int perfectKillAmount = 0;
        public int loomLegProgress = 0;
        int oldAnimID = -1;
        public int startState = -1;
        public int rockyRampUpDmg = 0;
        bool usedPebbleToss = false;
        public bool twinedFateUsed = false;
        public bool spuderStickyBubble = false;
        public bool spinelingFlipped = false;
        public bool inEndOfTurnDamage = false;
        public int inPlayerDelayedProjs = -1;
        public bool enemyDelProjUsesAtkBonuses = true;
        List<StrikeBlaster> strikeBlasters = new List<StrikeBlaster>();
        public Coroutine strikeBlasterManager = null;
        public int trustFallTurn = -1;
        public int trustFallDamage = 0;
        public static bool enemyUsedItem = false;
        public bool inStylish = false;
        bool failedStylish = false;
        public static float startStylishAmount = 0;
        public static float stylishBarAmount = 0;
        public int stylishCountThisAction = 0;
        public static StylishReward startStylishReward = StylishReward.None;
        SpriteRenderer stylishBarHolder = null;
        SpriteRenderer stylishBar = null;
        public static StylishReward stylishReward = StylishReward.None;
        SpriteRenderer rewardIcon = null;

        public bool inAiAttack = false;
        const int BASE_CORYCEPSLEECH_DMG = 4;
        public int gourmetItemUse = -1;
        List<DelayedProjExtra> delProjsPlayer = new List<DelayedProjExtra>();
        static BattleControl_Ext instance = null;
        List<Entity_Ext> entity_Exts = new List<Entity_Ext>();
        public DelayedProjExtra currentDelayedProj = null;
        public int mothFlowerBlocks = 0;
        public int mothFlowerSuperBlocks = 0;
        public bool inStylishTutorial = false;
        const int vengeanceMax = 3;
        public JumpAntFight jumpAntFightComp = null;
        public List<Vector3> cleanKilledEnemyPos = new List<Vector3>();
        public StatusInfo statusInfo;
        public bool inStatusInfo = false;
        public static float mashSuperblockThreshold = -1;
        public static float mashSuperblockTimer = -1;
        public bool noHugeMoveReduction = false;
        int stylishRoutine = 0;
        public string battleMusic;
        public static bool inStartBattle = false;
        public CachedEnemy[] cachedEnemies = new CachedEnemy[4];
        public Vector3? startPosition; // entity start Pos when starting doAction
        public static BattleControl_Ext Instance
        {
            get
            {
                if (MainManager.battle == null)
                    return null;

                if (instance == null)
                {
                    instance = battle.gameObject.AddComponent<BattleControl_Ext>();
                }
                return instance;
            }
        }

        void Start()
        {
            CreateStylishBar();
        }

        void Update()
        {
            if (VengeanceCondition)
            {
                for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                {
                    if (CheckVengeance(i) && MainManager.instance.playerdata[i].charge < vengeanceMax)
                    {
                        InVengeance = true;
                        StartCoroutine(DoVengeance(i));
                    }
                }
            }

            if (!VengeanceCondition)
                InVengeance = false;
        }

        public void ResetStuff()
        {
            BattleControl_Ext.enemyUsedItem = false;

            var entitiesExt = FindObjectsOfType<Entity_Ext>();
            for (int i = 0; i != entitiesExt.Length; i++)
            {
                Destroy(entitiesExt[i]);
            }

            if (stylishBarHolder != null)
                Destroy(stylishBarHolder.gameObject);
            if (statusInfo != null && statusInfo.backgroundBox != null)
                Destroy(statusInfo.backgroundBox.gameObject);
            Dizzy.dizzyKOpositions.Clear();
            Dizzy.dizzyKO = false;
            Destroy(this);
        }

        public IEnumerator GetSkillList(int actionID)
        {
            yield return new WaitUntil(() => destroyedList);
            destroyedList = false;
            int playerId = 0;
            if (actionID == 50)
                playerId = 1;
            else if (actionID == 51)
                playerId = 2;
            else if (actionID == 52)
                playerId = 3;
            holoSkillID = playerId - 1;
            MainManager.battle.currentaction = BattleControl.Pick.SkillList;
            MainManager.RefreshSkills();
            MainManager.SetUpList(-playerId, true, false);
            MainManager.listammount = 5;
            MainManager.ShowItemList(-playerId, MainManager.defaultlistpos, true, false);
        }
        public delegate int DoDamageDelegateNoBlock(ref MainManager.BattleData target, int amount, BattleControl.AttackProperty? property);

        IEnumerator DoLifeLust(BattleControl __instance, MainManager.BattleData player, Entity_Ext entityExt)
        {
            battle.GetAvaliableTargets(false, false, -1, true);
            var targets = battle.avaliabletargets;

            targets = targets.Where(e => e.position != BattleControl.BattlePosition.Underground || e.position != BattleControl.BattlePosition.OutOfReach).ToArray();
            if (targets.Length > 0)
            {
                int healedThisTurn = entityExt.healedThisTurn;
                entityExt.healedThisTurn = 0;
                int rest = healedThisTurn % targets.Length;
                int result = healedThisTurn / targets.Length;
                var enemyMiddle = targets.Sum(a => a.battleentity.transform.position.x) / (float)targets.Length;
                var middlePoint = new Vector3(enemyMiddle, 5);

                CreateBeam(player.battleentity.transform.position, middlePoint, player.battleentity.transform);
                __instance.StartCoroutine(FadeImage(middlePoint, 90f));
                yield return null;


                for (int i = 0; i != targets.Length; i++)
                {
                    int damageAmount = result;

                    if (rest > 0)
                    {
                        damageAmount++;
                        rest--;
                    }

                    if (damageAmount > 0)
                    {
                        var enemy = targets[i];
                        CreateBeam(enemy.battleentity.sprite.transform.position, middlePoint, enemy.battleentity.transform);
                        battle.DoDamage(null, ref __instance.enemydata[targets[i].battleentity.battleid], damageAmount, null, new DamageOverride[] { DamageOverride.NoFall }, false);
                    }
                }
                yield return EventControl.halfsec;
            }
        }

        void CreateBeam(Vector3 startPos, Vector3 endPos, Transform parent)
        {
            var go = Instantiate(Resources.Load("Prefabs/Particles/Heal")) as GameObject;
            var beam = go.GetComponent<ParticleSystem>();

            var main = beam.main;
            main.startSize = 0.5f;

            var col = beam.colorOverLifetime;
            col.enabled = true;

            Gradient grad = new Gradient();
            grad.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.red, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(0.5f, 0.0f), new GradientAlphaKey(1f, 1.0f) });

            col.color = grad;
            main.startRotation = 0;
            go.transform.position = (endPos + startPos) / 2;
            var distance = Vector3.Distance(endPos, startPos);

            ParticleSystem.ShapeModule sm = beam.shape;
            sm.rotation = new Vector3(0, 90f);
            beam.transform.LookAt(endPos);
            sm.shapeType = ParticleSystemShapeType.SingleSidedEdge;
            sm.radiusMode = ParticleSystemShapeMultiModeValue.BurstSpread;
            sm.radius = distance;

            ParticleSystem.EmissionModule em = beam.emission;
            int numParticles = (int)(distance * 100);
            ParticleSystem.Burst b = new ParticleSystem.Burst(0, numParticles);
            em.SetBurst(0, b);

            Destroy(go, 3f);
        }

        IEnumerator FadeImage(Vector3 position, float frameTime)
        {
            GameObject heart = new GameObject("heart");
            var spriteR = heart.AddComponent<SpriteRenderer>();
            spriteR.sprite = MainManager.itemsprites[1, (int)Medal.LifeLust];
            heart.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            spriteR.sortingOrder = 1;
            heart.transform.position = position;

            float a = 0f;
            Color ic = spriteR.material.color;
            do
            {
                var scale = Mathf.Lerp(spriteR.transform.localScale.x, 3f, a / frameTime);
                spriteR.transform.localScale = new Vector3(scale, scale, scale);
                spriteR.material.color = new Color(ic.r, ic.g, ic.b, Mathf.Lerp(ic.a, 0f, a / frameTime));
                a += MainManager.framestep;
                yield return null;
            }
            while (a < frameTime);

            Destroy(heart);
        }

        IEnumerator TeamEffortCheck()
        {
            var idRequired = new List<int>();

            foreach (var player in MainManager.instance.playerdata)
                idRequired.Add(player.battleentity.battleid);

            if (!idRequired.Except(attackedThisTurn).Any())
            {
                if (MainManager.BadgeIsEquipped((int)Medal.TeamEffort))
                {
                    battle.HealTP(2);
                    yield return EventControl.halfsec;
                }

                if (MainManager.BadgeIsEquipped((int)Medal.TeamCheer))
                {
                    for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                    {
                        if (MainManager.instance.playerdata[i].hp > 0)
                            battle.Heal(ref MainManager.instance.playerdata[i], 1, false);
                    }
                    yield return EventControl.halfsec;
                }
            }
            attackedThisTurn.Clear();
        }


        IEnumerator DoVengeance(int index)
        {
            MainManager.instance.playerdata[index].charge = vengeanceMax;
            battle.StartCoroutine(battle.StatEffect(MainManager.instance.playerdata[index].battleentity, 4));
            yield return EventControl.halfsec;
            MainManager.PlaySound("Wam");
            MainManager.PlaySound("StatUp", -1, 1.25f, 1f);
        }

        public static bool CheckVengeanceCharge() => Instance.InVengeance && !MainManager.battle.enemy && MainManager.battle.currentaction != BattleControl.Pick.ItemList;

        public static bool CheckVengeance(int index)
        {
            return VengeanceCondition && MainManager.instance.playerdata[index].hp > 0 && MainManager.HasCondition(MainManager.BattleCondition.Eaten, MainManager.instance.playerdata[index]) == -1 && MainManager.BadgeIsEquipped((int)Medal.Vengeance, MainManager.instance.playerdata[index].trueid);
        }

        public static int GetEquippedMedalBug(Medal medal, Func<int, bool> condition)
        {
            for (int i = 0; i != MainManager.instance.playerdata.Length; i++)
            {
                if (MainManager.BadgeIsEquipped((int)medal, MainManager.instance.playerdata[i].trueid) && condition(i))
                {
                    return i;
                }
            }
            return -1;
        }

        public static int GetEquippedMedalBug(Medal medal)
        {
            for (int i = 0; i != MainManager.instance.playerdata.Length; i++)
            {
                if (MainManager.BadgeIsEquipped((int)medal, MainManager.instance.playerdata[i].trueid))
                {
                    return i;
                }
            }
            return -1;
        }

        public static int GetEquippedMedalBug(int medal) => GetEquippedMedalBug((Medal)medal);


        public void PotentialEnergyCheck(ref MainManager.BattleData player)
        {
            if (MainManager.HasCondition(MainManager.BattleCondition.Numb, player) > -1 && MainManager.BadgeIsEquipped((int)Medal.PotentialEnergy, player.trueid) && player.moreturnnextturn < 15)
            {
                player.moreturnnextturn += 1;
                MainManager.PlaySound("Heal3");
                battle.StartCoroutine(battle.StatEffect(player.battleentity, 5));
            }
        }

        public void CheckFlavorCharger(MainManager.ItemUsage type, int? characterid)
        {
            MainManager.ItemUsage[] usages = new MainManager.ItemUsage[]{
                MainManager.ItemUsage.HPorDamage,
                MainManager.ItemUsage.HPRecover, MainManager.ItemUsage.Revive, MainManager.ItemUsage.HPRecoverFull,
                MainManager.ItemUsage.HPto1,MainManager.ItemUsage.TPRecover,MainManager.ItemUsage.TPRecoverFull,
            };

            if (usages.Contains(type) && MainManager.BadgeIsEquipped((int)Medal.FlavorCharger, characterid.Value))
            {
                DoFlavorCharger(characterid);
            }
            else
            {
                usages = new MainManager.ItemUsage[]
                {
                    MainManager.ItemUsage.HPRecoverAll, MainManager.ItemUsage.HPto1All,
                    MainManager.ItemUsage.ReviveAll
                };

                if (usages.Contains(type))
                {
                    for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                    {
                        if (MainManager.BadgeIsEquipped((int)Medal.FlavorCharger, i) && (MainManager.instance.playerdata[i].hp > 0 || type == ItemUsage.ReviveAll))
                        {
                            DoFlavorCharger(i);
                        }
                    }
                }
            }
        }

        public void DoFlavorCharger(int? characterid)
        {
            MainManager.PlaySound("StatUp", -1, 1.25f, 1f);
            battle.StartCoroutine(battle.StatEffect(MainManager.instance.playerdata[characterid.Value].battleentity, 4));
            MainManager.instance.playerdata[characterid.Value].charge = Mathf.Clamp(MainManager.instance.playerdata[characterid.Value].charge + 1, 0, MainManager_Ext.CheckMaxCharge(characterid.Value));
        }


        public static bool VengeanceCondition => MainManager.GetAlivePlayerAmmount() == 1 && MainManager.instance.playerdata.Length != 1;

        public void DoPoison(ref MainManager.BattleData etarget)
        {
            int poisonTurn = MainManager.HasCondition(MainManager.BattleCondition.Poison, etarget);
            battle.TryCondition(ref etarget, BattleCondition.Poison, 2);
            if (poisonTurn != MainManager.HasCondition(MainManager.BattleCondition.Poison, etarget))
            {
                MainManager.PlayParticle("poisoneffect", etarget.battleentity.transform.position + new Vector3(0, etarget.battleentity.height));
            }
        }

        static int CheckPerkfectItemDrop(int baseChance, EntityControl entity)
        {
            //int baseChance = !MainManager.BadgeIsEquipped(18) ? !MainManager.BadgeIsEquipped(11) && !MainManager.instance.flags[614] ? -3 : -1 : -7;
            int itemChance = Mathf.Clamp(baseChance, baseChance, 0);

            int[] seedlings = new int[]
            {
               (int)MainManager.Enemies.Seedling,
               (int)MainManager.Enemies.FlyingSeedling,
               (int)MainManager.Enemies.Acornling,
               (int)MainManager.Enemies.Cactus,
               (int)MainManager.Enemies.Flowering,
               (int)MainManager.Enemies.Underling,
               (int)MainManager.Enemies.GoldenSeedling,
               (int)NewEnemies.Caveling,
               (int)NewEnemies.FlyingCaveling,
               (int)MainManager.Enemies.Plumpling,
            };
            bool gotWhistle = false;
            foreach (int enemyDefeated in MainManager.instance.lastdefeated)
            {
                if (!gotWhistle && seedlings.Contains(enemyDefeated) && UnityEngine.Random.Range(0, 100) == 0)
                {
                    gotWhistle = true;
                    MainManager_Ext.CreateItemEntity((int)NewItem.SeedlingWhistle, entity, entity.spritetransform.position, 0);
                }
            }

            var npcExt = NPCControl_Ext.GetNPCControl_Ext(entity.npcdata);

            List<int> items = new List<int>();
            for (int i = 0; i < npcExt.items.Length; i++)
            {
                if (npcExt.items[i] != -1 && !npcExt.usedItem[i])
                    items.Add(npcExt.items[i]);
            }

            foreach (int item in items)
            {
                if (UnityEngine.Random.Range(0, 100) < 50)
                {
                    MainManager_Ext.CreateItemEntity(item, entity, entity.spritetransform.position, 0);
                    break;
                }
            }
            return UnityEngine.Random.Range(itemChance, entity.npcdata.vectordata.Length);
        }

        static void CheckHoloSkill()
        {
            EntityControl entity = MainManager.instance.playerdata[MainManager.battle.currentturn].battleentity;
            if (Instance.holoSkillID != -1 && entity.CompareTag("Player"))
            {
                MainManager.battle.StartCoroutine(Instance.UseHoloSkill(entity, MainManager.battle.selecteditem));
            }
            else
            {
                MainManager.battle.StartCoroutine(battle.DoAction(entity, battle.selecteditem));
            }
        }

        IEnumerator UseHoloSkill(EntityControl entity, int actionid)
        {
            if (MainManager.battle.cancelupdate)
            {
                yield return null;
                yield break;
            }
            MainManager.battle.overridechallengeblock = false;
            MainManager.battle.CancelInvoke("UpdateAnim");
            battle.DestroyHelpBox();
            MainManager.battle.action = true;
            battle.UpdateText();
            battle.UpdateAnim();

            entity.animstate = 4;
            MainManager.PlaySound("ItemHold");
            SpriteRenderer itemSprite = new GameObject().AddComponent<SpriteRenderer>();
            itemSprite.transform.position = entity.transform.position + new Vector3(0f, 2.5f, -0.1f);
            itemSprite.sprite = MainManager.itemsprites[1, (int)Medal.HoloSkill];
            itemSprite.material.renderQueue = 50000;
            itemSprite.gameObject.layer = 14;
            yield return EventControl.halfsec;
            Destroy(itemSprite.gameObject);
            MainManager.PlaySound("Scanner1");
            entity.spin = new Vector3(0, 30, 0);
            yield return EventControl.halfsec;
            oldAnimID = entity.animid;
            entity.animid = holoSkillID;
            entity.hologram = true;
            entity.UpdateSpriteMat();

            yield return EventControl.quartersec;
            entity.spin = Vector3.zero;
            MainManager.battle.StartCoroutine(battle.DoAction(entity, actionid));
        }



        IEnumerator DoWildfire()
        {
            var targets = new List<MainManager.BattleData>();

            for (int i = 0; i != MainManager.instance.playerdata.Length; i++)
            {
                if (MainManager.instance.playerdata[i].hp > 0 && MainManager.HasCondition(MainManager.BattleCondition.Sturdy, MainManager.instance.playerdata[i]) == -1)
                {
                    targets.Add(MainManager.instance.playerdata[i]);
                }
            }
            targets.AddRange(MainManager.battle.enemydata);

            var randomTarget = targets[UnityEngine.Random.Range(0, targets.Count)];
            MainManager.PlaySound("Flame");
            MainManager.PlayParticle("Fire", randomTarget.battleentity.transform.position + new Vector3(0, randomTarget.battleentity.height), 1f);
            int[] limit = { (int)MainManager.Enemies.KeyL, (int)MainManager.Enemies.KeyR, (int)MainManager.Enemies.Tablet };

            if (!limit.Any(l => l == randomTarget.animid))
            {
                MainManager.SetCondition(MainManager.BattleCondition.Fire, ref randomTarget, 3);
            }
            yield return EventControl.halfsec;
        }


        public static int GetCurrentBurnDMG(BattleData target, out int rawDMG)
        {
            bool zeroOut = false;

            rawDMG = 3;
            if (BadgeIsEquipped((int)Medal.HeatingUp))
            {
                var entityExt = Entity_Ext.GetEntity_Ext(target.battleentity);
                rawDMG += entityExt.fireDamage;
            }
            if (!target.battleentity.isplayer)
            {
                switch (target.animid)
                {
                    case (int)NewEnemies.FirePopper:
                        zeroOut = true;
                        break;
                }
            }
            return zeroOut ? 0 : Mathf.Max(0, rawDMG);
        }
        public static int DoHeatingUp(BattleData target)
        {
            int burnDamage = GetCurrentBurnDMG(target, out int rawDMG);

            if (BadgeIsEquipped((int)Medal.HeatingUp))
            {
                var entityExt = Entity_Ext.GetEntity_Ext(target.battleentity);
                entityExt.fireDamage++;
            }

            if (BadgeIsEquipped((int)Medal.FierySpirit) && burnDamage > 0)
            {
                Instance.DoFierySpirit(burnDamage, target);
            }

            Instance.DoFirePopperBurnHeal();

            if (!target.battleentity.isplayer)
            {
                switch (target.animid)
                {
                    case (int)NewEnemies.Moeruki:
                        Instance.ChargeUp(ref target, rawDMG, 0.25f);
                        break;
                }
            }

            if (burnDamage > 0)
            {
                Instance.CheckFieryHeart();
            }
            return burnDamage;
        }

        void DoFirePopperBurnHeal()
        {
            int heal = 2;
            for (int i = 0; i < battle.enemydata.Length; i++)
            {
                if (battle.enemydata[i].animid == (int)NewEnemies.FirePopper)
                {
                    battle.Heal(ref battle.enemydata[i], heal);
                }
            }
        }

        void CheckFieryHeart()
        {
            for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
            {
                if (BadgeIsEquipped((int)Medal.FieryHeart, MainManager.instance.playerdata[i].trueid) && MainManager.instance.playerdata[i].hp > 0)
                {
                    FieryHeartHeal(i);
                }
            }
        }

        void DoFierySpirit(int burnDamage, BattleData target)
        {
            int tpHeal = Mathf.Max(1, burnDamage / 2);
            Instance.RecoverPlayerTP(tpHeal, target);
        }

        public static bool IsEnemyBurnKOImmune(int animid)
        {
            return
                animid == (int)MainManager.Enemies.WaspKingIntermission ||
                animid == (int)MainManager.Enemies.WaspKing ||
                animid == (int)MainManager.Enemies.EverlastingKing ||
                animid == (int)NewEnemies.FireAnt ||
                animid == (int)NewEnemies.Moeruki ||
                animid == (int)NewEnemies.MechaJaw ||
                animid == (int)NewEnemies.FirePopper ||
                animid == (int)NewEnemies.Jester;
        }
        public static void FieryHeartHeal(int bug, int healMult = 1)
        {
            battle.StartCoroutine(battle.ItemSpinAnim(MainManager.instance.playerdata[bug].battleentity.transform.position + Vector3.up, itemsprites[1, (int)Medal.FieryHeart], true));
            battle.Heal(
                entity: ref MainManager.instance.playerdata[bug],
                ammount: BadgeHowManyEquipped((int)Medal.FieryHeart, MainManager.instance.playerdata[bug].trueid) * healMult,
                nosound: false);
        }

        IEnumerator DoBurnKO(List<BattleData> targets)
        {
            targets = targets.Where(t => t.hp > 0 && t.position != BattlePosition.OutOfReach).ToList();
            bool someoneDied = false;
            for (int i = 0; i < targets.Count; i++)
            {
                int burnDMG = GetCurrentBurnDMG(targets[i], out _);

                if (burnDMG > 0 && burnDMG >= targets[i].hp)
                {
                    someoneDied = true;
                    DamageOverride[] damageOverrides = new DamageOverride[] { DamageOverride.NoIceBreak, DamageOverride.DontAwake, DamageOverride.IgnoreNumb, (DamageOverride)NewDamageOverride.StatusDamage };

                    PlaySound("Flame");
                    if (!targets[i].battleentity.isplayer)
                        battle.DoDamage(null, ref battle.enemydata[targets[i].battleentity.battleid], burnDMG, AttackProperty.NoExceptions, damageOverrides, false);
                    else
                        battle.DoDamage(null, ref MainManager.instance.playerdata[targets[i].trueid], burnDMG, AttackProperty.NoExceptions, damageOverrides, false);

                    DoHeatingUp(targets[i]);

                    if (BadgeIsEquipped((int)Medal.HeatingUp))
                    {
                        var entityExt = Entity_Ext.GetEntity_Ext(targets[i].battleentity);
                        entityExt.fireDamage = 0;
                    }
                }
            }
            if (someoneDied)
                yield return EventControl.halfsec;
        }

        public void ChargeUp(ref BattleData target, int amount, float delay, bool particle = true, float? sfxPitch = null)
        {
            int maxCharge = target.battleentity.CompareTag("Player") ? MainManager_Ext.CheckMaxCharge(target.trueid) : Instance.GetMaxEnemyCharge(target.battleentity);
            target.charge = Mathf.Min(target.charge + amount, maxCharge);

            if (!particle) 
                return;

            if (amount > 0)
                battle.StartCoroutine(MultiArrow(target.battleentity, 4, amount, delay, sfxPitch));
        }
        public void HustleUp(ref BattleData target, int amount, float delay, bool immediately, bool particle = true, float? sfxPitch = null)
        {
            if (immediately)
                target.cantmove -= amount;
            else
                target.moreturnnextturn = Mathf.Min(8, target.moreturnnextturn + amount);

            if (!particle) 
                return;

            if (amount > 0)
                battle.StartCoroutine(MultiArrow(target.battleentity, 5, amount, delay, sfxPitch));
        }
        public IEnumerator MultiArrow(EntityControl target, int type, int amount, float delay, float? sfxPitch = null)
        {
            string sfx = type == 4 ? "StatUp" : "Heal3";

            delay /= Mathf.Max(1f, amount - 1f);
            delay *= 1f + 0.06f * Mathf.Max(1f, amount - 1f);
            for (int i = 0; i < amount; i++)
            {
                battle.StartCoroutine(battle.StatEffect(target, type));
                PlaySound(sfx, -1,
                    pitch: sfxPitch ?? (0.9f + 0.1f * (i + 1f)),
                    volume: amount == 1 ? 1 : Mathf.Lerp(0.5f, 1, Mathf.Pow(i / (amount - 1), 2f)));
                yield return new WaitForSeconds(delay);
            }
        }

        IEnumerator DoPerkfectionist()
        {
            int amount = Instance.perfectKillAmount;
            Instance.perfectKillAmount = 0;
            bool perfectKill = Instance.perfectKill;
            Instance.perfectKill = false;

            if (perfectKill)
            {
                for (int i = 0; i != MainManager.instance.playerdata.Length; i++)
                {
                    if (MainManager.instance.playerdata[i].hp > 0)
                        battle.Heal(ref MainManager.instance.playerdata[i], 1 * amount, false);
                }
                yield return EventControl.quartersec;

                RecoverPlayerTP(2 * amount, battle.partymiddle + Vector3.up);
                yield return EventControl.quartersec;
            }
        }

        static IEnumerator CheckPerkfectionist()
        {
            if (Instance.stylishRoutine > 0)
                yield return new WaitUntil(() => Instance.stylishRoutine <= 0);

            if (stylishBarAmount >= 1)
            {
                yield return MainManager.battle.StartCoroutine(Instance.DoStylishReward());
            }

            if (MainManager.BadgeIsEquipped((int)Medal.Perkfectionist))
            {
                yield return MainManager.battle.StartCoroutine(Instance.DoPerkfectionist());
            }

            if (MainManager.BadgeIsEquipped((int)Medal.StrikeBlaster))
            {
                yield return new WaitUntil(() => Instance.strikeBlasterManager == null);
                Instance.strikeBlasters.Clear();
            }
        }

        static void DoInkBlotEnemy()
        {
            if (MainManager.BadgeIsEquipped((int)Medal.Inkblot))
            {
                if (battle.enemydata.Length > 1)
                {
                    for (int i = 0; i < battle.enemydata.Length; i++)
                    {
                        if (battle.enemydata[i].hp <= 0)
                        {
                            for (int j = 0; j < battle.enemydata.Length; j++)
                            {
                                if (i != j && battle.enemydata[j].hp > 0 && battle.enemydata[j].position != BattlePosition.Underground)
                                {
                                    EntityControl targetEntity = battle.enemydata[j].battleentity;
                                    bool isClose = MainManager.GetSqrDistance(targetEntity.transform.position + targetEntity.freezeoffset + Vector3.up * targetEntity.height, battle.enemydata[i].battleentity.transform.position) <= 15.5f;

                                    if (isClose)
                                    {
                                        Vector3 particlePos = battle.enemydata[j].battleentity.transform.position + Vector3.up + battle.enemydata[j].battleentity.height * Vector3.up;
                                        Instance.ApplyStatus(BattleCondition.Inked, ref battle.enemydata[j], 2, "WaterSplash2", 0.8f, 1, "InkGet", particlePos, Vector3.one);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        static void DoInkBlotPlayer(int playerid)
        {
            if (MainManager.instance.playerdata[playerid].hp <= 0 && MainManager.BadgeIsEquipped((int)Medal.Inkblot))
            {
                for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                {
                    if (i != playerid && MainManager.instance.playerdata[i].hp > 0 && MainManager.HasCondition(MainManager.BattleCondition.Sturdy, MainManager.instance.playerdata[i]) == -1)
                    {
                        EntityControl targetEntity = MainManager.instance.playerdata[i].battleentity;
                        float distance = MainManager.GetSqrDistance(targetEntity.transform.position, MainManager.instance.playerdata[playerid].battleentity.transform.position);
                        bool isClose = distance <= 6f;

                        if (isClose)
                        {
                            Vector3 particlePos = MainManager.instance.playerdata[i].battleentity.transform.position + Vector3.up;
                            Instance.ApplyStatus(BattleCondition.Inked, ref MainManager.instance.playerdata[i], 2, "WaterSplash2", 0.8f, 1, "InkGet", particlePos, Vector3.one);
                        }
                    }
                }
            }
        }

        int CheckInkBlotAdjacent(int arrayLength, int index, ref bool condition, bool after)
        {
            int nextTarget = index - 1;
            condition = nextTarget >= 0;
            if (after)
            {
                nextTarget = index + 1;
                condition = nextTarget < arrayLength;
            }
            return nextTarget;
        }

        static BattleCondition[] CryostasisExceptions()
        {
            return new BattleCondition[] {
                BattleCondition.EventStop,
                BattleCondition.Eaten,
                BattleCondition.Topple,
                BattleCondition.Flipped,
                BattleCondition.Taunted,
                BattleCondition.Reflection,
                (BattleCondition)NewCondition.Vitiation,
            };
        }

        static bool CheckCryostatis(MainManager.BattleData target, int indexCondition)
        {
            MainManager.BattleCondition[] exceptions = CryostasisExceptions();
            int[] noTurnsReduction = new int[]
            {
                (int)MainManager.BattleCondition.Shield,
                (int)NewCondition.Paintball,
                (int)NewCondition.Slugskin
            };

            if (noTurnsReduction.Contains(target.condition[indexCondition][0]))
            {
                return true;
            }

            if (target.condition[indexCondition][0] == (int)NewCondition.Vitiation)
            {
                target.condition[indexCondition][1] -= Mathf.Max(10, target.condition[indexCondition][1] / 2);
                return true;
            }

            if (!exceptions.Contains((MainManager.BattleCondition)target.condition[indexCondition][0]))
            {
                for (int i = 0; i != MainManager.instance.playerdata.Length; i++)
                {
                    if (MainManager.HasCondition(MainManager.BattleCondition.Freeze, MainManager.instance.playerdata[i]) > -1
                        && MainManager.BadgeIsEquipped((int)Medal.Cryostatis, MainManager.instance.playerdata[i].trueid)
                        && (!target.battleentity.playerentity || target.trueid != MainManager.instance.playerdata[i].trueid))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static IEnumerator EndOfTurnCheck()
        {
            var battle = MainManager.battle;
            battle.action = true;
            Instance.entityAttacking = null;

            if (!battle.firststrike)
            {
                yield return new WaitUntil(() => battle.mainturn != null);

                for (int i = 0; i != MainManager.instance.playerdata.Length; i++)
                {
                    var entityExt = Entity_Ext.GetEntity_Ext(MainManager.instance.playerdata[i].battleentity);
                    if (entityExt.isPlayer && MainManager.instance.playerdata[i].hp > 0 && BadgeIsEquipped((int)BadgeTypes.LastWind, MainManager.instance.playerdata[i].trueid) && entityExt.lastTurnHp > MainManager.instance.playerdata[i].hp && entityExt.lastTurnHp - MainManager.instance.playerdata[i].hp >= 8)
                    {
                        MainManager.battle.StartCoroutine(battle.ItemSpinAnim(MainManager.instance.playerdata[i].battleentity.transform.position + Vector3.up, MainManager.itemsprites[1, (int)BadgeTypes.LastWind], true));
                        MainManager.instance.playerdata[i].cantmove--;
                    }
                    entityExt.lastTurnHp = MainManager.instance.playerdata[i].hp;
                }

                yield return Instance.DoDelProjPlayer();

                battle.UpdateText();
                Instance.usedPebbleToss = false;
                Instance.twinedFateUsed = false;
                DarkTeamSnakemouth.RefreshRelay();
                Sleep.CheckSleepSchedule();

                if (MainManager.BadgeIsEquipped((int)Medal.TeamEffort) || MainManager.BadgeIsEquipped((int)Medal.TeamCheer))
                    yield return battle.StartCoroutine(Instance.TeamEffortCheck());

                if (MainManager.BadgeIsEquipped((int)Medal.Wildfire) && MainManager.battle.enemydata.Length > 0)
                    yield return battle.StartCoroutine(Instance.DoWildfire());

                int turns = battle.turns;

                if (turns == Instance.trustFallTurn + 1 && Instance.trustFallTurn != -1)
                {
                    battle.HealTP(Instance.trustFallDamage);
                    Instance.trustFallTurn = -1;
                    Instance.trustFallDamage = 0;
                    yield return EventControl.halfsec;
                }
                Instance.inEndOfTurnDamage = true;

                if (MainManager.BadgeIsEquipped((int)Medal.Hailstorm) && Instance.CheckHailstorm())
                    yield return battle.StartCoroutine(Instance.DoHailStorm(false));

                for (int i = 0; i != MainManager.instance.playerdata.Length; i++)
                {
                    var entityExt = Entity_Ext.GetEntity_Ext(MainManager.instance.playerdata[i].battleentity);
                    entityExt.inkWellActive = false;
                    entityExt.adrenalineUsed = false;
                    entityExt.inkblotActive = false;
                    entityExt.didSpinout = false;
                    entityExt.ResetCarousel();

                    Instance.CheckHDWGHConditionAmount(MainManager.instance.playerdata[i], entityExt);
                    if (entityExt.smearchargeActive)
                    {
                        entityExt.smearchargeActive = false;
                        Instance.DoSmearcharge(ref MainManager.instance.playerdata[i]);
                        yield return EventControl.halfsec;
                    }

                    if (battle.AliveEnemies() > 0)
                    {
                        if (entityExt.healedThisTurn > 0 && MainManager.BadgeIsEquipped((int)Medal.LifeLust, MainManager.instance.playerdata[i].trueid) && MainManager.instance.playerdata[i].hp > 0)
                            yield return battle.StartCoroutine(Instance.DoLifeLust(battle, MainManager.instance.playerdata[i], entityExt));

                        if (MainManager.BadgeIsEquipped((int)Medal.Nightmare, MainManager.instance.playerdata[i].trueid) && MainManager.HasCondition(MainManager.BattleCondition.Sleep, MainManager.instance.playerdata[i]) > -1)
                            yield return battle.StartCoroutine(Sleep.DoNightmare(battle, MainManager.instance.playerdata[i]));

                        if (BadgeIsEquipped((int)Medal.Mothflower)
                            && Instance.mothFlowerBlocks > 0
                            && MainManager.instance.playerdata[i].trueid == 2
                            && MainManager.instance.playerdata[i].hp > 0)
                        {
                            yield return battle.StartCoroutine(Instance.DoMothflower(MainManager.instance.playerdata[i]));
                        }
                    }
                }
                Instance.inEndOfTurnDamage = false;
                Instance.mothFlowerBlocks = 0;
                Instance.mothFlowerSuperBlocks = 0;

                if (battle.AliveEnemies() > 0) // don't want burn/wildfire to kill you after KOing a boss or something
                {
                    List<BattleData> burningTargets = battle.enemydata.Where(e => HasCondition(BattleCondition.Fire, e) > -1 && !IsEnemyBurnKOImmune(e.animid)).ToList();
                    burningTargets.AddRange(MainManager.instance.playerdata.Where(p => HasCondition(BattleCondition.Fire, p) > -1 && !BadgeIsEquipped((int)Medal.FieryHeart, p.trueid)).ToList());
                    if (burningTargets.Count > 0)
                    {
                        yield return battle.StartCoroutine(Instance.DoBurnKO(burningTargets));
                    }
                }

                yield return Instance.EnemyPostTurnProcess();
                if (battle.AliveEnemies() > 0)
                {
                    yield return Instance.CheckReviveEnemies();
                }

                yield return battle.StartCoroutine(battle.CheckDead());

                for (int i = 0; i < battle.enemydata.Length; i++)
                {
                    battle.enemydata[i].cantmove = 1;
                    if (battle.enemydata[i].animid == (int)NewEnemies.IronSuit)
                    {
                        yield return IronSuitAI.ChangeForm(battle.enemydata[i].battleentity, battle.enemydata[i].battleentity.GetComponent<IronSuit>(), i);
                    }
                }             
                
                if (battle.inevent)
                    yield return new WaitUntil(() => battle == null || !battle.inevent);
            }
        }

        IEnumerator DoMothflower(BattleData leif)
        {
            var entityExt = Entity_Ext.GetEntity_Ext(leif.battleentity);

            BattleData[] targets = battle.enemydata.Where(e => e.hp > 0 && e.position != BattlePosition.OutOfReach).ToArray();
            if (targets.Length == 0)
                yield break;

            int totalDamage = Instance.mothFlowerBlocks;
            if (entityExt.slugskinActive)
                totalDamage += targets.Length;

            int dmgPerTarget = totalDamage / targets.Length;
            int excess = totalDamage % targets.Length;

            int earlyTotalDmg = totalDamage + Mathf.Min(totalDamage, targets.Length) * Instance.mothFlowerSuperBlocks;

            float intensity = Mathf.Pow(Mathf.InverseLerp(0, 30, totalDamage), 2f);
            float pitch = Mathf.Lerp(1.6f, 1.2f, intensity);
            PlaySound("IceMothHit", pitch, Mathf.Lerp(0.9f, 2, intensity));
            PlayParticle("mothicenormal", leif.battleentity.transform.position + leif.battleentity.height * Vector3.up, 1.5f)
                .transform.localScale *= Mathf.Lerp(1, 2, intensity);

            int prevAnimState = leif.battleentity.animstate;
            bool prevAnimOverride = leif.battleentity.overrideanim;
            if (!battle.IsStopped(leif))
            {
                leif.battleentity.overrideanim = true;
                leif.battleentity.animstate = 102;
                yield return new WaitForSeconds(0.75f);
                leif.battleentity.animstate = 119;
                yield return EventControl.quartersec;
            }
            else
            {
                yield return EventControl.halfsec;
            }

            GameObject[] iceSpikes = new GameObject[targets.Length];

            float waitTime = Mathf.Lerp(0.2f, 0.04f, Mathf.InverseLerp(4, 20, targets.Length));

            for (int i = 0; i != targets.Length; i++)
            {
                EntityControl targetEntity = targets[i].battleentity;
                int targetId = targetEntity.battleid;

                int DMG = dmgPerTarget;

                if (excess > 0)
                {
                    DMG++;
                    excess--;
                }

                if (DMG <= 0)
                    continue;

                if (HasWeakness(AttackProperty.Magic, targets[i]))
                    DMG++;

                DMG += Instance.mothFlowerSuperBlocks;
                totalDamage += Instance.mothFlowerSuperBlocks;

                DamageOverride[] overrides;
                if (targets[i].position == BattlePosition.Underground)
                {
                    iceSpikes[i] = Instantiate(Resources.Load("Prefabs/Objects/icepillar"), targetEntity.transform.position,
                        Quaternion.Euler(-90f, 0f, 0f)) as GameObject;

                    DialogueAnim dAnim = iceSpikes[i].AddComponent<DialogueAnim>();
                    float sizeFac = Mathf.Clamp01(DMG / 10f);

                    dAnim.targetscale = new Vector3(Mathf.Lerp(0.5f, 1f, sizeFac),
                        Mathf.Lerp(0.5f, 1f, sizeFac), Mathf.Lerp(0.8f, 1.6f, sizeFac));

                    iceSpikes[i].transform.localScale = Vector3.zero;
                    overrides = new DamageOverride[2] { DamageOverride.NoFall, DamageOverride.NoSound };
                    pitch = 0.1f;
                }
                else
                {
                    PlayParticle("mothicenormal", targetEntity.transform.position + targetEntity.height * Vector3.up,
                        1.5f).transform.localScale *= Mathf.Lerp(1, 2, DMG / 10f);
                    overrides = new DamageOverride[1] { DamageOverride.NoFall };
                    pitch = 0;
                }

                battle.DoDamage(null, ref battle.enemydata[targetId], DMG, AttackProperty.Pierce, overrides, false);

                if (BadgeIsEquipped((int)Medal.Blightfury))
                    Instance.DoPoison(ref battle.enemydata[targetId]);

                intensity = Mathf.Pow(Mathf.InverseLerp(0, 30, totalDamage), 2f);
                pitch += Mathf.Lerp(1.1f, 0.7f, intensity);
                pitch = Mathf.Max(0.1f, pitch * Mathf.Pow(0.95f, i));
                PlaySound("IceMothHit", pitch, Mathf.Lerp(0.9f, 2, intensity));
                yield return new WaitForSeconds(waitTime);
            }

            foreach (GameObject spike in iceSpikes)
            {
                if (spike != null)
                {
                    DialogueAnim dAnim = spike.GetComponent<DialogueAnim>();
                    dAnim.shrink = true;
                    dAnim.shrinkspeed = 0.05f;
                    Destroy(spike, 5);
                    yield return new WaitForSeconds(waitTime);

                }
            }
            yield return EventControl.quartersec;

            leif.battleentity.animstate = prevAnimState;
            leif.battleentity.overrideanim = prevAnimOverride;
        }

        IEnumerator EnemyPostTurnProcess()
        {
            int length = battle.enemydata.Length;
            for (int i = 0; i < battle.enemydata.Length; i++)
            {
                if (battle.enemydata[i].battleentity != null)
                {
                    if (battle.enemydata[i].animid == (int)NewEnemies.FireAnt)
                    {
                        battle.SetData(i, 1);
                        battle.enemydata[i].data[0] = 0;
                    }

                    var entityExt = Entity_Ext.GetEntity_Ext(battle.enemydata[i].battleentity);
                    entityExt.inkblotActive = false;

                    if (battle.enemydata[i].animid == (int)NewEnemies.MechaJaw
                        && MainManager.GetAlivePlayerAmmount() > 0
                        && !battle.IsStopped(battle.enemydata[i]))
                    {
                        yield return battle.StartCoroutine(
                            battle.enemydata[i].battleentity.GetComponent<MechaJawComp>().DecreaseFuseTimer(battle.enemydata[i]));

                        if (length != battle.enemydata.Length)
                        {
                            yield return null;
                            if (battle.AliveEnemies() != 0)
                                yield return EnemyPostTurnProcess();
                            break;
                        }

                    }
                }
            }
        }

        IEnumerator CheckReviveEnemies()
        {
            bool revived = false;
            for (int i = 0; i < battle.reservedata.Count; i++)
            {
                var data = battle.reservedata[i];
                data.turnssincedeath++;
                battle.reservedata[i] = data;
                EntityControl entity = battle.reservedata[i].battleentity;

                if (battle.reservedata[i].animid == (int)NewEnemies.FirePopper && battle.reservedata[i].turnssincedeath >= 1)
                {
                    MainManager.PlaySound("Charge7", 0.9f, 1);
                    entity.StartCoroutine(entity.ShakeSprite(0.2f, 60f));
                    yield return EventControl.sec;
                    entity.overrideanim = true;
                    entity.animstate = 110;
                    MainManager.PlaySound("Boing1", 1f, 1);
                    battle.ReviveEnemy(i, 0.5f, false, true);
                    revived = true;
                    yield return EventControl.halfsec;
                    break;
                }
            }

            if (revived && battle.reservedata.Count > 0)
                yield return CheckReviveEnemies();
        }

        public void DoInkWellCheck(int damageDone, ref BattleData target, bool targetIsPlayer)
        {
            if (!battle.enemy && battle.chompyattack == null && Instance.entityAttacking != null && !inAiAttack && !targetIsPlayer && battle.currentturn != -1 && MainManager.BadgeIsEquipped((int)Medal.Inkwell, MainManager.instance.playerdata[battle.currentturn].trueid) && MainManager.HasCondition(BattleCondition.Inked, target) > -1 && MainManager.instance.playerdata[battle.currentturn].hp > 0 && !Instance.inEndOfTurnDamage)
            {
                var entityExt = Entity_Ext.GetEntity_Ext(Instance.entityAttacking);

                if (!entityExt.inkWellActive)
                {
                    MainManager.battle.StartCoroutine(MainManager.battle.ItemSpinAnim(Instance.entityAttacking.transform.position + Vector3.up, MainManager.itemsprites[1, (int)Medal.Inkwell], true));
                    battle.Heal(ref MainManager.instance.playerdata[battle.currentturn], damageDone);
                    entityExt.inkWellActive = true;
                }
            }
        }

        public void DoWebsheetCheck(BattleData? attacker, ref BattleData target, bool targetIsPlayer)
        {
            if (attacker != null && targetIsPlayer && MainManager.BadgeIsEquipped((int)Medal.WebSheet, target.trueid) && !battle.nonphyscal)
            {
                bool didAnim = false;
                if (MainManager.HasCondition(MainManager.BattleCondition.Sticky, attacker.Value) == -1)
                {
                    EntityControl enemyEntity = attacker.Value.battleentity;
                    BattleControl_Ext.Instance.ApplyStatus(BattleCondition.Sticky, ref battle.enemydata[enemyEntity.battleid], 2, "AhoneynationSpit", 1, 1, "StickyGet", enemyEntity.transform.position, Vector3.one);
                    MainManager.battle.StartCoroutine(MainManager.battle.ItemSpinAnim(target.battleentity.transform.position + Vector3.up, MainManager.itemsprites[1, (int)Medal.WebSheet], true));
                    didAnim = true;
                }

                var caller = battle.caller;
                if (caller != null && MainManager.instance.items[0].Count < MainManager.instance.maxitems)
                {
                    var entityExt = Entity_Ext.GetEntity_Ext(attacker.Value.battleentity);
                    int enemyID = attacker.Value.battleentity.battleid;

                    if (MainManager.BadgeIsEquipped((int)Medal.WebSheet, target.trueid) && MainManager.HasCondition(MainManager.BattleCondition.Sticky, target) > -1 && entityExt.itemId != -1)
                    {
                        MainManager.instance.items[0].Add(entityExt.itemId);
                        GameObject item = Instantiate(entityExt.item.gameObject);
                        item.transform.parent = target.battleentity.transform;
                        item.transform.localPosition = new Vector3(0, 1, -0.1f);
                        item.GetComponent<SpriteRenderer>().enabled = true;

                        if (!didAnim)
                            battle.StartCoroutine(battle.ItemSpinAnim(target.battleentity.transform.position + Vector3.up, MainManager.itemsprites[1, (int)Medal.WebSheet], true));
                        entityExt.itemId = -1;
                        NPCControl_Ext.GetNPCControl_Ext(caller).items[enemyID] = -1;

                        Destroy(entityExt.item.gameObject);
                        Destroy(item, 1);
                    }
                }

            }

        }

        IEnumerator DoNightmare(BattleControl battle, MainManager.BattleData asleepPlayer)
        {
            battle.GetAvaliableTargets(false, false, -1, false);
            BattleData randomTarget = asleepPlayer;
            bool playerTarget = true;
            if (battle.avaliabletargets.Length > 0 && UnityEngine.Random.Range(0, 10) >= 3)
            {
                randomTarget = battle.avaliabletargets[UnityEngine.Random.Range(0, battle.avaliabletargets.Length)];
                playerTarget = false;
            }

            Color? baseSkyboxColor = RenderSettings.skybox?.GetColor("_Tint");
            Color baseAmbientColor = RenderSettings.ambientLight;
            Color baseFogColor = RenderSettings.fogColor;
            float a = 0f;
            float b = 60f;
            do
            {
                RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, Color.black, a / b);
                RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, Color.black, a / b);
                RenderSettings.skybox?.SetColor("_Tint", Color.Lerp(baseSkyboxColor.Value, Color.black, a / b));
                a += MainManager.TieFramerate(1f);
                yield return null;
            }
            while (a < b + 1f);

            MainManager.PlaySound("OmegaEye", 1.5f);
            var eye = (Instantiate(Resources.Load("Prefabs/Objects/Eye")) as GameObject).transform;
            eye.transform.position = randomTarget.battleentity.transform.position + new Vector3(0, 15f);
            eye.transform.rotation = Quaternion.Euler(0, 180, 180);

            var light = eye.GetChild(2);
            Renderer[] componentsInChildren = light.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                componentsInChildren[i].material.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, componentsInChildren[i].material.color.a);
            }

            yield return EventControl.sec;
            Destroy(eye.gameObject);

            var hand = (Instantiate(Resources.Load("Prefabs/Objects/DeadHand")) as GameObject).GetComponent<Animator>();
            hand.Play("1");
            Vector3 targetPos = randomTarget.battleentity.transform.position + new Vector3(0f, 5.2f, -0.1f) + new Vector3(0, randomTarget.battleentity.height);
            hand.transform.position = new Vector3(targetPos.x, targetPos.y + 5, targetPos.z);

            MainManager.PlaySound("OmegaMove");
            var startPos = hand.transform.position;
            a = 0f;
            b = 60f;
            do
            {
                hand.transform.position = MainManager.SmoothLerp(startPos, targetPos, a / b);
                a += MainManager.TieFramerate(1f);
                yield return null;
            }
            while (a < b + 1f);

            hand.Play("0");

            if (playerTarget)
                battle.DoDamage(null, ref MainManager.instance.playerdata[asleepPlayer.battleentity.battleid], 3, BattleControl.AttackProperty.Pierce, new DamageOverride[] { DamageOverride.NoFall }, false);
            else
                battle.DoDamage(null, ref battle.enemydata[randomTarget.battleentity.battleid], 3, BattleControl.AttackProperty.Pierce, new DamageOverride[] { DamageOverride.NoFall }, false);
            yield return EventControl.halfsec;

            hand.Play("1");
            startPos = hand.transform.position;
            targetPos = hand.transform.position + new Vector3(0, 20);
            a = 0f;
            b = 60f;
            do
            {
                hand.transform.position = MainManager.SmoothLerp(startPos, targetPos, a / b);
                RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, baseAmbientColor, a / b);
                RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, baseFogColor, a / b);
                RenderSettings.skybox?.SetColor("_Tint", Color.Lerp(Color.black, baseSkyboxColor.Value, a / b));
                a += MainManager.TieFramerate(1f);
                yield return null;
            }
            while (a < b + 1f);

            Destroy(hand.gameObject);
            yield return null;
        }

        public static IEnumerator DoPhoenix(int playerId)
        {
            MainManager.battle.action = true;

            if (CheckPhoenix(playerId))
            {
                yield return Instance.FirePillarPhoenix(playerId);
                battle.RevivePlayer(playerId, 2, true);
                MainManager.instance.playerdata[playerId].battleentity.animstate = (int)MainManager.Animations.WeakBattleIdle;
                yield return EventControl.halfsec;
                battle.ClearStatus(ref MainManager.instance.playerdata[playerId]);
                MainManager.instance.playerdata[playerId].cantmove = 1;
            }
        }

        public static bool CheckPhoenix(int playerId)
        {
            return MainManager.HasCondition(MainManager.BattleCondition.Fire, MainManager.instance.playerdata[playerId]) > -1 && MainManager.BadgeIsEquipped((int)Medal.Phoenix, MainManager.instance.playerdata[playerId].trueid) && MainManager.instance.playerdata[playerId].hp <= 0;

        }

        IEnumerator FirePillarPhoenix(int playerid)
        {
            var sound = MainManager_Ext.assetBundle.LoadAsset<AudioClip>("phoenixres");
            MainManager.PlaySound(sound);
            EntityControl battleEntity = MainManager.instance.playerdata[playerid].battleentity;
            var oldShieldpos = battleEntity.overrideshieldpos;
            battleEntity.animstate = (int)MainManager.Animations.KO;
            battleEntity.spin = Vector3.zero;
            battleEntity.CreateShield();
            battleEntity.shieldenabled = true;
            battleEntity.bubbleshield.targetscale = new Vector3(3f, 2.5f, 2f);
            battleEntity.overrideshieldpos = new Vector3?(new Vector3(0f, 0.5f));
            var mats = battleEntity.bubbleshield.GetComponent<Renderer>().materials;
            mats[0].SetColor("_EmissionColor", new Color(0.99f, 0.615f, 0.01f, 0.5f));
            mats[1].SetColor("_OutlineColor", Color.red);

            DialogueAnim pillar = (Instantiate(Resources.Load("Prefabs/Objects/FirePillar 1"), battleEntity.transform.position, Quaternion.identity) as GameObject).AddComponent<DialogueAnim>();
            pillar.transform.parent = MainManager.battle.battlemap.transform;
            pillar.transform.localScale = new Vector3(0f, 1f, 0f);
            pillar.targetscale = new Vector3(1f, 1f, 1f);
            pillar.shrink = false;
            pillar.shrinkspeed = 0.015f;
            battleEntity.bubbleshield.shrinkspeed = 0.015f;
            yield return new WaitForSeconds(3f);
            pillar.shrinkspeed = 0.2f;
            MainManager.ShakeScreen(0.25f, 0.75f);
            battleEntity.bubbleshield.shrink = true;
            battleEntity.bubbleshield.shrinkspeed = 0.2f;
            battleEntity.bubbleshield.targetscale = Vector3.zero;
            battleEntity.animstate = (int)MainManager.Animations.Hurt;
            yield return new WaitForSeconds(1.5f);
            pillar.targetscale = new Vector3(0f, 1f, 0f);
            ParticleSystem[] particles = pillar.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Stop();
            }
            pillar.shrinkspeed = 0.1f;
            Destroy(pillar.gameObject, 2f);
            Destroy(battleEntity.bubbleshield.gameObject);
            battleEntity.shieldenabled = false;
            battleEntity.bubbleshield = null;
            battleEntity.CreateShield();
            battleEntity.overrideshieldpos = oldShieldpos;
        }

        public delegate int DoDamageDelegate(MainManager.BattleData? attacker, ref MainManager.BattleData target, int damage, BattleControl.AttackProperty? property, bool block);
        public delegate int DoDamageOverridesDelegate(MainManager.BattleData? attacker, ref MainManager.BattleData target, int damage, BattleControl.AttackProperty? property, int[] overrides, bool block);

        public delegate void HealDelegate(ref MainManager.BattleData entity, int? amount, bool nosound);

        public int FindRelayable(EntityControl entity, BattleControl instance)
        {
            var enemies = new List<int>();
            for (int i = 0; i != instance.enemydata.Length; i++)
            {
                if (instance.enemydata[i].battleentity != entity && CanBeRelayed(instance.enemydata[i]))
                {
                    enemies.Add(i);
                    if (MainManager.HasCondition(MainManager.BattleCondition.AttackUp, instance.enemydata[i]) != -1 || instance.enemydata[i].charge > 0)
                    {
                        return i;
                    }
                }
            }
            return enemies[UnityEngine.Random.Range(0, enemies.Count)];
        }

        public bool CanRelay(EntityControl entity, BattleControl instance)
        {
            if (HasCondition((BattleCondition)NewCondition.Dizzy, instance.enemydata[entity.battleid]) > -1)
                return false;

            for (int i = 0; i != instance.enemydata.Length; i++)
            {
                if (instance.enemydata[i].battleentity != entity && CanBeRelayed(instance.enemydata[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CanBeRelayed(MainManager.BattleData enemy)
        {
            return enemy.hp > 0 && !enemy.lockrelayreceive
                && MainManager.HasCondition(MainManager.BattleCondition.Numb, enemy) == -1
                && MainManager.HasCondition(MainManager.BattleCondition.Sleep, enemy) == -1
                && MainManager.HasCondition(MainManager.BattleCondition.Freeze, enemy) == -1
                && MainManager.HasCondition(MainManager.BattleCondition.Taunted, enemy) == -1
                && MainManager.HasCondition(MainManager.BattleCondition.Sturdy, enemy) == -1;
        }

        public IEnumerator DoHardCharge(EntityControl entity, int animid, BattleControl instance, int maxCharge = 3, int hpReduction = 5)
        {
            battle.dontusecharge = true;
            MainManager.PlaySound("Charge7");
            entity.animstate = 24;
            StartCoroutine(entity.ShakeSprite(0.1f, 30f));
            yield return EventControl.halfsec;
            for (int i = 0; i < 3; i++)
            {
                battle.StartCoroutine(battle.StatEffect(entity, 4));
                MainManager.PlaySound("StatUp", -1, 0.9f + (float)(i + 1) * 0.1f, 1f);
                yield return EventControl.tenthsec;
                yield return EventControl.tenthsec;
            }

            if (entity.CompareTag("Player"))
            {
                MainManager.instance.playerdata[animid].charge = MainManager_Ext.CheckMaxCharge(animid);
            }
            else
            {
                instance.enemydata[animid].charge = maxCharge;

                instance.enemydata[animid].hp -= hpReduction;
            }
        }

        public IEnumerator UseItem(EntityControl entity, int targetID, BattleControl instance, MainManager.Items item, bool enemyOwnItem = false)
        {
            var entityExt = Entity_Ext.GetEntity_Ext(entity);

            if (enemyOwnItem)
            {
                enemyUsedItem = true;
                Destroy(entityExt.item);
                entityExt.itemId = -1;
            }

            if (battle.caller != null)
            {
                NPCControl_Ext.GetNPCControl_Ext(battle.caller).usedItem[actionID] = true;
            }

            battle.dontusecharge = true;
            entity.animstate = 4;
            MainManager.PlaySound("ItemHold");
            SpriteRenderer itemSprite = new GameObject().AddComponent<SpriteRenderer>();
            Vector3 offset = battle.enemydata[actionID].itemoffset == Vector3.zero ? new Vector3(0, 2.5f, -0.1f) : battle.enemydata[actionID].itemoffset;

            itemSprite.transform.position = entity.transform.position + offset + Vector3.up * entity.height;
            itemSprite.sprite = MainManager.itemsprites[0, (int)item];
            itemSprite.material.renderQueue = 50000;
            itemSprite.gameObject.layer = 14;
            yield return EventControl.sec;
            MainManager.ItemUse itemUse = MainManager.GetItemUse((int)item, 0);
            Destroy(itemSprite.gameObject);

            Instance.CheckMiniMegaMush((int)item, ref battle.enemydata[actionID]);

            for (int i = 0; i < itemUse.usetype.Length; i++)
            {
                int reviveID = -1;
                if (itemUse.usetype[i] == MainManager.ItemUsage.Revive && battle.reservedata.Count > 0)
                {
                    reviveID = UnityEngine.Random.Range(0, battle.reservedata.Count);
                }
                yield return DoItemEffect(itemUse.usetype[i], itemUse.values[i], targetID, reviveID, item);
            }
        }

        void ReviveEnemy(int id, int value)
        {
            if (battle.reservedata.Count != 0)
            {
                MainManager.BattleData target = battle.reservedata[id];
                battle.ReviveEnemy(id, value, true, true);
                MainManager.HealParticle(target.battleentity.transform, Vector3.one, Vector3.up);
            }
        }


        public delegate void AddBuffDelegate(ref MainManager.BattleData entity, MainManager.BattleCondition condition, int turns);
        public delegate void DoDamageDelegateNoAttackerProperty(ref MainManager.BattleData target, int damage, BattleControl.AttackProperty? property);

        void RemoveEnemyCondition(int enemyID, MainManager.BattleCondition condition)
        {
            MainManager.PlaySound("Heal3");
            MainManager.PlayParticle("MagicUp", null, MainManager.battle.enemydata[enemyID].battleentity.transform.position);
            MainManager.RemoveCondition(condition, MainManager.battle.enemydata[enemyID]);
        }

        public void AddEnemyBuff(int enemyID, MainManager.BattleCondition condition, int value, string sound, int statEffect)
        {
            if (sound != null)
            {
                MainManager.PlaySound(sound);
            }

            battle.AddBuff(ref battle.enemydata[enemyID], condition, value);

            if (statEffect != -1)
            {
                MainManager.PlaySound("StatUp");
                battle.StartCoroutine(battle.StatEffect(battle.enemydata[enemyID].battleentity, statEffect));
            }
        }

        public void CurePositiveStatus(ref MainManager.BattleData target)
        {
            int conditionAmount = target.condition.Count;
            int oldCharge = target.charge;
            target.charge = 0;
            MainManager.RemoveCondition(MainManager.BattleCondition.Reflection, target);
            MainManager.RemoveCondition(MainManager.BattleCondition.GradualHP, target);
            MainManager.RemoveCondition(MainManager.BattleCondition.GradualHP, target);
            MainManager.RemoveCondition(MainManager.BattleCondition.Shield, target);
            MainManager.RemoveCondition((BattleCondition)NewCondition.Vitiation, target);
            MainManager.RemoveCondition((BattleCondition)NewCondition.Slugskin, target);

            int amountCured = conditionAmount - target.condition.Count;
            if (oldCharge != 0)
                amountCured++;

            if (target.battleentity.playerentity && target.hp > 0 && amountCured > 0)
            {
                Instance.DoPurifyingPulseCheck(ref target, amountCured);
                Instance.DoRevitalizingRippleCheck(ref target, amountCured);
            }
        }
        public void CureNegativeStatus(ref MainManager.BattleData target)
        {
            int conditionAmount = target.condition.Count;

            MainManager.RemoveCondition(MainManager.BattleCondition.Poison, target);
            MainManager.RemoveCondition(MainManager.BattleCondition.Sleep, target);
            MainManager.RemoveCondition(MainManager.BattleCondition.Freeze, target);
            MainManager.RemoveCondition(MainManager.BattleCondition.Numb, target);
            MainManager.RemoveCondition(MainManager.BattleCondition.Fire, target);
            MainManager.RemoveCondition(MainManager.BattleCondition.Inked, target);
            MainManager.RemoveCondition(MainManager.BattleCondition.Sticky, target);
            MainManager.RemoveCondition((MainManager.BattleCondition)NewCondition.Dizzy, target);

            int amountCured = conditionAmount - target.condition.Count;
            if (target.battleentity.playerentity && target.hp > 0 && amountCured > 0)
            {
                Instance.DoPurifyingPulseCheck(ref target, amountCured);
                Instance.DoRevitalizingRippleCheck(ref target, amountCured);
            }

            if (target.battleentity.firepart != null)
            {
                Destroy(target.battleentity.firepart.gameObject);
            }
            target.battleentity.BreakIce();
            target.isasleep = false;
            target.isnumb = false;
        }

        void ReviveAllEnemies(int value)
        {
            if (battle.reservedata.Count > 0)
            {
                ReviveEnemy(0, value);

                if (battle.reservedata.Count > 0)
                    ReviveEnemy(0, value);
            }
        }

        public void RecoverEnemyTp(int value, int id)
        {
            MainManager.PlaySound("Heal2");
            battle.ShowDamageCounter(2,
                    value,
                    battle.enemydata[id].battleentity.transform.position + battle.enemydata[id].cursoroffset,
                    Vector3.up);
            GetEnemyTpCharge(value, id);
            battle.dontusecharge = true;
        }

        public void RecoverPlayerTP(int value, MainManager.BattleData target)
        {
            RecoverPlayerTP(value, target.battleentity.transform.position + target.cursoroffset + Vector3.up);
        }

        public void RecoverPlayerTP(int value, Vector3 position)
        {
            MainManager.instance.tp = Mathf.Clamp(MainManager.instance.tp + value, 0, MainManager.instance.maxtp);
            battle.ShowDamageCounter(2, value, position, Vector3.up);
            if (MainManager.SoundIsPlaying("Heal2") == -1)
                MainManager.PlaySound("Heal2");
        }

        public IEnumerator DoItemEffect(MainManager.ItemUsage type, int value, int? enemyID, int reviveID, MainManager.Items item)
        {
            bool wait = false;

            switch (type)
            {
                case MainManager.ItemUsage.Revive:
                    if (battle.reservedata.Count > 0 && reviveID != -1)
                    {
                        ReviveEnemy(reviveID, value);
                    }
                    else
                    {
                        battle.Heal(ref battle.enemydata[enemyID.Value], value, false);
                    }
                    wait = true;
                    break;


                case MainManager.ItemUsage.HPRecover:
                    battle.Heal(ref battle.enemydata[enemyID.Value], value, false);
                    wait = true;
                    break;

                case MainManager.ItemUsage.TPRecoverFull:
                case MainManager.ItemUsage.TPRecover:
                    RecoverEnemyTp(value, enemyID.Value);
                    wait = true;
                    break;

                case MainManager.ItemUsage.HPRecoverAll:
                    for (int i = 0; i < battle.enemydata.Length; i++)
                    {
                        battle.Heal(ref battle.enemydata[i], value, false);
                    }
                    wait = true;
                    break;

                case MainManager.ItemUsage.ReviveAll:
                    for (int i = 0; i < battle.enemydata.Length; i++)
                    {
                        battle.Heal(ref battle.enemydata[i], value, false);
                    }
                    ReviveAllEnemies(value);
                    wait = true;
                    break;

                case ItemUsage.Battle:
                    yield return ManageBattleItems(battle.enemydata[actionID].battleentity, battle, item, enemyID.Value);
                    break;

                case (MainManager.ItemUsage)NewItemUse.AddAtkDown:
                    battle.StatusEffect(battle.enemydata[enemyID.Value], BattleCondition.AttackDown, value, true, false);
                    wait = true;
                    break;

                case (MainManager.ItemUsage)NewItemUse.AddDefDown:
                    battle.StatusEffect(battle.enemydata[enemyID.Value], BattleCondition.DefenseDown, value, true, false);
                    wait = true;
                    break;

                case (MainManager.ItemUsage)NewItemUse.AddTaunt:
                    Entity_Ext ext = Entity_Ext.GetEntity_Ext(battle.enemydata[enemyID.Value].battleentity);
                    ext.tauntedBy = battle.GetRandomAvaliablePlayer();
                    AddEnemyBuff(enemyID.Value, MainManager.BattleCondition.Taunted, value, "Taunt", -1);
                    wait = true;
                    break;

                case MainManager.ItemUsage.Sturdy:
                case (MainManager.ItemUsage)NewItemUse.AddSturdy:
                    AddEnemyBuff(enemyID.Value, MainManager.BattleCondition.Sturdy, value, "MagicUp", -1);
                    wait = true;
                    break;

                case (MainManager.ItemUsage)NewItemUse.AddFire:
                    AddEnemyBuff(enemyID.Value, MainManager.BattleCondition.Fire, value, "Flame", -1);
                    wait = true;
                    break;

                case MainManager.ItemUsage.DefUpStat:
                    AddEnemyBuff(enemyID.Value, MainManager.BattleCondition.DefenseUp, value, null, 1);
                    wait = true;
                    break;

                case MainManager.ItemUsage.AtkUpStat:
                    AddEnemyBuff(enemyID.Value, MainManager.BattleCondition.AttackUp, value, null, 0);
                    wait = true;
                    break;

                case (MainManager.ItemUsage)NewItemUse.RandomBuff:
                    if (battle.enemydata[enemyID.Value].hp > 0 && battle.enemydata[enemyID.Value].position != BattlePosition.Underground)
                    {
                        MainManager_Ext.Instance.DoRandomMysteryBuff(enemyID.Value, value, false);
                    }
                    wait = true;
                    break;

                case (MainManager.ItemUsage)NewItemUse.RandomBuffParty:
                    for (int i = 0; i < battle.enemydata.Length; i++)
                    {
                        if (battle.enemydata[i].hp > 0 && battle.enemydata[i].position != BattlePosition.Underground)
                        {
                            MainManager_Ext.Instance.DoRandomMysteryBuff(i, value, false);
                        }
                    }
                    wait = true;
                    break;

                case (MainManager.ItemUsage)NewItemUse.RandomDebuff:
                    if (battle.enemydata[enemyID.Value].hp > 0 && battle.enemydata[enemyID.Value].position != BattlePosition.Underground)
                    {
                        MainManager_Ext.Instance.DoRandomMysteryDebuff(enemyID.Value, value, false);
                    }
                    wait = true;
                    break;


                case (MainManager.ItemUsage)NewItemUse.RandomDebuffParty:
                    for (int i = 0; i < battle.enemydata.Length; i++)
                    {
                        bool once = type == (MainManager.ItemUsage)NewItemUse.RandomDebuff;

                        if (once)
                            i = enemyID.Value;

                        if (battle.enemydata[i].hp > 0 && battle.enemydata[i].position != BattlePosition.Underground)
                        {
                            MainManager_Ext.Instance.DoRandomMysteryDebuff(i, value, false);
                        }
                        if (once)
                            break;
                    }
                    wait = true;
                    break;

                case MainManager.ItemUsage.CurePoison:
                    RemoveEnemyCondition(enemyID.Value, MainManager.BattleCondition.Poison);
                    wait = true;
                    break;

                case MainManager.ItemUsage.CureFreeze:
                    RemoveEnemyCondition(enemyID.Value, MainManager.BattleCondition.Freeze);
                    battle.enemydata[enemyID.Value].battleentity.BreakIce();
                    wait = true;
                    break;

                case MainManager.ItemUsage.CureNumb:
                    RemoveEnemyCondition(enemyID.Value, MainManager.BattleCondition.Numb);
                    battle.enemydata[enemyID.Value].isnumb = false;
                    wait = true;
                    break;

                case MainManager.ItemUsage.CureSleep:
                    RemoveEnemyCondition(enemyID.Value, MainManager.BattleCondition.Sleep);
                    battle.enemydata[enemyID.Value].isasleep = false;
                    wait = true;
                    break;

                case MainManager.ItemUsage.CureParty:
                    MainManager.PlaySound("Heal3");
                    for (int i = 0; i < battle.enemydata.Length; i++)
                    {
                        if (battle.enemydata[i].hp > 0)
                        {
                            MainManager.PlayParticle("MagicUp", null, battle.enemydata[i].battleentity.transform.position);
                            CureNegativeStatus(ref battle.enemydata[i]);
                        }
                    }
                    wait = true;
                    break;

                case MainManager.ItemUsage.AddPoison:
                    if (MainManager.HasCondition(MainManager.BattleCondition.Sturdy, battle.enemydata[enemyID.Value]) > -1 || (UnityEngine.Random.Range(0, 100) < battle.enemydata[enemyID.Value].poisonres))
                    {
                        break;
                    }
                    AddEnemyBuff(enemyID.Value, MainManager.BattleCondition.Poison, value, "Poison", -1);
                    break;

                case MainManager.ItemUsage.AddSleep:
                    if (MainManager.HasCondition(MainManager.BattleCondition.Sturdy, battle.enemydata[enemyID.Value]) > -1 || (UnityEngine.Random.Range(0, 100) < battle.enemydata[enemyID.Value].sleepres))
                    {
                        break;
                    }
                    AddEnemyBuff(enemyID.Value, MainManager.BattleCondition.Sleep, value, "Sleep", -1);
                    break;

                case MainManager.ItemUsage.AddNumb:

                    if (MainManager.HasCondition(MainManager.BattleCondition.Sturdy, battle.enemydata[enemyID.Value]) > -1 || (UnityEngine.Random.Range(0, 100) < battle.enemydata[enemyID.Value].numbres))
                    {
                        break;
                    }
                    AddEnemyBuff(enemyID.Value, MainManager.BattleCondition.Numb, value, "Shock", -1);
                    break;

                case MainManager.ItemUsage.AddFreeze:
                    if (MainManager.HasCondition(MainManager.BattleCondition.Sturdy, battle.enemydata[enemyID.Value]) > -1 || (UnityEngine.Random.Range(0, 100) < battle.enemydata[enemyID.Value].freezeres))
                    {
                        break;
                    }
                    AddEnemyBuff(enemyID.Value, MainManager.BattleCondition.Freeze, value, "Freeze", -1);
                    MainManager.PlayParticle("mothicenormal", null, battle.enemydata[enemyID.Value].battleentity.transform.position + Vector3.up + battle.enemydata[enemyID.Value].battleentity.height * Vector3.up).transform.localScale = Vector3.one * 1.5f;
                    if (MainManager.HasCondition(MainManager.BattleCondition.Freeze, battle.enemydata[enemyID.Value]) > -1 && (battle.enemydata[enemyID.Value].battleentity.icecube == null || !battle.enemydata[enemyID.Value].battleentity.icecube.activeInHierarchy))
                    {
                        battle.enemydata[enemyID.Value].battleentity.Freeze();
                    }
                    break;

                case MainManager.ItemUsage.HPto1:
                    MainManager.PlaySound("Damage0");
                    battle.enemydata[enemyID.Value].hp = 1;
                    wait = true;
                    break;

                case MainManager.ItemUsage.GradualHP:
                    AddEnemyBuff(enemyID.Value, MainManager.BattleCondition.GradualHP, value, "Heal3", -1);
                    MainManager.PlayParticle("MagicUp", null, battle.enemydata[enemyID.Value].battleentity.transform.position);
                    break;

                case MainManager.ItemUsage.GradualTP:
                    AddEnemyBuff(enemyID.Value, MainManager.BattleCondition.GradualTP, value, "Heal3", -1);
                    MainManager.PlayParticle("MagicUp", null, battle.enemydata[enemyID.Value].battleentity.transform.position);
                    wait = true;
                    break;

                case MainManager.ItemUsage.GradualHPParty:
                    MainManager.PlaySound("Heal3");
                    for (int i = 0; i < battle.enemydata.Length; i++)
                    {
                        if (battle.enemydata[i].hp > 0)
                        {
                            MainManager.PlayParticle("MagicUp", null, battle.enemydata[i].battleentity.transform.position);
                            AddEnemyBuff(i, MainManager.BattleCondition.GradualHP, value, null, -1);
                        }
                    }
                    wait = true;
                    break;


                case MainManager.ItemUsage.ChargeUp:
                    MainManager.PlaySound("StatUp");
                    battle.enemydata[enemyID.Value].charge = Mathf.Clamp(battle.enemydata[enemyID.Value].charge + value, 0, GetMaxEnemyCharge(battle.enemydata[enemyID.Value].battleentity));
                    battle.StartCoroutine(battle.StatEffect(battle.enemydata[enemyID.Value].battleentity, 4));
                    wait = true;
                    break;

                case MainManager.ItemUsage.AtkDownAfter:
                    battle.enemydata[enemyID.Value].atkdownonloseatkup = true;
                    break;

                case MainManager.ItemUsage.CureFire:
                    MainManager.PlaySound("Heal3");
                    RemoveEnemyCondition(enemyID.Value, MainManager.BattleCondition.Fire);
                    if (battle.enemydata[enemyID.Value].battleentity.firepart != null)
                    {
                        Destroy(battle.enemydata[enemyID.Value].battleentity.firepart.gameObject);
                    }
                    wait = true;
                    break;

                case MainManager.ItemUsage.CureAll:
                    MainManager.PlaySound("Heal3");
                    CureNegativeStatus(ref battle.enemydata[enemyID.Value]);
                    break;

                case MainManager.ItemUsage.TurnNextTurn:
                    MainManager.PlaySound("Heal3");
                    battle.enemydata[enemyID.Value].moreturnnextturn += 1;
                    battle.StartCoroutine(battle.StatEffect(battle.enemydata[enemyID.Value].battleentity, 5));
                    break;

                case MainManager.ItemUsage.HPorDamage:
                    if (UnityEngine.Random.Range(0, 100) > 33)
                    {
                        battle.Heal(ref battle.enemydata[enemyID.Value], value, false);
                    }
                    else
                    {
                        battle.DoDamage(ref battle.enemydata[enemyID.Value], value, BattleControl.AttackProperty.NoExceptions);
                    }
                    battle.enemydata[enemyID.Value].hp = Mathf.Clamp(battle.enemydata[enemyID.Value].hp, 1, battle.enemydata[enemyID.Value].maxhp);
                    break;

                case MainManager.ItemUsage.CurePoisonAll:
                    MainManager.PlaySound("Heal3");
                    for (int i = 0; i < battle.enemydata.Length; i++)
                    {
                        if (battle.enemydata[i].hp > 0)
                        {
                            RemoveEnemyCondition(i, MainManager.BattleCondition.Poison);
                        }
                    }
                    break;
                case (ItemUsage)NewItemUse.AddInk:
                    AddEnemyBuff(enemyID.Value, MainManager.BattleCondition.Inked, value, "WaterSplash2", -1);
                    break;
                case (ItemUsage)NewItemUse.AddSticky:
                    AddEnemyBuff(enemyID.Value, MainManager.BattleCondition.Sticky, value, "WaterSplash2", -1);
                    break;
                case (ItemUsage)NewItemUse.AddInkParty:
                    MainManager.PlaySound("WaterSplash2");
                    for (int i = 0; i < battle.enemydata.Length; i++)
                    {
                        if (battle.enemydata[i].hp > 0)
                        {
                            AddEnemyBuff(i, MainManager.BattleCondition.Inked, value, null, -1);
                        }
                    }
                    break;
                case (ItemUsage)NewItemUse.AddStickyParty:
                    MainManager.PlaySound("WaterSplash2");
                    for (int i = 0; i < battle.enemydata.Length; i++)
                    {
                        if (battle.enemydata[i].hp > 0)
                        {
                            AddEnemyBuff(i, MainManager.BattleCondition.Sticky, value, null, -1);
                        }
                    }
                    break;

                case (ItemUsage)NewItemUse.ChargeMax:
                    MainManager.PlaySound("StatUp");
                    battle.enemydata[enemyID.Value].charge = GetMaxEnemyCharge(battle.enemydata[enemyID.Value].battleentity);
                    battle.StartCoroutine(battle.StatEffect(battle.enemydata[enemyID.Value].battleentity, 4));
                    wait = true;
                    break;

                case (MainManager.ItemUsage)NewItemUse.AddSlugskin:
                    MainManager.PlaySound("Shield", 1.4f, 1);
                    AddEnemyBuff(enemyID.Value, (MainManager.BattleCondition)NewCondition.Slugskin, value, null, -1);
                    wait = true;
                    break;

                case (MainManager.ItemUsage)NewItemUse.AtkUpParty:
                    MainManager.PlaySound("StatUp");
                    for (int i = 0; i < battle.enemydata.Length; i++)
                    {
                        if (battle.enemydata[i].hp > 0)
                        {
                            AddEnemyBuff(i, MainManager.BattleCondition.AttackUp, value, null, 0);
                        }
                    }
                    break;

                case (MainManager.ItemUsage)NewItemUse.DefUpParty:
                    MainManager.PlaySound("StatUp");
                    for (int i = 0; i < battle.enemydata.Length; i++)
                    {
                        if (battle.enemydata[i].hp > 0)
                        {
                            AddEnemyBuff(i, MainManager.BattleCondition.DefenseUp, value, null, 0);
                        }
                    }
                    break;

                case (MainManager.ItemUsage)NewItemUse.GradualTPParty:
                    MainManager.PlaySound("Heal3");
                    for (int i = 0; i < battle.enemydata.Length; i++)
                    {
                        if (battle.enemydata[i].hp > 0)
                        {
                            MainManager.PlayParticle("MagicUp", null, battle.enemydata[i].battleentity.transform.position);
                            AddEnemyBuff(i, MainManager.BattleCondition.GradualTP, value, null, -1);
                        }
                    }
                    wait = true;
                    break;

                case (MainManager.ItemUsage)NewItemUse.AddVitiation:
                    MainManager.PlaySound("Shield");
                    AddEnemyBuff(enemyID.Value, (MainManager.BattleCondition)NewCondition.Vitiation, value, null, -1);
                    wait = true;
                    break;

                case (MainManager.ItemUsage)NewItemUse.AddDizzy:
                    TryDizzy(null, ref battle.enemydata[enemyID.Value], value);
                    wait = true;
                    break;

                case (MainManager.ItemUsage)NewItemUse.AddDizzyParty:
                    for (int i = 0; i < battle.enemydata.Length; i++)
                    {
                        if (battle.enemydata[i].hp > 0)
                        {
                            TryDizzy(null, ref battle.enemydata[i], value);
                        }
                    }
                    wait = true;
                    break;

                case (MainManager.ItemUsage)NewItemUse.DizzyAfter:
                    Entity_Ext.GetEntity_Ext(battle.enemydata[enemyID.Value].battleentity).extraData.dizzyAfter.Add(value);
                    break;
            }

            if (wait)
            {
                yield return EventControl.thirdsec;
            }
        }

        public int GetLowHPEnemy()
        {
            for (int j = 0; j < battle.enemydata.Length; j++)
            {
                float hpPercent = battle.HPPercent(battle.enemydata[j]);
                if (hpPercent <= 0.7f)
                {
                    return j;
                }
            }
            return -1;
        }

        static IEnumerator CheckUseItem()
        {
            var entityExt = Entity_Ext.GetEntity_Ext(MainManager.battle.enemydata[actionID].battleentity);
            int target = -1;
            int itemUseOdds = 35;
            if (entityExt.itemId != -1)
            {
                MainManager.ItemUse itemUse = MainManager.GetItemUse(entityExt.itemId, 0);
                for (int i = 0; i < itemUse.usetype.Length; i++)
                {
                    switch (itemUse.usetype[i])
                    {
                        case ItemUsage.Revive:
                        case ItemUsage.ReviveAll:
                            if (battle.reservedata.Count > 0)
                            {
                                yield return BattleControl_Ext.Instance.UseItem(battle.enemydata[actionID].battleentity, -1, battle, (MainManager.Items)entityExt.itemId, true);
                                yield break;
                            }
                            else
                            {
                                target = BattleControl_Ext.Instance.GetLowHPEnemy();
                                if (target != -1)
                                {
                                    yield return BattleControl_Ext.Instance.UseItem(battle.enemydata[actionID].battleentity, target, battle, (MainManager.Items)entityExt.itemId, true);
                                    yield break;
                                }
                            }
                            break;

                        case ItemUsage.HPRecover:
                        case ItemUsage.HPRecoverAll:
                        case ItemUsage.HPRecoverFull:
                        case ItemUsage.HPorDamage:
                            target = BattleControl_Ext.Instance.GetLowHPEnemy();
                            if (target != -1)
                            {
                                yield return BattleControl_Ext.Instance.UseItem(battle.enemydata[actionID].battleentity, target, battle, (MainManager.Items)entityExt.itemId, true);
                                yield break;
                            }
                            break;

                        case ItemUsage.AtkUpStat:
                        case ItemUsage.DefUpStat:
                        case ItemUsage.GradualHP:
                        case ItemUsage.GradualHPParty:
                        case ItemUsage.TurnNextTurn:
                        case ItemUsage.ChargeUp:
                            target = UnityEngine.Random.Range(0, battle.enemydata.Length);
                            yield return BattleControl_Ext.Instance.UseItem(battle.enemydata[actionID].battleentity, target, battle, (MainManager.Items)entityExt.itemId, true);
                            yield break;

                        case ItemUsage.Battle:
                            itemUseOdds = 70;
                            break;
                    }
                }
                if (UnityEngine.Random.Range(0, 100) < itemUseOdds)
                {
                    target = UnityEngine.Random.Range(0, battle.enemydata.Length);
                    yield return BattleControl_Ext.Instance.UseItem(battle.enemydata[actionID].battleentity, target, battle, (MainManager.Items)entityExt.itemId, true);
                    yield break;
                }
            }
        }

        IEnumerator ManageBattleItems(EntityControl entity, BattleControl instance, MainManager.Items itemID, int targetId = -1)
        {
            battle.nonphyscal = true;
            int target = -1;

            switch (itemID)
            {
                case Items.LonglegSummoner:
                    battle.GetSingleTarget();
                    yield return battle.LongLeg(entity, MainManager.instance.playerdata[battle.playertargetID]);
                    break;

                case (Items)NewItem.PointSwap:
                    yield return DoPointSwap(entity, true, targetId);
                    break;

                case (Items)NewItem.WhirlySeed:
                    battle.GetSingleTarget();
                    yield return DoWhirlySeed(entity, true);
                    break;

                case (Items)NewItem.WhirlyBomb:
                    yield return DoWhirlyBomb(entity, true);
                    break;

                case (Items)NewItem.WhirlaRang:
                    battle.GetSingleTarget();
                    yield return DoWhirlaRang(entity, true);
                    break;

                case (Items)NewItem.InkyBrew:
                case (Items)NewItem.BubbleHoney:
                    battle.GetSingleTarget();
                    yield return DoInkBubbleItem(entity, true, (NewItem)itemID);
                    break;

                case (Items)NewItem.Spiroll:
                    yield return DoSpiroll(entity, true, targetId);
                    break;

                case (Items)NewItem.FlameBomb:
                    yield return DoFlameBomb(entity, true);
                    break;

                case (Items)NewItem.SeedlingWhistle:
                    yield return DoSeedlingStampede(true);
                    break;

                case (Items)NewItem.InkTrap:
                    battle.GetSingleTarget();
                    yield return DoInkTrap(true, entity, battle.enemydata[actionID]);
                    break;

                case (Items)NewItem.StickyBomb:
                    yield return DoStickyBomb(true, entity, battle.enemydata[actionID]);
                    break;

                case Items.HardSeed:
                case Items.PoisonDart:
                case Items.NumbDart:
                case (Items)NewItem.BeeBattery:
                case Items.Ice:
                case (Items)NewItem.WebWad:
                case Items.FlameRock:
                case (Items)NewItem.MysterySeed:
                case (Items)NewItem.SucculentSeed:
                case (Items)NewItem.SquashSeed:
                    battle.GetSingleTarget();
                    target = battle.playertargetID;

                    int damage = 2;
                    int piercedStatusRes = 0;
                    AttackProperty? property = null;
                    List<DamageOverride> overrides = new List<DamageOverride>() { (DamageOverride)NewDamageOverride.Pierce1 };
                    bool spin = true;
                    bool curve = true;
                    switch (itemID)
                    {
                        case Items.PoisonDart:
                            piercedStatusRes = 40;
                            property = AttackProperty.Poison;
                            curve = false;
                            spin = false;
                            break;

                        case Items.NumbDart:
                            piercedStatusRes = 40;
                            property = AttackProperty.Sleep;
                            spin = false;
                            curve = false;
                            break;

                        case (Items)NewItem.BeeBattery:
                            piercedStatusRes = 40;
                            property = AttackProperty.Numb;
                            break;

                        case Items.Ice:
                            piercedStatusRes = 40;
                            property = AttackProperty.Freeze;
                            break;

                        case (Items)NewItem.WebWad:
                            damage = 1;
                            overrides.Add((DamageOverride)NewDamageOverride.Pierce1);
                            SetCondition(BattleCondition.Sticky, ref battle.enemydata[actionID], 2);
                            PlayParticle("StickyGet", entity.transform.position + Vector3.up);
                            PlaySound("AhoneynationSpit", -1, 0.8f, 1f);
                            break;

                        case Items.FlameRock:
                            property = AttackProperty.Fire;
                            break;

                        case (Items)NewItem.MysterySeed:
                            damage = 4;
                            piercedStatusRes = 100;
                            break;

                        case (Items)NewItem.SquashSeed:
                        case (Items)NewItem.SucculentSeed:
                            damage = 4;
                            break;
                    }

                    yield return ThrowItem(entity, battle, itemID, AttackArea.SingleEnemy, curve, spin);
                    
                    Entity_Ext extEnt = Entity_Ext.GetEntity_Ext(MainManager.instance.playerdata[target].battleentity);
                    extEnt.piercedStatusRes += piercedStatusRes;

                    int damageDealt = battle.DoDamage(null, ref MainManager.instance.playerdata[target], damage, property, overrides.ToArray(), battle.commandsuccess);
                    
                    if (itemID == (Items)NewItem.MysterySeed)
                    {
                        MainManager_Ext.Instance.DoRandomMysteryBuff(target, 4, true);
                        MainManager_Ext.Instance.DoRandomMysteryDebuff(target, 4, true);
                    }
                    extEnt.piercedStatusRes -= piercedStatusRes;

                    if (damageDealt > 0)
                    {
                        if (itemID == (Items)NewItem.SucculentSeed)
                            battle.Heal(ref battle.enemydata[actionID], damageDealt);

                        if (itemID == (Items)NewItem.SquashSeed)
                            RecoverEnemyTp(damageDealt, actionID);
                    }

                    if (itemID == (Items)NewItem.WebWad)
                    {
                        if (MainManager.instance.playerdata[target].hp > 0)
                        {
                            SetCondition(BattleCondition.Sticky, ref MainManager.instance.playerdata[target], 4);
                            PlayParticle("StickyGet", MainManager.instance.playerdata[target].battleentity.transform.position + Vector3.up);
                        }
                        PlaySound("AhoneynationSpit", -1, 0.8f, 1f);
                    }

                    yield return EventControl.quartersec;
                    break;

                case Items.SpicyBomb:
                case Items.BurlyBomb:
                case Items.PoisonBomb:
                case Items.SleepBomb:
                case Items.NumbBomb:
                case Items.FrostBomb:
                case Items.CherryBomb:
                case (Items)NewItem.CherryBomb2:
                    damage = 5;
                    property = AttackProperty.None;
                    overrides = null;
                    string particle = null;
                    float shakeAmount = 0.2f;
                    Vector3 particleScale = Vector3.one;
                    Color? particleColor = null;
                    switch (itemID)
                    {
                        case Items.SpicyBomb:
                            property = AttackProperty.AtkDownOnBlock;
                            particle = "explosion";
                            particleColor = new Color(0.7f, 0.6f, 0.6f);
                            break;

                        case Items.BurlyBomb:
                            damage--;
                            property = AttackProperty.DefDownOnBlock;
                            particle = "explosion";
                            particleColor = new Color(0.6f, 0.6f, 0.7f);
                            break;

                        case Items.PoisonBomb:
                            property = AttackProperty.Poison;
                            particle = "PoisonEffect";
                            particleScale *= 2f;
                            break;

                        case Items.SleepBomb:
                            property = AttackProperty.Sleep;
                            overrides = new List<DamageOverride>() { DamageOverride.DontAwake };
                            shakeAmount = 0.1f;
                            particleScale *= 3f;
                            break;

                        case Items.NumbBomb:
                            property = AttackProperty.Numb1Turn;
                            particle = "ElecFast";
                            shakeAmount = 0.1f;
                            particleScale *= 2f;
                            break;

                        case Items.FrostBomb:
                            property = AttackProperty.Freeze;
                            overrides = new List<DamageOverride>() { (DamageOverride)NewDamageOverride.Magic };
                            particle = "mothicenormal";
                            shakeAmount = 0.1f;
                            particleScale *= 3f;
                            break;

                        case (Items)NewItem.CherryBomb2:
                        case Items.CherryBomb:
                            itemID = (Items)NewItem.CherryBomb2;
                            damage += 2;
                            overrides = new List<DamageOverride>() { // hits all weaknesses
                                (DamageOverride)NewDamageOverride.Beemerang,
                                (DamageOverride)NewDamageOverride.FlipNoPierce,
                                (DamageOverride)NewDamageOverride.FlipNoPierce,
                                (DamageOverride)NewDamageOverride.Magic,
                                (DamageOverride)NewDamageOverride.ExtraAirTopple,
                            };
                            particle = "PoisonEffect";
                            shakeAmount = 0.3f;
                            particleScale *= 3f;
                            particleColor = new Color(156 / 306f, 113 / 360f, 173 / 360f);
                            break;
                    }

                    yield return CheckBomb(itemID, entity, damage, property, overrides?.ToArray(), particle, shakeAmount, particleScale, particleColor);
                    break;

                case (Items)NewItem.InkBomb:
                    yield return CheckBomb(itemID, entity, 5, AttackProperty.Pierce, null, "InkGet", 0.2f, Vector3.one * 2, null);
                    if (!battle.commandsuccess)
                    {
                        for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                        {
                            BattleData playerdata = MainManager.instance.playerdata[i];
                            if (playerdata.hp <= 0 || HasCondition(BattleCondition.Sturdy, playerdata) > -1)
                            {
                                continue;
                            }
                            piercedStatusRes = 100 - 50 * BadgeHowManyEquipped((int)BadgeTypes.ResistAll, playerdata.trueid);
                            if (UnityEngine.Random.value < piercedStatusRes / 100f)
                            {
                                SetCondition(BattleCondition.Inked, ref MainManager.instance.playerdata[i], 4);
                            }
                        }
                    }
                    for (int i = 0; i < battle.enemydata.Length; i++)
                    {
                        if (battle.enemydata[i].hp > 0 && battle.enemydata[i].position != BattlePosition.Underground && battle.enemydata[i].position != BattlePosition.OutOfReach)
                        {
                            SetCondition(BattleCondition.Inked, ref battle.enemydata[i], 2);
                        }
                    }
                    var ps = PlayParticle("impactsmoke", "BubbleBurst", Vector3.one).GetComponent<ParticleSystem>();
                    var main = ps.main;
                    main.startColor = new Color(0.22f, 0f, 0.5f, 1f);
                    break;

                case (Items)NewItem.MysteryBomb:
                    yield return CheckBomb(itemID, entity, 5, AttackProperty.Flip, null, "explosion", 0.2f, Vector3.one, null);
                    piercedStatusRes = 50;
                    for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                    {
                        if (MainManager.instance.playerdata[i].hp > 0)
                        {
                            extEnt = Entity_Ext.GetEntity_Ext(MainManager.instance.playerdata[i].battleentity);
                            extEnt.piercedStatusRes += piercedStatusRes;
                            MainManager_Ext.Instance.DoRandomMysteryBuff(i, 2, true);
                            MainManager_Ext.Instance.DoRandomMysteryDebuff(i, 2, true);
                            extEnt.piercedStatusRes -= piercedStatusRes;
                        }
                    }
                    break;

                case Items.Abombhoney:
                    yield return ThrowItem(entity, battle, itemID, BattleControl.AttackArea.All, true, true);
                    yield return DoAbomb(new Vector3(0f, 0f, -0.5f), battle);
                    break;

                case Items.ClearBomb:
                    yield return ThrowItem(entity, battle, itemID, BattleControl.AttackArea.All, true, true);
                    yield return battle.ClearBombEffect();
                    break;


                case Items.GenerousSeed:
                case Items.VitalitySeed:

                    BattleCondition condition = BattleCondition.AttackUp;
                    int statEffect = 0;

                    if (itemID == Items.GenerousSeed)
                    {
                        statEffect = 1;
                        condition = BattleCondition.DefenseUp;
                    }

                    int targetID = UnityEngine.Random.Range(0, battle.enemydata.Length);
                    AddEnemyBuff(targetID, condition, 2, null, statEffect);
                    yield return EventControl.thirdsec;
                    break;
            }
        }

        void DoMysteryEffect(ref MainManager.BattleData target, int turns)
        {
            if (target.hp > 0)
            {
                BattleCondition status = UnityEngine.Random.Range(0, 2) == 0 ? BattleCondition.AttackDown : BattleCondition.AttackUp;
                MainManager.SetCondition(status, ref target, turns);
                if (status == BattleCondition.AttackDown)
                {
                    StartCoroutine(battle.StatEffect(target.battleentity, 2));
                    MainManager.PlaySound("StatDown");
                }
                else
                {
                    StartCoroutine(battle.StatEffect(target.battleentity, 0));
                    MainManager.PlaySound("StatUp");
                }
            }
        }

        IEnumerator CheckBomb(MainManager.Items bomb, EntityControl entity, int damage, AttackProperty? property, 
            DamageOverride[] overrides, string particleName, float shakeAmount, Vector3 particleScale, Color? particleColor)
        {
            yield return ThrowItem(entity, battle, bomb, BattleControl.AttackArea.AllEnemies, true, true);

            MainManager.ShakeScreen(shakeAmount, 0.75f, true);
            if (particleName != null)
            {
                GameObject obj = PlayParticle(particleName, battle.partymiddle);
                obj.transform.localScale = particleScale;
                if (particleColor != null)
                {
                    ParticleSystem ps = obj.GetComponent<ParticleSystem>();
                    if (ps != null)
                    {
                        var main = ps.main;
                        main.startColor = particleColor.Value;
                    }
                }
            }
            if (bomb == Items.SleepBomb)
            {
                MainManager.DeathSmoke(battle.partymiddle, Vector3.one * 3f);
            }

            if (bomb == Items.NumbBomb)
            {
                MainManager.PlayParticle("impactsmoke", battle.partymiddle);
            }
            battle.PartyDamage(actionID, damage, property, battle.commandsuccess, 0f, Vector3.zero, false, overrides);
        }

        IEnumerator ThrowItem(EntityControl entity, BattleControl battle, MainManager.Items item, BattleControl.AttackArea area, bool curve, bool spin, float speed = 40f)
        {
            yield return EventControl.tenthsec;
            entity.animstate = 28;
            MainManager.PlaySound("Toss");
            yield return EventControl.tenthsec;

            Vector3 itemPos;
            bool usedByEnemy = battle.enemy;

            if (usedByEnemy)
            {
                itemPos = entity.transform.position + battle.enemydata[actionID].itemoffset + Vector3.up * entity.height;
            }
            else
            {
                itemPos = entity.transform.position + MainManager.instance.playerdata[battle.currentturn].cursoroffset - Vector3.up;
            }

            SpriteRenderer itemSprite = MainManager.NewSpriteObject(itemPos, null, MainManager.itemsprites[0, (int)item]);

            if (item == MainManager.Items.NumbDart || item == MainManager.Items.PoisonDart)
            {
                itemSprite.transform.localEulerAngles = new Vector3(0f, 0f, -15f);
            }

            Vector3 startPos = itemSprite.transform.position;
            Vector3 endPos = Vector3.zero;


            if (area == BattleControl.AttackArea.AllEnemies)
            {
                endPos = usedByEnemy ? battle.partymiddle : Vector3.right * 2;
            }
            else if (area == BattleControl.AttackArea.SingleEnemy)
            {
                if (usedByEnemy)
                {
                    var targetEntity = MainManager.instance.playerdata[battle.playertargetID].battleentity;
                    endPos = targetEntity.transform.position + Vector3.up;
                }
                else
                {
                    int target = MainManager.battle.avaliabletargets[battle.target].battleentity.battleid;
                    var targetEntity = battle.enemydata[target].battleentity;
                    endPos = targetEntity.transform.position + battle.enemydata[target].cursoroffset + new Vector3(0f, targetEntity.height - 1f);
                }
            }

            if (item == Items.ClearBomb || (NewItem)item == NewItem.InkBomb)
            {
                endPos = Vector3.zero;
            }

            if (item == Items.Abombhoney)
            {
                endPos = new Vector3(0f, 0f, -0.5f);
            }

            float a = 0f;
            float b = speed;
            do
            {
                if (curve)
                {
                    itemSprite.transform.position = MainManager.BeizierCurve3(startPos, endPos, 5, a / b);
                }
                else
                {
                    itemSprite.transform.position = Vector3.Lerp(startPos, endPos, a / b);
                }

                if (spin)
                {
                    itemSprite.transform.eulerAngles += new Vector3(0f, 0f, -MainManager.framestep * 20f);
                }
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < b);
            Destroy(itemSprite.gameObject);
        }

        IEnumerator DoAbomb(Vector3 position, BattleControl battle)
        {
            MainManager.PlaySound("Splat1");
            MainManager.PlaySound("Fuse");
            GameObject part = Instantiate(Resources.Load("Prefabs/Objects/Abombnation"), position, Quaternion.identity) as GameObject;

            yield return new WaitForSeconds(2f);
            MainManager.PlayParticle("explosion", part.transform.position);
            MainManager.PlaySound("Explosion3");
            MainManager.PlaySound("Splat2");
            Destroy(part.gameObject);

            MainManager.ShakeScreen(0.25f, 0.75f);
            for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
            {
                if (MainManager.instance.playerdata[i].hp > 0 && MainManager.instance.playerdata[i].eatenby == null)
                {
                    if (MainManager.HasCondition(MainManager.BattleCondition.Shield, MainManager.instance.playerdata[i]) > -1)
                    {
                        MainManager.RemoveCondition(MainManager.BattleCondition.Shield, MainManager.instance.playerdata[i]);
                        MainManager.instance.playerdata[i].battleentity.shieldenabled = false;
                    }
                    else
                    {
                        if (!MainManager.instance.playerdata[i].plating)
                        {
                            battle.DoDamage(null, ref MainManager.instance.playerdata[i], 10, BattleControl.AttackProperty.NoExceptions, null, false);
                        }
                        else
                        {
                            MainManager.instance.playerdata[i].plating = false;
                        }
                    }
                }
            }
            for (int i = 0; i < battle.enemydata.Length; i++)
            {
                if (MainManager.HasCondition(MainManager.BattleCondition.Shield, battle.enemydata[i]) <= -1)
                {
                    if (battle.enemydata[i].animid == (int)MainManager.Enemies.Abomihoney || battle.enemydata[i].animid == (int)MainManager.Enemies.Ahoneynation)
                    {
                        battle.Heal(ref battle.enemydata[i], 10, false);
                    }
                    else
                    {
                        battle.DoDamage(null, ref battle.enemydata[i], 10, BattleControl.AttackProperty.NoExceptions, null, false);
                    }
                }
                else
                {
                    MainManager.RemoveCondition(MainManager.BattleCondition.Shield, battle.enemydata[i]);
                    battle.enemydata[i].battleentity.shieldenabled = false;
                }
            }

            SpriteRenderer[] splatters = new SpriteRenderer[3];
            Sprite splatterSprite = Resources.Load<Sprite>("Sprites/Objects/splatter");
            Vector3[] splattersSize = new Vector3[]
            {
                new Vector3(1.75f, 2f, 1f),
                Vector3.one * 2f,
                new Vector3(1.25f, 1.5f, 1f)
            };
            Vector3[] splattersPos = new Vector3[]
            {
                new Vector3(-4.73f, 1.22f, 10f),
                new Vector3(0f, 0f, 10f),
                new Vector3(5.65f, 0.15f, 10f)
            };
            for (int i = 0; i < splatters.Length; i++)
            {
                splatters[i] = MainManager.NewUIObject("splat" + i, MainManager.GUICamera.transform, splattersPos[i], splattersSize[i], splatterSprite, -5 - i).GetComponent<SpriteRenderer>();
                splatters[i].flipX = i == 1;
                DialogueAnim dialogueAnim = splatters[i].gameObject.AddComponent<DialogueAnim>();
                dialogueAnim.targetscale = splatters[i].transform.localScale;
                dialogueAnim.transform.localScale = Vector3.zero;
                dialogueAnim.shrinkspeed = 0.15f;
                splatters[i].color = new Color(1f, 0.75f, 0f);
                yield return new WaitForSeconds(0.025f);
            }

            float a = 0f;
            float b = 100f;
            do
            {
                if (a >= 50f)
                {
                    for (int i = 0; i < splatters.Length; i++)
                    {
                        splatters[i].color = new Color(splatters[i].color.r, splatters[i].color.g, splatters[i].color.b, Mathf.Lerp(1f, 0f, (a - 50f) / 50f));
                    }
                }
                a += MainManager.framestep;
                yield return null;
            } while (a < b);

            for (int i = 0; i < splatters.Length; i++)
            {
                Destroy(splatters[i].gameObject);
            }
            if(battle.enemy)
                FixEnemyDiedOnItemUse();
        }

        void CheckEnemyItems()
        {
            var usableItems = new List<MainManager.Items>();

            usableItems.AddRange((Items[])Enum.GetValues(typeof(MainManager.Items)));
            usableItems.AddRange((Items[])Enum.GetValues(typeof(NewItem)));
            usableItems.Remove(Items.MoneySmall);
            usableItems.Remove(Items.MoneyMedium);
            usableItems.Remove(Items.None);

            for (int i = 0; i < usableItems.Count; i++)
            {
                switch (MainManager.GetItemUse((int)usableItems[i]).usetype[0])
                {
                    case ItemUsage.TPUP:
                    case ItemUsage.AttackUp:
                    case ItemUsage.HPUP:
                    case ItemUsage.HPUPAll:
                    case ItemUsage.MPUP:
                    case ItemUsage.DefenseUp:
                    case ItemUsage.None:
                        usableItems.Remove(usableItems[i]);
                        i--;
                        break;
                }
            }

            if (MainManager.battle.caller != null)
            {
                bool isOutdoorMap = MainManager_Ext.IsOutdoorMap(MainManager.map);
                var npcExt = NPCControl_Ext.GetNPCControl_Ext(MainManager.battle.caller);
                for (int i = 0; i < MainManager.battle.enemydata.Length; i++)
                {
                    int enemyID = MainManager.battle.enemydata[i].animid;
                    if (EnemyItemData.enemyData[enemyID] != null && !MainManager.instance.inevent)
                    {
                        var entityExt = Entity_Ext.GetEntity_Ext(MainManager.battle.enemydata[i].battleentity);
                        if (npcExt.items[i] != -1)
                        {
                            entityExt.CreateItem(npcExt.items[i]);
                            continue;
                        }

                        if (!npcExt.rolledItem)
                        {
                            if (MainManager_Ext.Instance.GetWhirlySeedChance() && isOutdoorMap)
                            {
                                int itemID = (int)NewItem.WhirlySeed;
                                entityExt.CreateItem(itemID);
                                npcExt.items[i] = itemID;
                            }
                            else if (UnityEngine.Random.Range(0, 100) < 20 || MainManager.instance.flags[(int)NewCode.SCAVENGE])
                            {
                                int itemID;
                                if (MainManager.instance.flags[(int)NewCode.SCAVENGE])
                                {
                                    itemID = (int)usableItems[UnityEngine.Random.Range(0, usableItems.Count)];
                                }
                                else
                                {
                                    MainManager.Items[] items = EnemyItemData.enemyData[enemyID];
                                    itemID = (int)items[UnityEngine.Random.Range(0, items.Length)];
                                }

                                if (UnityEngine.Random.Range(0, 100) == 0 && MainManager_Ext.IsOutdoorMap(MainManager.map))
                                {
                                    itemID = (int)NewItem.WhirlySeed;
                                }

                                entityExt.CreateItem(itemID);
                                npcExt.items[i] = itemID;
                            }
                        }
                    }
                }
                npcExt.rolledItem = true;
            }
        }

        static IEnumerator DoPebbleToss()
        {
            if (!IsUsingItem())
            {
                var entity = MainManager.instance.playerdata[battle.currentturn].battleentity;

                battle.GetAvaliableTargets(false, false, actionID, true);
                var targetEntity = battle.avaliabletargets[battle.target];

                battle.dontusecharge = true;
                yield return EventControl.tenthsec;
                entity.animstate = 28;
                MainManager.PlaySound("Toss");
                yield return EventControl.tenthsec;

                SpriteRenderer pebble = new GameObject().AddComponent<SpriteRenderer>();
                Vector3 startPos = entity.transform.position + MainManager.instance.playerdata[battle.currentturn].cursoroffset - Vector3.up;
                Vector3 endPos = targetEntity.battleentity.transform.position + targetEntity.cursoroffset + new Vector3(0f, targetEntity.battleentity.height - 1f);

                List<GameObject> icecles = new List<GameObject>();

                bool bounce = false;

                bool tanjy = false;
                if (MainManager.BadgeIsEquipped((int)Medal.TanjyToss))
                {
                    tanjy = true;
                }

                if (tanjy || (MainManager.instance.flags[275] && UnityEngine.Random.Range(0, 128) == 0))
                {
                    pebble.sprite = Resources.LoadAll<Sprite>("Sprites/Entities/moth0")[(!MainManager.instance.flags[555]) ? 93 : ((!MainManager.instance.flags[682] || UnityEngine.Random.Range(0, 4) > 1) ? 92 : 91)];
                    pebble.transform.localScale = Vector3.one * 0.9f;
                    pebble.flipX = true;
                    bounce = true;
                }
                else
                {
                    pebble.sprite = MainManager.itemsprites[1, 13];
                }

                float startTime = 0f;
                float height = 5f + targetEntity.battleentity.height;
                float endTime = 40;

                bool skippingStone = BadgeIsEquipped((int)Medal.SkippingStone);
                bool failedStylish = false;
                bool succeedStylish = false;
                int stylishHits = 0;
                while (startTime < endTime)
                {
                    pebble.transform.eulerAngles += new Vector3(0f, 0f, -MainManager.framestep * 20f);
                    pebble.transform.position = MainManager.BeizierCurve3(startPos, endPos, height, startTime / endTime);

                    if (!failedStylish && !succeedStylish)
                    {
                        if (StylishUtils.CheckStylish(ref failedStylish, entity, startTime, 25f))
                        {
                            succeedStylish = true;
                            battle.StartCoroutine(KabbuStylish.DoPebbleTossStylish(entity, stylishHits, gain: skippingStone ? 0.08f : 0.1f));
                            stylishHits++;
                        }
                    }

                    startTime += MainManager.TieFramerate(1f);
                    yield return null;
                }

                entity.Emoticon(MainManager.Emoticons.None);

                int damage = tanjy ? 2 : 1;
                damage += Instance.rockyRampUpDmg;
                int enemyID = MainManager.battle.GetEnemyID(targetEntity.battleentity.transform);
                battle.DoDamage(ref battle.enemydata[enemyID], damage, new BattleControl.AttackProperty?(BattleControl.AttackProperty.NoExceptions));

                Instance.CheckAvalanche(battle, enemyID, icecles);
                Instance.CheckGrumbleGravel(enemyID, battle);

                if (skippingStone)
                {
                    MainManager.PlayParticle(NewParticle.Ripples.ToString(), new Vector3(pebble.transform.position.x, pebble.transform.position.y + 0.5f, pebble.transform.position.z));
                    for (int i = enemyID + 1; i < battle.enemydata.Length; i++)
                    {
                        targetEntity = battle.enemydata[i];
                        failedStylish = false;
                        succeedStylish = false;

                        startTime = 0f;
                        endTime = 30f;
                        startPos = pebble.transform.position;
                        endPos = targetEntity.battleentity.transform.position + targetEntity.cursoroffset + new Vector3(0f, targetEntity.battleentity.height - 1f);
                        while (startTime < endTime)
                        {
                            pebble.transform.eulerAngles += new Vector3(0f, 0f, -MainManager.framestep * 20f);
                            pebble.transform.position = MainManager.BeizierCurve3(startPos, endPos, height, startTime / endTime);

                            if (!failedStylish && !succeedStylish)
                            {
                                if (StylishUtils.CheckStylish(ref failedStylish, entity, startTime, 15f))
                                {
                                    succeedStylish = true;
                                    battle.StartCoroutine(KabbuStylish.DoPebbleTossStylish(entity, stylishHits, gain: 0.04f / (float)stylishHits));
                                    stylishHits++;
                                }
                            }

                            startTime += MainManager.TieFramerate(1f);
                            yield return null;
                        }
                        entity.Emoticon(MainManager.Emoticons.None);
                        if (targetEntity.position == BattleControl.BattlePosition.Underground)
                            break;
                        MainManager.PlayParticle(NewParticle.Ripples.ToString(), new Vector3(pebble.transform.position.x, pebble.transform.position.y + 0.5f, pebble.transform.position.z));
                        battle.DoDamage(ref battle.enemydata[i], damage, new BattleControl.AttackProperty?(BattleControl.AttackProperty.NoExceptions));
                        Instance.CheckGrumbleGravel(i, battle);
                        Instance.CheckAvalanche(battle, i, icecles);
                    }
                }

                if (!bounce)
                {
                    Destroy(pebble.gameObject);
                }
                else
                {
                    MainManager.instance.StartCoroutine(MainManager.ArcMovement(pebble.gameObject, new Vector3(15f, 0f), 5f, 60f));
                    pebble.gameObject.AddComponent<SpinAround>().itself = new Vector3(0f, 0f, 20f);
                    Destroy(pebble.gameObject, 2f);
                }

                if (MainManager.BadgeIsEquipped((int)Medal.RockyRampUp) && !Instance.usedPebbleToss)
                {
                    Instance.rockyRampUpDmg++;
                    Instance.usedPebbleToss = true;
                }

                while (!MainManager.ArrayIsEmpty(icecles.ToArray()))
                {
                    yield return null;
                }
                yield return EventControl.halfsec;
            }
        }

        public void CheckAvalanche(BattleControl battleControl, int enemyID, List<GameObject> icecles)
        {
            int avalanche = BadgeHowManyEquipped((int)Medal.Avalanche);
            if (avalanche > 0)
            {
                var endPosition = battleControl.enemydata[enemyID].battleentity.transform.position + Vector3.up * battleControl.enemydata[enemyID].battleentity.height;
                icecles.Add(Instantiate(Resources.Load("Prefabs/Objects/icecle"), new Vector3(endPosition.x, 15f, endPosition.z), Quaternion.identity) as GameObject);
                battleControl.StartCoroutine(Instance.DoAvalanche(icecles[icecles.Count - 1], icecles[icecles.Count - 1].transform.position, endPosition, battleControl, enemyID, avalanche - 1));
            }
        }
        IEnumerator DoAvalanche(GameObject icecle, Vector3 startPos, Vector3 endPosition, BattleControl instance, int enemyID, int damage)
        {
            PlaySound("IceMothHit", 1.4f, 0.5f);
            float startTime = 0f;
            float endTime = 40f;
            float icecleScale = 0.7f + damage * 0.1f;
            icecle.transform.localScale *= icecleScale;

            do
            {
                float framestep = TieFramerate(1f);
                icecle.transform.position = Vector3.Lerp(startPos, endPosition, startTime / endTime);
                icecle.transform.eulerAngles += new Vector3(0f, framestep * 20f);
                startTime += framestep;
                yield return null;
            }
            while (startTime < endTime + 1f);

            PlayParticle("mothicenormal", endPosition + Vector3.up).transform.localScale = Vector3.one * icecleScale * 2f;
            if (instance.enemydata[enemyID].hp > 0 && instance.enemydata[enemyID].position != BattlePosition.Underground)
            {
                battle.DoDamage(null, ref instance.enemydata[enemyID], damage, AttackProperty.NoExceptions, new DamageOverride[] { (DamageOverride)NewDamageOverride.Magic }, false);
                battle.TryCondition(ref instance.enemydata[enemyID], BattleCondition.Freeze, 2);
            }
            icecle.transform.position = new Vector3(0f, -999f);
            yield return EventControl.halfsec;
            Destroy(icecle.gameObject);
        }


        void CheckGrumbleGravel(int enemyID, BattleControl instance)
        {
            if (MainManager.BadgeIsEquipped((int)Medal.GrumbleGravel))
            {
                MainManager.SetCondition(MainManager.BattleCondition.Taunted, ref instance.enemydata[enemyID], 1);
                var entityExt = Entity_Ext.GetEntity_Ext(instance.enemydata[enemyID].battleentity);
                entityExt.tauntedBy = instance.currentturn;
            }
        }

        public delegate int DoDamageDelegateNoAttacker(ref MainManager.BattleData target, int damage, bool block);

        static IEnumerator CheckCustomEnemyAI()
        {
            var instance = MainManager.battle;
            if (instance.enemy && actionID >= 0 && AI.HasCustomAI((NewEnemies)instance.enemydata[actionID].animid))
            {
                yield return AI.GetAI((NewEnemies)instance.enemydata[actionID].animid).DoBattleAI(Instance.entityAttacking, actionID);
            }
        }

        public IEnumerator DoLateDamage(int enemyID, int playerID, int damage, BattleControl.AttackProperty? property, float delay, BattleControl instance)
        {
            yield return new WaitForSeconds(delay);
            battle.DoDamage(instance.enemydata[enemyID], ref MainManager.instance.playerdata[playerID], damage, property, instance.commandsuccess);
        }

        static int GetNewEnemySwap(int animid)
        {
            switch ((NewEnemies)animid)
            {
                case NewEnemies.PirahnaChomp:
                    return (int)MainManager.Enemies.FlyTrap;

                case NewEnemies.Spineling:
                    return (int)MainManager.Enemies.Cactus;

                case NewEnemies.RedSeedling:
                case NewEnemies.BlueSeedling:
                    return (int)MainManager.Enemies.Seedling;
            }

            return MainManager.battle.enemydata[actionID].animid;
        }

        static bool IsCustomEnemy()
        {
            return MainManager.battle.enemydata[actionID].animid > 112;
        }

        static bool IsUsingItem()
        {
            return MainManager.battle.currentaction == BattleControl.Pick.ItemList;
        }

        IEnumerator DoVitiation(EntityControl entity)
        {
            battle.dontusecharge = true;
            MainManager.instance.camtargetpos = new Vector3?(entity.transform.position + new Vector3(2f, 0f));
            MainManager.instance.camspeed = 0.01f;
            MainManager.instance.camoffset = new Vector3(0f, 2.65f, -7f);
            entity.animstate = 102;
            yield return new WaitForSeconds(0.75f);
            entity.animstate = 119;
            yield return EventControl.quartersec;
            BattleControl.SetDefaultCamera();

            int[] targets;
            if (actionID == (int)NewSkill.Vitiation)
            {
                targets = MainManager.OrganizeArrayInt(battle.partypointer, MainManager.GradualFill(MainManager.instance.playerdata.Length));
            }
            else
            {
                (targets = new int[1])[0] = battle.target;
            }

            MainManager.PlaySound("Shield");

            int vitiationAmount = 10 * BadgeHowManyEquipped((int)Medal.ViolentVitiation);
            for (int i = 0; i < targets.Length; i++)
            {
                if (MainManager.instance.playerdata[targets[i]].hp > 0 && MainManager.instance.playerdata[targets[i]].eatenby == null)
                {
                    SetCondition((BattleCondition)NewCondition.Vitiation, ref MainManager.instance.playerdata[targets[i]], vitiationAmount);
                }
            }

            float startTime = 0f;
            float endTime = 45f;

            bool failedStylish = false;
            bool succeedStylish = false;
            while (startTime < endTime)
            {
                if (!failedStylish && !succeedStylish)
                {
                    if (StylishUtils.CheckStylish(ref failedStylish, entity, startTime, 25f))
                    {
                        succeedStylish = true;
                        MainManager.battle.StartCoroutine(DoStylish(0, 0.1f));
                    }
                }

                startTime += MainManager.TieFramerate(1f);
                yield return null;
            }

            entity.Emoticon(MainManager.Emoticons.None);
            yield return WaitStylish(0f);
            yield break;
        }

        IEnumerator DoLecture(EntityControl entity)
        {
            yield return EventControl.tenthsec;
            AudioClip bleep = Resources.Load<AudioClip>("Audio/Sounds/Dialogue/Dialogue" + entity.dialoguebleepid);
            entity.talking = true;
            entity.animstate = (int)MainManager.Animations.Idle;

            int stepsAmount = 3;
            int baseInputs = 3;
            int baseOdds = 40;

            int[] kabbusAnims = new int[] { (int)MainManager.Animations.Idle, (int)MainManager.Animations.Angry, (int)MainManager.Animations.BattleIdle };
            int[] partyAnims = new int[] { (int)MainManager.Animations.Surprized, (int)MainManager.Animations.WeakPickAction, (int)MainManager.Animations.Sleep };

            for (int i = 0; i < stepsAmount; i++)
            {
                if (i != 0)
                {
                    yield return EventControl.sec;
                }
                entity.animstate = kabbusAnims[UnityEngine.Random.Range(0, kabbusAnims.Length)];
                MainManager.battle.StartCoroutine(battle.DoCommand(180f, ActionCommands.SequentialKeys, new float[] { baseInputs + i }));

                yield return null;
                int x = 0;
                while (MainManager.battle.doingaction)
                {
                    yield return null;
                    MainManager.PlayBleep(bleep, entity.bleeppitch, 1, x);
                    x++;
                    yield return null;
                }

                if (MainManager.battle.commandsuccess)
                {
                    baseOdds += 20;
                    entity.animstate = (int)MainManager.Animations.Happy;

                    for (int j = 0; j < MainManager.instance.playerdata.Length; j++)
                    {
                        if (MainManager.instance.playerdata[j].battleentity != entity && MainManager.instance.playerdata[j].hp > 0
                            && !battle.IsStopped(MainManager.instance.playerdata[j]))
                        {
                            if (partyAnims[i] == (int)MainManager.Animations.Surprized && j == 2)
                            {
                                MainManager.instance.playerdata[j].battleentity.animstate = (int)MainManager.Animations.Idle;
                            }
                            else
                            {
                                MainManager.instance.playerdata[j].battleentity.animstate = partyAnims[i];
                            }
                        }
                    }
                    StartStylishTimer(6, 15, stylishID: i, stylishGain: 0.04f, commandSuccess: false);
                }
                else
                {
                    break;
                }
            }

            entity.talking = false;
            Sprite[] notesprite = Resources.LoadAll<Sprite>("Sprites/Particles/music");

            float frequency = 1f;
            float amplitude = 1.5f;
            Vector3 targetPos = new Vector3(7, 3f, 0.1f);
            Vector3 startPos = new Vector3(0, 3f, 0.1f);
            int musicAmount = 12;
            SpriteRenderer[] musics = new SpriteRenderer[musicAmount];

            MainManager.PlaySound("Charge4", 1.2f, 1);
            yield return EventControl.tenthsec;

            for (int i = 0; i < musics.Length; i++)
            {
                float t = (float)i / (musicAmount - 1);
                Vector3 basePosition = Vector3.Lerp(startPos, targetPos, t);
                float waveOffset = Mathf.Sin(t * frequency * Mathf.PI * 2) * amplitude;
                basePosition.y += waveOffset;
                musics[i] = new GameObject().AddComponent<SpriteRenderer>();
                musics[i].sprite = notesprite[UnityEngine.Random.Range(0, notesprite.Length)];
                musics[i].color = MainManager.RainbowColor(UnityEngine.Random.Range(0, 10));

                musics[i].color = new Color(musics[i].color.r, musics[i].color.g, musics[i].color.b, 0);
                musics[i].transform.position = basePosition;
                MainManager.battle.StartCoroutine(MainManager_Ext.LerpSpriteColor(musics[i], 30f, new Color(musics[i].color.r,
                    musics[i].color.g, musics[i].color.b, 1)));
                MainManager.battle.StartCoroutine(LerpPosition(120, musics[i].transform.position, musics[i].transform.position + Vector3.up,
                    musics[i].transform));
                yield return new WaitForSeconds(0.05f);
            }

            yield return EventControl.halfsec;

            for (int i = 0; i < musics.Length; i++)
            {
                MainManager.battle.StartCoroutine(MainManager_Ext.LerpSpriteColor(musics[i], 30f, new Color(musics[i].color.r, musics[i].color.g,
                    musics[i].color.b, 0)));
                Destroy(musics[i].gameObject, 2f);
            }
            yield return EventControl.tenthsec;

            for (int i = 0; i < MainManager.battle.enemydata.Length; i++)
            {
                if (UnityEngine.Random.Range(0, 100) < baseOdds
                    && (MainManager.battle.enemydata[i].position != BattlePosition.Underground
                    || MainManager.battle.enemydata[i].position != BattlePosition.OutOfReach))
                {
                    battle.StatusEffect(battle.enemydata[i], BattleCondition.AttackDown, 1, true, false);
                    battle.TryCondition(ref MainManager.battle.enemydata[i], BattleCondition.Sleep, 2);
                }
            }
            yield return EventControl.quartersec;
        }

        IEnumerator DoCordycepsLeech(EntityControl entity)
        {
            Vector3 basePos = entity.transform.position;
            int target = battle.GetEnemies(false, false, false)[0];

            EntityControl targetEntity = MainManager.battle.enemydata[target].battleentity;
            MainManager.SetCamera(targetEntity.transform.position, MainManager.instance.camangleoffset, new Vector3(0f, 2.5f, -6f), 0.02f);

            float size = battle.GetEnemySize(target);

            float baseCamOffset = MainManager.instance.camoffset.y;
            entity.MoveTowards(targetEntity.transform.position + Vector3.left * Mathf.Clamp(size, 1.3f, float.PositiveInfinity) + Vector3.back * 0.25f, 2f, 1, 0);
            while (entity.forcemove)
            {
                yield return null;
            }

            MainManager.instance.camspeed = 0.01f;
            MainManager.instance.camoffset = new Vector3(0f, 2.5f, -5f);
            entity.animstate = (int)MainManager.Animations.WeakBattleIdle;

            float[] data = new float[] { -1f, 6.5f, 0.15f, 1f, 0f, 0f, 0f, 0f, 0f, 4f };
            MainManager.PlaySound("Fungi", -1, 1f, 1f, true);
            MainManager.battle.StartCoroutine(battle.DoCommand(180f, ActionCommands.TappingKey, data));
            yield return null;

            Vector3 startp = entity.spritetransform.localPosition;
            Vector3 intensity = new Vector3(0.1f, 0);
            entity.StartCoroutine(entity.ShakeSprite(0.1f, 240f));
            yield return new WaitUntil(() => !MainManager.battle.doingaction);

            int playerAtk = MainManager.instance.playerdata[MainManager.battle.currentturn].atk;
            int baseDamage = playerAtk + BASE_CORYCEPSLEECH_DMG;
            int damage = Mathf.Clamp(Mathf.CeilToInt((baseDamage) * battle.barfill), 4, baseDamage);
            yield return EventControl.sec;

            data = new float[] { (float)UnityEngine.Random.Range(4, 7) };
            MainManager.battle.StartCoroutine(battle.DoCommand(60f, ActionCommands.PressKeyTimer, data));

            yield return null;
            yield return new WaitUntil(() => !MainManager.battle.doingaction);
            yield return EventControl.halfsec;
            MainManager.StopSound("Fungi");
            MainManager.instance.camspeed = 0.1f;
            MainManager.instance.camoffset = new Vector3(0f, baseCamOffset, -7f);
            MainManager.PlaySound("Buzz1");

            yield return EventControl.tenthsec;

            if (battle.commandsuccess)
            {
                MainManager.ShakeScreen(0.2f, 0.5f, true);
                MainManager.PlaySound("HugeHit");
            }
            else
            {
                damage = damage / 2;
            }

            //cordyceps anim
            entity.animstate = 124;

            yield return EventControl.tenthsec;
            int damageDone = battle.DoDamage(MainManager.instance.playerdata[MainManager.battle.currentturn], ref MainManager.battle.enemydata[target], damage, null, null, false);
            yield return null;
            entity.animstate = 124;

            float healMult = 0.75f;
            int healValue = Mathf.Clamp(Mathf.CeilToInt(damageDone * healMult), 0, 99);
            if (BadgeIsEquipped((int)BadgeTypes.HPFunnel,  MainManager.instance.playerdata[battle.currentturn].trueid))
                Instance.RecoverPlayerTP(healValue, MainManager.instance.playerdata[battle.currentturn]);
            else
                battle.Heal(ref MainManager.instance.playerdata[battle.currentturn], healValue, false);

            yield return EventControl.halfsec;
            StartStylishTimer(3, 15);
            entity.animstate = (int)MainManager.Animations.WeakBattleIdle;
            yield return EventControl.halfsec;
            yield return WaitStylish(0);
        }

        IEnumerator DoPlayerThrowable(EntityControl entity)
        {
            battle.nonphyscal = true;
            battle.dontusecharge = true;

            int target = 0;
            if (battle.target > -1 && battle.target < battle.avaliabletargets.Length)
                target = battle.avaliabletargets[battle.target].battleentity.battleid;

            int selectedItem = battle.selecteditem;
            bool curve = true;
            bool spin = true;
            if (selectedItem == (int)NewItem.WebWad)
            {
                MainManager.SetCondition(BattleCondition.Sticky, ref MainManager.instance.playerdata[battle.currentturn], 2);
                MainManager.PlayParticle("StickyGet", entity.transform.position + Vector3.up);
                MainManager.PlaySound("AhoneynationSpit", -1, 0.8f, 1f);
            }
            if (selectedItem == (int)Items.PoisonDart || selectedItem == (int)Items.NumbDart)
            {
                curve = false;
                spin = false;
            }

            MainManager.Items thrownItem = selectedItem == (int)Items.CherryBomb ? (Items)NewItem.CherryBomb2 : (Items)selectedItem;
            yield return ThrowItem(entity, battle, thrownItem, battle.itemarea, curve, spin);

            int damage;
            int piercedStatusRes;
            AttackProperty property;
            List<DamageOverride> overrides;
            float splashRadius;
            Entity_Ext extEnt;
            switch (selectedItem)
            {
                case (int)Items.HardSeed:
                case (int)Items.PoisonDart:
                case (int)Items.NumbDart:
                case (int)NewItem.BeeBattery:
                case (int)Items.Ice:
                case (int)Items.FlameRock:
                    damage = 2;
                    property = AttackProperty.None;
                    overrides = new List<DamageOverride>() { (DamageOverride)NewDamageOverride.Pierce1 };
                    piercedStatusRes = 0;
                    switch (selectedItem)
                    {
                        case (int)Items.HardSeed:
                            property = AttackProperty.Pierce;
                            break;
                        case (int)Items.PoisonDart:
                            property = AttackProperty.Poison;
                            piercedStatusRes = 40;
                            break;
                        case (int)Items.NumbDart:
                            property = AttackProperty.Sleep;
                            overrides.Add(DamageOverride.DontAwake);
                            piercedStatusRes = 40;
                            break;
                        case (int)NewItem.BeeBattery:
                            property = AttackProperty.Numb1Turn;
                            piercedStatusRes = 40;
                            break;
                        case (int)Items.Ice:
                            property = AttackProperty.Freeze;
                            overrides.Add((DamageOverride)NewDamageOverride.Magic);
                            piercedStatusRes = 40;
                            break;
                        case (int)Items.FlameRock:
                            property = AttackProperty.Fire;
                            overrides.Add((DamageOverride)NewDamageOverride.Magic);
                            break;
                    }
                    extEnt = Entity_Ext.GetEntity_Ext(battle.enemydata[target].battleentity);
                    extEnt.piercedStatusRes += piercedStatusRes;
                    battle.DoDamage(null, ref battle.enemydata[target], damage, property, overrides.ToArray(), false);
                    extEnt.piercedStatusRes -= piercedStatusRes;
                    break;

                case (int)NewItem.WebWad:
                    damage = 1;
                    overrides = new List<DamageOverride>() {
                        (DamageOverride)NewDamageOverride.Pierce1,
                        (DamageOverride)NewDamageOverride.Pierce1
                    };
                    battle.DoDamage(null, ref battle.enemydata[target], damage, null, overrides.ToArray(), false);
                    if (battle.enemydata[target].hp > 0)
                    {
                        SetCondition(BattleCondition.Sticky, ref battle.enemydata[target], 4);
                        PlayParticle("StickyGet", battle.enemydata[target].battleentity.transform.position + Vector3.up);
                    }
                    PlaySound("AhoneynationSpit", -1, 0.8f, 1f);
                    break;

                case (int)NewItem.MysterySeed:
                    damage = 4;
                    piercedStatusRes = 100;
                    overrides = new List<DamageOverride>() { (DamageOverride)NewDamageOverride.Pierce1 };
                    extEnt = Entity_Ext.GetEntity_Ext(battle.enemydata[target].battleentity);
                    extEnt.piercedStatusRes += piercedStatusRes;
                    battle.DoDamage(null, ref battle.enemydata[target], damage, null, overrides.ToArray(), false);
                    extEnt.piercedStatusRes -= piercedStatusRes;
                    MainManager_Ext.Instance.DoRandomMysteryBuff(target, 4, false);
                    MainManager_Ext.Instance.DoRandomMysteryDebuff(target, 4, false);
                    break;

                case (int)NewItem.SquashSeed:
                case (int)NewItem.SucculentSeed:
                    damage = 4;
                    overrides = new List<DamageOverride>() { (DamageOverride)NewDamageOverride.Pierce1 };
                    damage = battle.DoDamage(null, ref battle.enemydata[target], damage, null, overrides.ToArray(), false);
                    if (damage > 0)
                    {
                        if (selectedItem == (int)NewItem.SucculentSeed)
                            battle.Heal(ref MainManager.instance.playerdata[battle.currentturn], damage);

                        if (selectedItem == (int)NewItem.SquashSeed)
                            battle.HealTP(damage);
                    }
                    break;

                case (int)Items.SpicyBomb:
                case (int)Items.BurlyBomb:
                case (int)Items.PoisonBomb:
                case (int)Items.SleepBomb:
                case (int)Items.NumbBomb:
                case (int)Items.FrostBomb:
                case (int)Items.CherryBomb:
                case (int)NewItem.CherryBomb2:
                    Vector3 targetCenterPos = battle.CenterPos(battle.enemydata[target], true);
                    damage = 3;
                    property = AttackProperty.None;
                    overrides = null;
                    splashRadius = 15.5f;
                    string particle = null;
                    float shakeAmount = 0.2f;
                    Vector3 particleScale = Vector3.one;
                    Color? particleColor = null;
                    switch (selectedItem)
                    {
                        case (int)Items.SpicyBomb:
                            property = AttackProperty.AtkDownOnBlock;
                            particle = "explosion";
                            particleColor = new Color(0.7f, 0.6f, 0.6f);
                            break;
                        case (int)Items.BurlyBomb:
                            damage--;
                            property = AttackProperty.DefDownOnBlock;
                            particle = "explosion";
                            particleColor = new Color(0.6f, 0.6f, 0.7f);
                            break;
                        case (int)Items.PoisonBomb:
                            property = AttackProperty.Poison;
                            particle = "PoisonEffect";
                            particleScale *= 2f;
                            break;
                        case (int)Items.SleepBomb:
                            property = AttackProperty.Sleep;
                            overrides = new List<DamageOverride>() { DamageOverride.DontAwake };
                            shakeAmount = 0.1f;
                            DeathSmoke(targetCenterPos, Vector3.one * 3f);
                            break;
                        case (int)Items.NumbBomb:
                            property = AttackProperty.Numb1Turn;
                            PlayParticle("impactsmoke", targetCenterPos);
                            particle = "ElecFast";
                            shakeAmount = 0.1f;
                            particleScale *= 2f;
                            break;
                        case (int)Items.FrostBomb:
                            property = AttackProperty.Freeze;
                            overrides = new List<DamageOverride>() { (DamageOverride)NewDamageOverride.Magic };
                            particle = "mothicenormal";
                            shakeAmount = 0.1f;
                            particleScale *= 3f;
                            break;
                        case (int)NewItem.CherryBomb2:
                        case (int)Items.CherryBomb:
                            damage++;
                            overrides = new List<DamageOverride>() { // hits all weaknesses
                                (DamageOverride)NewDamageOverride.Beemerang,
                                (DamageOverride)NewDamageOverride.FlipNoPierce,
                                (DamageOverride)NewDamageOverride.FlipNoPierce,
                                (DamageOverride)NewDamageOverride.Magic,
                                (DamageOverride)NewDamageOverride.ExtraAirTopple,
                            };
                            splashRadius *= 1.3f;
                            particle = "PoisonEffect";
                            shakeAmount = 0.3f;
                            particleScale *= 3f;
                            particleColor = new Color(156 / 306f, 113 / 360f, 173 / 360f);

                            if(selectedItem == (int)Items.CherryBomb && MainManager.instance.items[0].Count < MainManager.instance.maxitems)
                                MainManager_Ext.DoNewItemUse(NewItemUse.MultiUse, (int)NewItem.CherryBomb2, null);
                            break;
                    }
                    if (particle != null)
                    {
                        GameObject obj = PlayParticle(particle, targetCenterPos);
                        obj.transform.localScale = particleScale;
                        if (particleColor != null && obj.GetComponent<ParticleSystem>() is ParticleSystem ps)
                        {
                            var pmain = ps.main;
                            pmain.startColor = particleColor.Value;
                        }
                    }
                    damage += 2 * BadgeHowManyEquipped((int)BadgeTypes.BombPlus);
                    battle.DoDamage(null, ref battle.enemydata[target], damage + 2, property, overrides?.ToArray(), false);
                    if (splashRadius > 0 && battle.enemydata.Length > 1)
                    {
                        for (int e = 0; e < battle.enemydata.Length; e++)
                        {
                            EntityControl collat = battle.enemydata[e].battleentity;
                            if (e == target || GetSqrDistance(
                                    collat.transform.position + collat.freezeoffset + Vector3.up * collat.height,
                                    battle.enemydata[target].battleentity.transform.position + battle.enemydata[target].cursoroffset + new Vector3(0, battle.enemydata[target].battleentity.height - 1f)) > splashRadius)
                            {
                                continue;
                            }

                            if (selectedItem == (int)Items.BurlyBomb)
                            {
                                battle.DoDamage(null, ref battle.enemydata[e], damage, null, overrides?.ToArray(), false);
                                battle.StatusEffect(battle.enemydata[e], BattleCondition.DefenseDown, 3, effect: true);
                            }
                            else
                            {
                                battle.DoDamage(null, ref battle.enemydata[e], damage, property, overrides?.ToArray(), false);
                            }
                        }
                    }
                    PlaySound("Explosion");
                    ShakeScreen(shakeAmount, 0.75f, true);
                    break;

                case (int)NewItem.InkBomb:
                    Vector3 position = Vector3.zero;
                    PlayParticle("InkGet", position).transform.localScale = Vector3.one * 1.5f;
                    GameObject partcle = PlayParticle("impactsmoke", "BubbleBurst", position);
                    partcle.transform.localScale = Vector3.one * 2f;
                    var main = partcle.GetComponent<ParticleSystem>().main;
                    main.startColor = new Color(1f, 0, 1f);

                    damage = 5 + 2 * BadgeHowManyEquipped((int)BadgeTypes.BombPlus);
                    for (int i = 0; i < battle.enemydata.Length; i++)
                    {
                        if (battle.enemydata[i].hp > 0 && battle.enemydata[i].position != BattlePosition.Underground)
                        {
                            battle.DoDamage(null, ref battle.enemydata[i], damage, AttackProperty.Pierce, null, false);
                            SetCondition(BattleCondition.Inked, ref battle.enemydata[i], 4);
                        }
                    }
                    PlaySound("WaterSplash2", -1, 0.8f, 1f);
                    for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                    {
                        if (MainManager.instance.playerdata[i].hp > 0)
                        {
                            SetCondition(BattleCondition.Inked, ref MainManager.instance.playerdata[i], 2);
                        }
                    }
                    PlaySound("Explosion");
                    ShakeScreen(Vector3.one * 0.1f, 0.15f);
                    break;

                case (int)NewItem.MysteryBomb:
                    damage = 5 + 2 * BadgeHowManyEquipped((int)BadgeTypes.BombPlus);
                    piercedStatusRes = 50;
                    for (int i = 0; i < battle.enemydata.Length; i++)
                    {
                        if (battle.enemydata[i].hp > 0 && battle.enemydata[i].position != BattlePosition.OutOfReach)
                        {
                            battle.DoDamage(null, ref battle.enemydata[i], damage, AttackProperty.Flip, null, false);
                            extEnt = Entity_Ext.GetEntity_Ext(battle.enemydata[i].battleentity);
                            extEnt.piercedStatusRes += piercedStatusRes;
                            MainManager_Ext.Instance.DoRandomMysteryBuff(i, 2, false);
                            MainManager_Ext.Instance.DoRandomMysteryDebuff(i, 2, false);
                            extEnt.piercedStatusRes -= piercedStatusRes;
                        }
                    }
                    PlayParticle("explosion", Vector3.right * 2);
                    PlaySound("Explosion");
                    ShakeScreen(Vector3.one * 0.1f, 0.15f);
                    break;

                case (int)Items.Abombhoney:
                    yield return DoAbomb(new Vector3(0f, 0f, -0.5f), battle);
                    break;

                case (int)Items.ClearBomb:
                    battle.checkingdead = battle.StartCoroutine(battle.ClearBombEffect());
                    break;
            }
            yield return EventControl.halfsec;
        }

        IEnumerator DoInkTrap(bool usedByEnemy, EntityControl entity, MainManager.BattleData summonedBy)
        {
            battle.dontusecharge = true;
            Vector3 startPos = entity.transform.position;

            GameObject inkTrap = Instantiate(Resources.Load("Prefabs/Objects/BombCart"), entity.transform.position + new Vector3(0f, -2f), Quaternion.identity, battle.battlemap.transform) as GameObject;
            SpriteRenderer bombSprite = inkTrap.transform.GetChild(0).GetComponent<SpriteRenderer>();
            bombSprite.sprite = MainManager.itemsprites[0, (int)NewItem.InkBomb];

            if (!usedByEnemy)
            {
                bombSprite.flipX = true;
                inkTrap.GetComponent<SpriteRenderer>().flipX = true;
            }
            int randomMult = UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1;
            Vector3 randompos = startPos + new Vector3(1, 0, -2) * randomMult;
            entity.MoveTowards(randompos, 2f);
            MainManager.SetCamera(randompos, 0.035f);
            while (entity.forcemove)
            {
                yield return null;
            }
            yield return EventControl.halfsec;

            if (!usedByEnemy)
            {
                switch (summonedBy.trueid)
                {
                    case 0:
                        entity.animstate = (int)MainManager.Animations.Happy;
                        break;
                    case 1:
                        entity.animstate = 105;
                        break;
                    case 2:
                        entity.animstate = 102;
                        break;
                }
            }
            MainManager.PlaySound("Dig2", -1, 1.1f, 1f);

            Vector3 start = new Vector3(0.5f, 0.85f, -0.1f);

            yield return LerpPosition(30f, start + entity.transform.position, new Vector3(-0.25f, -0.25f, -0.1f) + entity.transform.position, inkTrap.transform);

            GameObject part = MainManager.PlayParticle("Digging", inkTrap.transform.position, -1f);
            yield return EventControl.halfsec;
            float a = 0f;
            float b = 10f;
            do
            {
                inkTrap.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, a / b);
                a += MainManager.TieFramerate(1f);
                yield return null;
            }
            while (a < b + 1f);
            part.transform.parent = inkTrap.transform;
            part.transform.localScale = Vector3.one;
            int inkTrapDamage = 5;
            List<DamageOverride> overrides = new List<DamageOverride>() { (DamageOverride)NewDamageOverride.Pierce1 };
            if (!usedByEnemy)
            {
                MainManager.BattleData targetData = MainManager.battle.avaliabletargets[battle.target];
                int target = targetData.battleentity.battleid;
                AddDelProjsPlayer(inkTrap, DelProjType.InkTrap, target, inkTrapDamage, 1, 0, AttackProperty.Ink, overrides, 35f, summonedBy, "WaterSplash2", "InkGet", "Digging@Down", false, new List<BattlePosition>() { BattlePosition.Ground, BattlePosition.Underground});
                delProjsPlayer[delProjsPlayer.Count - 1].delProjData.args = "move,0,-0.5,0@noshadow@partoff,0,0.5,0@partoff,0,1,0";
            }
            else
            {
                Instance.enemyDelProjUsesAtkBonuses = false;
                battle.AddDelayedProjectile(inkTrap, battle.playertargetID, inkTrapDamage, 1, 0, AttackProperty.Ink, 35f, summonedBy, "WaterSplash2", "InkGet", "Digging@Down");
                Instance.enemyDelProjUsesAtkBonuses = true;
                DelayedProjExtra.AddDelayedProjExtra(inkTrap, null, null, overrides);
                battle.delprojs[battle.delprojs.Length - 1].args = "move,0,-0.5,0@noshadow@partoff,0,0.5,0@partoff,0,1,0";
            }
            yield return EventControl.halfsec;
        }

        IEnumerator DoStickyBomb(bool usedByEnemy, EntityControl entity, MainManager.BattleData summonedBy)
        {
            battle.dontusecharge = true;
            yield return EventControl.tenthsec;
            entity.animstate = 28;
            MainManager.PlaySound("Toss");
            yield return EventControl.tenthsec;

            Vector3 itemPos;

            if (usedByEnemy)
            {
                itemPos = entity.transform.position + new Vector3(-0.5f, 2f, -0.1f) + Vector3.up * entity.height;
                MainManager.SetCondition(MainManager.BattleCondition.Sticky, ref battle.enemydata[actionID], 2);
            }
            else
            {
                itemPos = entity.transform.position + MainManager.instance.playerdata[battle.currentturn].cursoroffset - Vector3.up;
                MainManager.SetCondition(MainManager.BattleCondition.Sticky, ref MainManager.instance.playerdata[battle.currentturn], 2);
            }
            MainManager.PlayParticle("StickyGet", entity.transform.position + Vector3.up);
            MainManager.PlaySound("AhoneynationSpit", -1, 0.8f, 1f);
            SpriteRenderer stickyBomb = MainManager.NewSpriteObject(itemPos, null, MainManager.itemsprites[0, (int)NewItem.StickyBomb]);

            Vector3 startPos = stickyBomb.transform.position;
            Vector3 targetpos;
            int target = 0;
            if (usedByEnemy)
            {
                targetpos = battle.partymiddle;
            }
            else
            {
                MainManager.BattleData targetData = MainManager.battle.avaliabletargets[battle.target];
                target = targetData.battleentity.battleid;
                var targetEntity = battle.enemydata[target].battleentity;
                targetpos = targetEntity.transform.position + battle.enemydata[target].cursoroffset + new Vector3(0f, targetEntity.height - 1f);
            }
            Vector3 endPos = new Vector3(targetpos.x, 0.4f, targetpos.z - 0.1f);

            float a = 0f;
            float b = 35f;
            do
            {
                stickyBomb.transform.position = MainManager.BeizierCurve3(startPos, endPos, 5, a / b);
                stickyBomb.transform.eulerAngles += new Vector3(0f, 0f, -MainManager.framestep * 20f);
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < b);

            stickyBomb.transform.position = endPos;
            MainManager.PlayParticle("StickyGet", endPos);
            MainManager.PlaySound("AhoneynationSpit", -1, 0.8f, 1f);

            int stickyBombDamage = 6;
            int areaDamage = 4;
            if (!usedByEnemy)
            {
                int bonusDMG = 2 * MainManager.BadgeHowManyEquipped((int)MainManager.BadgeTypes.BombPlus);
                stickyBombDamage += bonusDMG;
                areaDamage += bonusDMG;
            }

            if (!usedByEnemy)
            {
                AddDelProjsPlayer(stickyBomb.gameObject, DelProjType.StickyBomb, target, stickyBombDamage, 1, areaDamage, null, null, 35f, summonedBy, "AhoneynationSpit", "explosion", "Explosion", false, new List<BattlePosition>() { BattlePosition.Ground, BattlePosition.Flying });
                SetDelProjPlayerArgs(delProjsPlayer.Count - 1, "move,0,-0.5,0@noshadow@partoff,0,0.5,0@partoff,0,1,0");
            }
            else
            {
                Instance.enemyDelProjUsesAtkBonuses = false;
                battle.AddDelayedProjectile(stickyBomb.gameObject, 0, stickyBombDamage, 1, areaDamage + 2, null, 35f, summonedBy, "AhoneynationSpit", "explosion", "Explosion");
                Instance.enemyDelProjUsesAtkBonuses = true;
                DelayedProjExtra.AddDelayedProjExtra(stickyBomb.gameObject, new int[] { 4 }, StickyBombExtraEffects);
                battle.delprojs[battle.delprojs.Length - 1].args = "move,0,-0.5,0@noshadow@partoff,0,0.5,0@partoff,0,1,0";
            }
        }
        static IEnumerator StickyBombExtraEffects(int projIndex, int[] data, int damageDone)
        {
            for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
            {
                SetCondition(BattleCondition.Sticky, ref MainManager.instance.playerdata[i], data[0]);
                PlayParticle("StickyGet", MainManager.instance.playerdata[i].battleentity.transform.position + Vector3.up);
                PlaySound("WaterSplash2", -1, 0.8f, 1f);
            }
            yield return null;
        }

        public void SetDelProjPlayerArgs(int index, string args)
        {
            if(index >=0 && index < delProjsPlayer.Count)
                delProjsPlayer[index].delProjData.args = args;
        }

        public void SetLastDelProjArgs(string args) => SetDelProjPlayerArgs(delProjsPlayer.Count - 1, args);

        IEnumerator DoFlameBomb(EntityControl entity, bool usedByEnemy)
        {
            battle.dontusecharge = true;
            yield return EventControl.tenthsec;
            entity.animstate = 28;
            MainManager.PlaySound("Toss");
            yield return EventControl.tenthsec;

            Vector3 itemPos;

            if (usedByEnemy)
            {
                itemPos = entity.transform.position + new Vector3(-0.5f, 2f, -0.1f) + Vector3.up * entity.height;
            }
            else
            {
                itemPos = entity.transform.position + MainManager.instance.playerdata[battle.currentturn].cursoroffset - Vector3.up;
            }
            SpriteRenderer flameBomb = MainManager.NewSpriteObject(itemPos, null, MainManager.itemsprites[0, (int)NewItem.FlameBomb]);
            GameObject flamePart = MainManager.PlayParticle("Flame", flameBomb.transform.position);
            flamePart.transform.parent = flameBomb.transform;

            Vector3 startPos = flameBomb.transform.position;
            Vector3 endPos = new Vector3(0, 0.4f, 0 - 0.1f);

            float a = 0f;
            float b = 35f;
            do
            {
                flameBomb.transform.position = MainManager.BeizierCurve3(startPos, endPos, 5, a / b);
                flameBomb.transform.eulerAngles += new Vector3(0f, 0f, -MainManager.framestep * 20f);
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < b);
            flameBomb.transform.position = endPos;

            entity.animstate = 0;

            int amount = 4;
            int damage = 3;

            if (!usedByEnemy && MainManager.BadgeIsEquipped((int)MainManager.BadgeTypes.BombPlus))
                damage += 2;

            Transform[] fireballs = new Transform[2];

            for (int i = 0; i < amount; i++)
            {
                if (MainManager.GetAlivePlayerAmmount() == 0 || battle.AliveEnemies() == 0)
                    break;
                MainManager.PlayParticle("explosion", endPos);
                MainManager.PlaySound("Explosion");

                for (int j = 0; j < fireballs.Length; j++)
                {
                    fireballs[j] = (Instantiate(Resources.Load("Prefabs/Particles/Fireball"), endPos, Quaternion.identity, battle.battlemap.transform) as GameObject).transform;
                    MainManager.PlaySound("WaspKingMFireball1");
                    int target = -1;
                    if (j == 0)
                    {
                        battle.GetSingleTarget();
                        target = battle.playertargetID;
                    }
                    else
                    {
                        int[] ids = battle.enemydata.Select((e, index) => (e.hp > 0 && e.position != BattlePosition.Underground) ? index : -1)
                        .Where(index => index != -1).ToArray();
                        if (ids.Length > 0)
                            target = ids[UnityEngine.Random.Range(0, ids.Length)];
                    }

                    if (target != -1)
                        StartCoroutine(DoFireballProj(target, fireballs[j], j == 1, damage));
                    else
                        Destroy(fireballs[j].gameObject);
                }
                yield return EventControl.halfsec;
                yield return EventControl.quartersec;
            }

            if (battle.enemy)
            {
                FixEnemyDiedOnItemUse();
            }
            Destroy(flameBomb.gameObject);
        }

        IEnumerator ThrowWhirly(EntityControl entity, bool usedByEnemy, int item, Vector3 targetPos)
        {
            entity.animstate = 28;
            MainManager.PlaySound("Toss");
            yield return EventControl.tenthsec;

            Vector3 itemPos;

            if (usedByEnemy)
            {
                itemPos = entity.transform.position + new Vector3(-0.5f, 2f, -0.1f) + Vector3.up * entity.height;
            }
            else
            {
                itemPos = entity.transform.position + MainManager.instance.playerdata[battle.currentturn].cursoroffset - Vector3.up;
            }
            SpriteRenderer whirlySeed = MainManager.NewSpriteObject(itemPos, null, MainManager.itemsprites[0, item]);

            Vector3 startPos = whirlySeed.transform.position;
            Vector3 endPos = new Vector3(1, 4f, -0.1f);

            float a = 0f;
            float b = 35f;
            do
            {
                whirlySeed.transform.position = MainManager.BeizierCurve3(startPos, endPos, 5, a / b);
                whirlySeed.transform.eulerAngles += new Vector3(0, 0, -10);
                whirlySeed.transform.eulerAngles = new Vector3(Mathf.Lerp(0, 70, a / b), whirlySeed.transform.eulerAngles.y, whirlySeed.transform.eulerAngles.z);
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < b);
            whirlySeed.transform.position = endPos;
            startPos = endPos;

            entity.animstate = 0;

            GameObject hurricanePart = MainManager.PlayParticle("Hurricane", whirlySeed.transform.position, -1);
            hurricanePart.transform.parent = whirlySeed.transform;

            a = 0f;
            b = 45f;
            do
            {
                whirlySeed.transform.position = Vector3.Lerp(startPos, targetPos, a / b);
                whirlySeed.transform.eulerAngles += new Vector3(0, 0f, -30f);
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < b);

            whirlySeed.enabled = false;
            Destroy(whirlySeed.gameObject, 5);
            hurricanePart.GetComponent<ParticleSystem>().Stop();
        }

        IEnumerator DoWhirlyBomb(EntityControl entity, bool usedByEnemy)
        {
            int damage = 3;
            int dizzyTurns = 3;
            List<DamageOverride> overrides = new List<DamageOverride>() {
                (DamageOverride)NewDamageOverride.FlipNoPierce,
                (DamageOverride)NewDamageOverride.Pierce1,
                (DamageOverride)NewDamageOverride.Pierce1,
                (DamageOverride)NewDamageOverride.Pierce1
            };

            battle.dontusecharge = true;
            yield return EventControl.tenthsec;

            Vector3 targetPos;
            if (usedByEnemy)
            {
                targetPos = battle.partymiddle;
            }
            else
            {
                damage += 2 * MainManager.BadgeHowManyEquipped((int)BadgeTypes.BombPlus);
                targetPos = (battle.enemydata[0].battlepos + battle.enemydata[battle.enemydata.Length - 1].battlepos) / 2;
                targetPos = new Vector3(targetPos.x, 0);
            }

            yield return ThrowWhirly(entity, usedByEnemy, (int)NewItem.WhirlyBomb, targetPos);

            MainManager.ShakeScreen(0.2f, 0.75f, true);
            MainManager.PlayParticle("explosion", targetPos);
            MainManager.PlaySound("Explosion");

            if (usedByEnemy)
            {
                for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                {
                    if (MainManager.instance.playerdata[battle.playertargetID].hp > 0)
                    {
                        battle.DoDamage(null, ref MainManager.instance.playerdata[battle.playertargetID], damage, null, overrides.ToArray(), battle.commandsuccess);
                        if(!battle.commandsuccess)
                            Instance.TryDizzy(null, ref MainManager.instance.playerdata[battle.playertargetID], dizzyTurns);
                    }
                }
            }
            else
            {
                for (int i = 0; i < battle.enemydata.Length; i++)
                {
                    if (battle.enemydata[i].hp > 0 && battle.enemydata[i].position != BattlePosition.Underground)
                    {
                        battle.DoDamage(null, ref battle.enemydata[i], damage, null, overrides.ToArray(), false);
                        Instance.TryDizzy(null, ref battle.enemydata[i], dizzyTurns);
                    }
                }
            }
        }

        IEnumerator DoWhirlySeed(EntityControl entity, bool usedByEnemy)
        {
            int damage = 2;
            int dizzyTurns = 3;
            int dizzyResPierce = 40;
            List<DamageOverride> overrides = new List<DamageOverride>() {
                (DamageOverride)NewDamageOverride.FlipNoPierce,
                (DamageOverride)NewDamageOverride.Pierce1,
                (DamageOverride)NewDamageOverride.Pierce1,
                (DamageOverride)NewDamageOverride.Pierce1
            };

            battle.dontusecharge = true;
            yield return EventControl.tenthsec;

            Vector3 targetPos;
            int targetId = 0;
            if (usedByEnemy)
            {
                targetPos = battle.playertargetentity.transform.position + new Vector3(0, 1, -0.1f);
            }
            else
            {
                MainManager.BattleData targetData = MainManager.battle.avaliabletargets[battle.target];
                targetId = targetData.battleentity.battleid;
                var targetEntity = battle.enemydata[targetId].battleentity;
                targetPos = targetEntity.transform.position + battle.enemydata[targetId].cursoroffset + new Vector3(0f, targetEntity.height - 1f);
            }

            yield return ThrowWhirly(entity, usedByEnemy, (int)NewItem.WhirlySeed, targetPos);

            if (usedByEnemy)
            {
                battle.DoDamage(null, ref MainManager.instance.playerdata[battle.playertargetID], damage, null, overrides.ToArray(), battle.commandsuccess);
                if (MainManager.instance.playerdata[battle.playertargetID].hp > 0 && !battle.commandsuccess)
                    Instance.TryDizzy(null, ref MainManager.instance.playerdata[battle.playertargetID], dizzyTurns, dizzyResPierce);
            }
            else
            {
                battle.DoDamage(null, ref battle.enemydata[targetId], damage, null, overrides.ToArray(), false);
                if (battle.enemydata[targetId].hp > 0)
                    Instance.TryDizzy(null, ref battle.enemydata[targetId], dizzyTurns, dizzyResPierce);
            }
        }

        IEnumerator DoWhirlaRang(EntityControl entity, bool usedByEnemy)
        {
            int damage = 4;
            List<DamageOverride> overrides = new List<DamageOverride>() {
                (DamageOverride)NewDamageOverride.FlipNoPierce,
                (DamageOverride)NewDamageOverride.Pierce1,
                (DamageOverride)NewDamageOverride.Pierce1,
                (DamageOverride)NewDamageOverride.Pierce1
            };

            entity.animstate = 28;
            MainManager.PlaySound("Toss");
            yield return EventControl.tenthsec;

            Vector3 itemPos;
            Vector3 targetPos;
            int targetId = 0;
            EntityControl targetEntity;
            if (usedByEnemy)
            {
                targetEntity = battle.playertargetentity;
                targetPos = targetEntity.transform.position + new Vector3(0, 1, -0.1f);
                itemPos = entity.transform.position + new Vector3(-0.5f, 2f, -0.1f) + Vector3.up * entity.height;
            }
            else
            {
                MainManager.BattleData targetData = MainManager.battle.avaliabletargets[battle.target];
                targetEntity = targetData.battleentity;
                itemPos = entity.transform.position + MainManager.instance.playerdata[battle.currentturn].cursoroffset - Vector3.up;
                targetId = targetEntity.battleid;
                targetPos = targetEntity.transform.position + battle.enemydata[targetId].cursoroffset + new Vector3(0f, targetEntity.height - 1f);
            }

            SpriteRenderer whirlarang = MainManager.NewSpriteObject(itemPos, null, MainManager.itemsprites[0, (int)NewItem.WhirlaRang]);

            Vector3 startPos = whirlarang.transform.position + new Vector3(0.5f, 0.5f);
            Vector3 midPos = Vector3.Lerp(startPos, targetPos, 0.5f) + new Vector3(0f, 0f, -5f);

            Entity_Ext entityExt = null;
            SpriteRenderer stolenItem = null;
            int stolenItemId = -1;

            if (!usedByEnemy)
            {
                entityExt = Entity_Ext.GetEntity_Ext(targetEntity);
                if (entityExt.itemId != -1)
                {
                    stolenItem = Instantiate(entityExt.item);
                    stolenItemId = entityExt.itemId;
                    Destroy(entityExt.item.gameObject);
                }
            }
            else if (usedByEnemy && MainManager.instance.items[0].Count > 0)
            {
                entityExt = Entity_Ext.GetEntity_Ext(entity);
                entityExt.itemId = -1;
                stolenItemId = MainManager.instance.items[0]
                    [UnityEngine.Random.Range(0, MainManager.instance.items[0].Count())];
                MainManager.instance.items[0].Remove(stolenItemId);

                stolenItem = MainManager.NewSpriteObject(itemPos, null, MainManager.itemsprites[0, stolenItemId]);
            }

            if (stolenItem != null)
                stolenItem.enabled = false;
            MainManager.PlaySound("Woosh", 8, 1.1f, 1f, true);

            float a = 0f;
            float b = 40f;
            bool hit = false;
            do
            {
                whirlarang.transform.position = MainManager.BeizierCurve3(startPos, targetPos, midPos, Mathf.Clamp01(hit ? ((b - a) / 20f) : (a / 20f)));
                whirlarang.transform.localEulerAngles = new Vector3(80f, 0f, whirlarang.transform.localEulerAngles.z - MainManager.framestep * 20f);
                if (!hit && a >= 20f)
                {
                    if (usedByEnemy)
                        battle.DoDamage(null, ref MainManager.instance.playerdata[battle.playertargetID], damage, null, overrides.ToArray(), battle.commandsuccess);
                    else
                        battle.DoDamage(null, ref MainManager.battle.enemydata[targetId], damage, null, overrides.ToArray(), false);

                    if (stolenItem != null)
                    {
                        stolenItem.enabled = true;
                    }

                    midPos = new Vector3(0f, 0f, 5f);
                    hit = true;
                }

                if (hit && stolenItem != null)
                {
                    stolenItem.transform.position = whirlarang.transform.position + Vector3.up * 0.1f;
                }
                a += MainManager.TieFramerate(1f);
                yield return null;
            }
            while (a < b);

            if (stolenItem != null)
            {
                Vector3 offset;

                if (usedByEnemy)
                    offset = battle.enemydata[entity.battleid].cursoroffset + entity.height*Vector3.up;
                else
                    offset = MainManager.instance.playerdata[battle.currentturn].cursoroffset;

                stolenItem.transform.parent = MainManager.battle.battlemap.transform;
                stolenItem.transform.position = entity.transform.position + offset - Vector3.forward * 0.1f;
                entity.animstate = (int)MainManager.Animations.ItemGet;
                MainManager.PlaySound("ItemGet0");
            }

            Destroy(whirlarang.gameObject);
            MainManager.StopSound(8, 0.1f);

            if (stolenItem != null)
            {
                yield return EventControl.halfsec;
                Destroy(stolenItem.gameObject);

                if (!usedByEnemy)
                {
                    if (MainManager.instance.items[0].Count < MainManager.instance.maxitems)
                    {
                        MainManager.instance.items[0].Add(stolenItemId);
                    }
                    else
                    {
                        MainManager.PlaySound("Fail");
                        entity.animstate = (int)MainManager.Animations.Angry;
                        yield return EventControl.quartersec;
                    }
                    entityExt.itemId = -1;
                    if (battle.caller != null)
                        NPCControl_Ext.GetNPCControl_Ext(battle.caller).items[targetId] = -1;
                }
                else
                {
                    if (entityExt.itemId == -1)
                    {
                        if (battle.caller != null)
                        {
                            var npcControlExt = NPCControl_Ext.GetNPCControl_Ext(battle.caller);
                            npcControlExt.usedItem[entity.battleid] = false;
                            npcControlExt.items[entity.battleid] = stolenItemId;
                        }
                        entityExt.CreateItem(stolenItemId);
                    }
                }
            }

            if (!usedByEnemy && MainManager.instance.items[0].Count < MainManager.instance.maxitems)
            {
                yield return EventControl.quartersec;

                itemPos = entity.transform.position + new Vector3(0f, 2.5f, -0.1f);
                SpriteRenderer whirlySeed = MainManager.NewSpriteObject(itemPos, null, MainManager.itemsprites[0, (int)NewItem.WhirlySeed]);
                MainManager.instance.items[0].Add((int)NewItem.WhirlySeed);
                entity.animstate = (int)MainManager.Animations.ItemGet;
                MainManager.PlaySound("ItemGet0");
                yield return EventControl.halfsec;
                Destroy(whirlySeed.gameObject);
            }

            yield return EventControl.quartersec;
        }

        IEnumerator DoInkBubbleItem(EntityControl entity, bool usedByEnemy, NewItem item)
        {
            battle.dontusecharge = true;
            yield return EventControl.tenthsec;

            int targetId = 0;
            Vector3 targetPos;

            if (usedByEnemy)
            {
                targetPos = battle.playertargetentity.transform.position + new Vector3(0, 1, -0.1f);

                ItemUsage effect = item == NewItem.InkyBrew ? ItemUsage.GradualHP : ItemUsage.GradualTP;
                int turns = 4;

                yield return DoItemEffect(effect, turns, battle.actionid, -1, (MainManager.Items)item);

                if (item == NewItem.BubbleHoney)
                {
                    int charge = 1;
                    yield return DoItemEffect(ItemUsage.ChargeUp, charge, battle.actionid, -1, (MainManager.Items)item);
                }
            }
            else
            {
                MainManager.BattleData targetData = MainManager.battle.avaliabletargets[battle.target];
                targetId = targetData.battleentity.battleid;
                var targetEntity = battle.enemydata[targetId].battleentity;
                targetPos = targetEntity.transform.position + battle.enemydata[targetId].cursoroffset + new Vector3(0f, targetEntity.height - 1f);

                ItemUsage effect = item == NewItem.InkyBrew ? ItemUsage.GradualHP : ItemUsage.GradualTP;
                int turns = 4;

                MainManager.DoItemEffect(effect, turns, battle.currentturn);
                MainManager.PlayParticle("MagicUp", null, MainManager.instance.playerdata[battle.currentturn].battleentity.transform.position);
                yield return EventControl.halfsec;

                if (item == NewItem.BubbleHoney)
                {
                    int charge = 1;
                    MainManager.DoItemEffect(ItemUsage.ChargeUp, charge, battle.currentturn);
                    MainManager.PlaySound("StatUp");
                    battle.StartCoroutine(battle.StatEffect(MainManager.instance.playerdata[battle.currentturn].battleentity, 4));
                    yield return EventControl.halfsec;
                }
            }

            Color inkColor = new Color(0.54f, 0, 0.78f, 1);
            yield return AI.ThrowBubble(inkColor, entity.transform.position + Vector3.up, targetPos + Vector3.up, 60f, 5, Vector3.one * 0.5f);

            if (usedByEnemy)
                Instance.ApplyStatus((MainManager.BattleCondition)NewCondition.Paintball, ref MainManager.instance.playerdata[battle.playertargetID], 1, "Shield", 0.8f, 1, "InkGet", targetPos, Vector3.one);
            else
                Instance.ApplyStatus((MainManager.BattleCondition)NewCondition.Paintball, ref battle.enemydata[targetId], 1, "Shield", 0.8f, 1, "InkGet", targetPos, Vector3.one);
            yield return new WaitForSeconds(0.75f);
        }

        IEnumerator DoSpiroll(EntityControl entity, bool enemy, int target = 0)
        {
            int tpRestore = 9;
            int hpRestore = 3 + 
                (enemy ? 0 : BadgeHowManyEquipped((int)BadgeTypes.HealPlus, MainManager.instance.playerdata[battle.currentturn].trueid));
            int dizzyTurns = 3;
  
            int called = enemy ? actionID : battle.currentturn;
            target = enemy ? target : battle.target;

            battle.Heal(ref enemy ? ref battle.enemydata[target] : ref MainManager.instance.playerdata[target], hpRestore);
            yield return EventControl.quartersec;

            if (enemy)
            {
                RecoverEnemyTp(tpRestore, target);

                if(battle.enemydata[target].animid != (int)NewEnemies.JumpAnt)
                    yield return SwitchEnemyPos(called, target);
            }
            else
            {
                RecoverPlayerTP(tpRestore, MainManager.instance.playerdata[target]);
                yield return SwitchPlayerPos(called, target);
            }

            startPosition = entity.transform.position;

            if(called == target)
            {
                if(enemy)
                    TryDizzy(null, ref battle.enemydata[target], dizzyTurns);
                else
                    TryDizzy(null, ref MainManager.instance.playerdata[target], dizzyTurns);

                yield return EventControl.halfsec;
            }
        }

        IEnumerator SwitchEnemyPos(int called, int targeted)
        {
            MainManager.PlaySound("Switch");
            Vector3 spin = new Vector3(0f, -20f);
            battle.enemydata[targeted].battleentity.spin = spin;
            battle.enemydata[called].battleentity.spin = spin;

            yield return EventControl.thirdsec;
            yield return EventControl.tenthsec;

            Vector3 temp = battle.enemydata[called].battleentity.transform.position;
            battle.enemydata[called].battleentity.transform.position = battle.enemydata[targeted].battleentity.transform.position;
            battle.enemydata[called].battlepos = battle.enemydata[targeted].battleentity.transform.position;

            battle.enemydata[targeted].battlepos = temp;
            battle.enemydata[targeted].battleentity.transform.position = temp;

            battle.enemydata[targeted].battleentity.spin = Vector3.zero;
            battle.enemydata[called].battleentity.spin = Vector3.zero;

            battle.UpdateConditionIcons();
            yield return null;
            battle.UpdateAnim();
        }

        IEnumerator SwitchPlayerPos(int called, int targeted)
        {
            MainManager.PlaySound("Switch");
            for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
            {
                if (i == called || i == targeted)
                {
                    MainManager.instance.playerdata[i].battleentity.spin = new Vector3(0f, -20f, 0f);
                }
            }
            yield return EventControl.thirdsec;

            called = Array.IndexOf(battle.partypointer, called);
            targeted = Array.IndexOf(battle.partypointer, targeted);

            int temp = battle.partypointer[called];
            battle.partypointer[called] = battle.partypointer[targeted];
            battle.partypointer[targeted] = temp;

            yield return EventControl.tenthsec;

            for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
            {
                MainManager.instance.playerdata[i].battleentity.overrridejump = true;
                MainManager.instance.playerdata[i].battleentity.spin = Vector3.zero;
                MainManager.instance.playerdata[battle.partypointer[i]].battleentity.transform.position = battle.partypos[i];
                MainManager.instance.playerdata[i].pointer = battle.partypointer[i];
                if (MainManager.instance.playerdata[i].battleentity.deathcoroutine != null)
                {
                    MainManager.instance.playerdata[i].battleentity.StopCoroutine(MainManager.instance.playerdata[i].battleentity.deathcoroutine);
                    MainManager.instance.playerdata[i].battleentity.deathcoroutine = null;
                    MainManager.instance.playerdata[i].battleentity.animstate = 18;
                }
            }
            yield return null;
            if(called != targeted)
            {
                Dizzy.DoCarousel(battle.partypointer[targeted]);
                Dizzy.DoCarousel(battle.partypointer[called]);
            }
            battle.UpdateConditionIcons();
            yield return null;
            battle.UpdateAnim();
        }

        void FixEnemyDiedOnItemUse()
        {
            bool enemyDied = false;
            for (int i = 0; i < battle.enemydata.Length; i++)
            {
                if (battle.enemydata[i].hp <= 0)
                {
                    enemyDied = true;
                }
            }

            if (battle.enemydata[actionID].hitaction)
            {
                battle.enemy = false;
            }

            if (enemyDied)
            {
                //abomb user did not die
                if (battle.enemydata[actionID].hp > 0)
                {
                    //im not sure what to do here, the main problem is reorg getting called in endenemyturn, im scared to remove it so
                    //im just doing a fake endenemyturn after the abomb use without the reorg
                    //best solution is probably adding a if in endenemyturn to not reorg if an enemy died here.
                    if (!MainManager.BadgeIsEquipped(11) && !battle.enemydata[actionID].hitaction && !battle.enemydata[actionID].notired)
                    {
                        battle.enemydata[actionID].tired++;
                    }
                    if (!battle.enemydata[actionID].hitaction)
                    {
                        battle.enemydata[actionID].cantmove++;
                    }

                    battle.enemydata[actionID].hitaction = false;
                    battle.enemydata[actionID].blockTimes = 0;
                    battle.RefreshAllData();
                }
                battle.selfsacrifice = true;
            }
            if(actionID >= 0 && actionID < battle.enemydata.Length)
                battle.enemydata[actionID].hitaction = false;

            //this is to prevent all enemies from not attacking if its a firststrike and an enemy gets his hitaction activated by abomb
            if (battle.firststrike)
            {
                for (int i = 0; i < battle.enemydata.Length; i++)
                {
                    battle.enemydata[i].hitaction = false;
                }
            }
        }

        IEnumerator DoFireballProj(int targetId, Transform fireball, bool targetIsEnemy, int damage)
        {
            EntityControl entity = targetIsEnemy ? battle.enemydata[targetId].battleentity : MainManager.instance.playerdata[targetId].battleentity;

            yield return MainManager.ArcMovement(fireball.gameObject, fireball.position, entity.transform.position + Vector3.up * entity.height, new Vector3(0, 0, 20), 10, 30, true);

            DamageOverride[] overrides = new[] { (DamageOverride)NewDamageOverride.Magic };
            if (targetIsEnemy)
            {
                battle.DoDamage(null, ref battle.enemydata[targetId], damage, AttackProperty.Fire, overrides, false);
            }
            else
            {
                battle.DoDamage(null, ref MainManager.instance.playerdata[targetId], damage, AttackProperty.Fire, overrides, false);
            }
        }

        IEnumerator DoRainDance(EntityControl entity)
        {
            int target = battle.target;
            EntityControl friend = MainManager.instance.playerdata[target].battleentity;
            EntityControl[] bugs = { entity, friend };

            battle.dontusecharge = true;

            yield return EventControl.tenthsec;
            var data = new float[] { 6, 1f };
            MainManager.battle.StartCoroutine(battle.DoCommand(275f, ActionCommands.SequentialKeys, data));
            GameObject rainCloud = Instance.CreateRainCloud(entity.transform.position + new Vector3(-1, 7, 0), entity, 180f);
            ParticleSystem rain = rainCloud.GetComponentInChildren<ParticleSystem>();

            yield return null;

            friend.flip = false;
            friend.animstate = (int)MainManager.Animations.ItemGet;

            for (int i = 0; i < bugs.Length; i++)
            {
                bugs[i].LockRigid(true);
                bugs[i].overrideanim = true;
                bugs[i].overrridejump = true;
            }

            Coroutine[] jumpRoutines = new Coroutine[2];
            Vector3[] basePositions = { bugs[0].transform.position, bugs[1].transform.position };
            while (MainManager.battle.doingaction)
            {
                for (int i = 0; i < bugs.Length; i++)
                {
                    bugs[i].flip = !bugs[i].flip;
                    bugs[i].animstate = (int)MainManager.Animations.ItemGet;
                    if (jumpRoutines[i] == null)
                    {
                        Vector3 targetPos = UnityEngine.Random.Range(0, 2) == 0 ? Vector3.right : Vector3.left;
                        jumpRoutines[i] = StartCoroutine(DoRainJump(jumpRoutines, i, bugs[i], basePositions[i] + targetPos, 30f));
                    }
                    yield return EventControl.tenthsec;
                }
                yield return EventControl.halfsec;
            }
            for (int i = 0; i < bugs.Length; i++)
            {
                bugs[i].flip = true;
                bugs[i].animstate = (int)MainManager.Animations.ItemGet;
                StartCoroutine(DoRainJump(jumpRoutines, i, bugs[i], basePositions[i], 30f));
            }
            yield return EventControl.sec;

            for (int i = 0; i < bugs.Length; i++)
            {
                bugs[i].overrideanim = false;
                bugs[i].overrridejump = false;
                bugs[i].transform.position = basePositions[i];
                bugs[i].LockRigid(false);
                bugs[i].animstate = (int)MainManager.Animations.ItemGet;
            }

            rain.Play();
            MainManager.PlaySound("Water0", 1.2f, 0.5f);
            yield return EventControl.halfsec;

            MainManager.PlaySound("Heal");
            MainManager.PlaySound("Heal3");
            int healAmount = Mathf.FloorToInt(battle.barfill * 4) + MainManager.BadgeHowManyEquipped(74, MainManager.instance.playerdata[battle.currentturn].trueid);
            healAmount = Mathf.Clamp(healAmount, 1, 99);

            for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
            {
                if (MainManager.instance.playerdata[i].hp > 0)
                {
                    battle.Heal(ref MainManager.instance.playerdata[i], healAmount, false);
                    Instance.CureNegativeStatus(ref MainManager.instance.playerdata[i]);
                    MainManager.PlayParticle("MagicUp", MainManager.instance.playerdata[i].battleentity.transform.position);
                }
            }
            yield return EventControl.sec;
            StartStylishTimer(3, 20);
            MainManager.StopSound("Water0");
            yield return EventControl.halfsec;
            yield return DestroyRainCloud(rainCloud, entity);
            battle.MultiSkillMove(new int[] { battle.currentturn, target });
        }

        IEnumerator DoRainJump(Coroutine[] coroutines, int index, EntityControl bug, Vector3 targetPos, float time)
        {
            bug.PlaySound("Jump");
            yield return StartCoroutine(MainManager.ArcMovement(bug.gameObject, targetPos, 4, time));
            bug.spin = new Vector3(0, 20, 0);
            yield return EventControl.quartersec;
            bug.spin = Vector3.zero;
            coroutines[index] = null;
        }

        public GameObject CreateRainCloud(Vector3 position, EntityControl entity, float growTime)
        {
            GameObject rainCloud = Instantiate(MainManager_Ext.assetBundle.LoadAsset<GameObject>("Clouds"));
            rainCloud.transform.position = position;
            Sprite cloudSprite = Resources.Load<Sprite>("sprites/particles/sprite-smoke-sheet");
            foreach (Transform cloud in rainCloud.transform)
            {
                if (cloud.name == "Cloud")
                {
                    var sr = cloud.gameObject.AddComponent<SpriteRenderer>();
                    sr.sprite = cloudSprite;
                    cloud.localScale = Vector3.zero;
                    entity.StartCoroutine(GrowCloud(cloud.gameObject, growTime, Vector3.one, sr, Color.gray, true));
                }
            }
            return rainCloud;
        }

        IEnumerator GrowCloud(GameObject cloud, float endTime, Vector3 endScale, SpriteRenderer renderer, Color targetColor, bool start)
        {
            float a = 0;
            Color startColor = renderer.color;
            Vector3 startScale = cloud.transform.localScale;
            do
            {
                cloud.transform.localScale = Vector3.Lerp(startScale, endScale, a / endTime);
                renderer.color = Color.Lerp(startColor, targetColor, a / endTime);
                a += MainManager.TieFramerate(1f);
                yield return null;

            } while (a < endTime);
            if (start)
            {
                SpriteBounce sb = cloud.AddComponent<SpriteBounce>();
                sb.frequency = 0.05f;
                sb.speed = 10f;
            }
        }

        public IEnumerator DestroyRainCloud(GameObject rainCloud, EntityControl entity)
        {
            foreach (Transform cloud in rainCloud.transform)
            {
                if (cloud.name == "Cloud")
                {
                    entity.StartCoroutine(GrowCloud(cloud.gameObject, 30f, Vector3.zero, cloud.gameObject.GetComponent<SpriteRenderer>(), new Color(0.5f, 0.5f, 0.5f, 0), false));
                }
            }
            yield return EventControl.halfsec;
            Destroy(rainCloud);
        }

        IEnumerator DoPointSwap(EntityControl entity, bool usedByEnemy, int target = -1)
        {
            battle.dontusecharge = true;
            Vector3 targetPos = Vector3.zero;
            int enemyTarget = 0;
            int playerTarget = 0;

            if (usedByEnemy)
            {
                if (target == -1)
                    enemyTarget = UnityEngine.Random.Range(0, battle.enemydata.Length);
                else
                    enemyTarget = target;
                targetPos = battle.enemydata[enemyTarget].battleentity.transform.position + entity.height * Vector3.up;
            }
            else
            {
                playerTarget = battle.target;
                targetPos = MainManager.instance.playerdata[playerTarget].battleentity.transform.position;
            }
            targetPos += Vector3.up * 3;
            GameObject holder = new GameObject("holder");
            holder.transform.position = targetPos;

            GameObject hpIcon = MainManager.NewUIObject("hpIcon", holder.transform, new Vector3(1, 0, 0), new Vector3(0.5f, 0.5f), MainManager.guisprites[24]);
            GameObject tpIcon = MainManager.NewUIObject("tpIcon", holder.transform, new Vector3(-1, 0, 0), new Vector3(0.75f, 0.75f), MainManager.guisprites[28]);
            MainManager.NewUIObject("swapIcon", holder.transform, new Vector3(0, 0), new Vector3(1, 1, 1), MainManager.guisprites[(int)NewGui.PointSwap]);

            yield return EventControl.halfsec;

            MainManager.PlaySound("Spin6");
            float a = 0;
            float b = 30f;
            Vector3 startRot = holder.transform.localEulerAngles;
            do
            {
                holder.transform.localEulerAngles = Vector3.Lerp(startRot, new Vector3(0, 0, 180), a / b);
                Quaternion inverseRotation = Quaternion.Inverse(transform.rotation);
                hpIcon.transform.rotation = inverseRotation;
                tpIcon.transform.rotation = inverseRotation;
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < b);

            yield return EventControl.halfsec;
            Destroy(holder);
            int hpAmount;
            if (usedByEnemy)
            {
                hpAmount = battle.enemydata[enemyTarget].hp;
                battle.enemydata[enemyTarget].hp = Mathf.Clamp(MainManager.instance.tp, 1, battle.enemydata[enemyTarget].maxhp);
            }
            else
            {
                hpAmount = MainManager.instance.playerdata[playerTarget].hp;
                MainManager.instance.playerdata[playerTarget].hp = Mathf.Clamp(MainManager.instance.tp, 1, MainManager.instance.playerdata[playerTarget].maxhp);
            }
            MainManager.PlaySound("HealPing");
            MainManager.instance.tp = Mathf.Clamp(hpAmount, 1, MainManager.instance.maxtp);
            yield return EventControl.quartersec;
        }

        static IEnumerator CheckNewSkills()
        {
            if (!battle.enemy && actionID >= 0)
            {
                EntityControl playerEntity = MainManager.instance.playerdata[battle.currentturn].battleentity;
                switch ((NewSkill)actionID)
                {
                    case NewSkill.SleepSchedule:
                        yield return Sleep.DoSleepSchedule(playerEntity);
                        break;
                    case NewSkill.VitiationLite:
                    case NewSkill.Vitiation:
                        yield return Instance.DoVitiation(playerEntity);
                        break;
                    case NewSkill.SeedlingWhistle:
                        yield return Instance.DoSeedlingStampede(false);
                        break;
                    case NewSkill.Steal:
                        yield return StealToss.DoStealToss(playerEntity);
                        break;
                    case NewSkill.NeedleSurge:
                        yield return NeedleSurge.DoNeedleSurge(playerEntity);
                        break;
                    case NewSkill.Lecture:
                        yield return Instance.DoLecture(playerEntity);
                        break;
                    case NewSkill.CordycepsLeech:
                        yield return Instance.DoCordycepsLeech(playerEntity);
                        break;

                    case NewSkill.ThrowableItems:
                        yield return Instance.DoPlayerThrowable(playerEntity);
                        break;

                    case NewSkill.InkTrap:
                        yield return Instance.DoInkTrap(false, playerEntity, MainManager.instance.playerdata[battle.currentturn]);
                        break;

                    case NewSkill.StickyBomb:
                        yield return Instance.DoStickyBomb(false, playerEntity, MainManager.instance.playerdata[battle.currentturn]);
                        break;

                    case NewSkill.RainDance:
                        yield return Instance.DoRainDance(playerEntity);
                        break;

                    case NewSkill.PointSwap:
                        yield return Instance.DoPointSwap(playerEntity, false);
                        break;

                    case NewSkill.FlameBomb:
                        yield return Instance.DoFlameBomb(playerEntity, false);
                        break;

                    case NewSkill.WhirlySeed:
                        yield return Instance.DoWhirlySeed(playerEntity, false);
                        break;

                    case NewSkill.WhirlyBomb:
                        yield return Instance.DoWhirlyBomb(playerEntity, false);
                        break;

                    case NewSkill.WhirlaRang:
                        yield return Instance.DoWhirlaRang(playerEntity, false);
                        break;

                    case NewSkill.InkBubbleItem:
                        yield return Instance.DoInkBubbleItem(playerEntity, false, (NewItem)battle.selecteditem);
                        break;

                    case NewSkill.Spiroll:
                        yield return Instance.DoSpiroll(playerEntity, false);
                        break;
                }
            }
        }

        static bool CheckRecharge() => CheckRecharge(MainManager.battle.currentturn);

        static bool CheckRecharge(int playerID)
        {
            var entityExt = Entity_Ext.GetEntity_Ext(MainManager.instance.playerdata[playerID].battleentity);
            if (MainManager.BadgeIsEquipped((int)Medal.Adrenaline, MainManager.instance.playerdata[playerID].trueid) && MainManager.instance.playerdata[playerID].hp <= 4 && !entityExt.adrenalineUsed)
            {
                entityExt.adrenalineUsed = true;
                MainManager.battle.StartCoroutine(battle.ItemSpinAnim(entityExt.entity.transform.position + Vector3.up, MainManager.itemsprites[1, (int)Medal.Adrenaline], true));
                return false;
            }

            if (BadgeIsEquipped((int)Medal.Recharge, MainManager.instance.playerdata[playerID].trueid) && MainManager.instance.playerdata[playerID].charge > 0)
            {
                MainManager.instance.playerdata[playerID].charge--;
                if (battle.currentturn == playerID)
                {
                    battle.dontusecharge = true;
                }
                return false;
            }
            return true;
        }


        public static bool CanUseCharge(int playerID)
        {
            if (MainManager.BadgeIsEquipped((int)Medal.Recharge, MainManager.instance.playerdata[playerID].trueid) ||
                MainManager.BadgeIsEquipped((int)Medal.ChargeGuard, MainManager.instance.playerdata[playerID].trueid))
            {
                if (MainManager.battle.currentturn == playerID)
                {
                    battle.dontusecharge = true;
                }
                return false;
            }
            return true;
        }

        static IEnumerator WaitForEnemyDrop()
        {
            battle.startdrop = true;
            while (battle.EnemyDropping())
            {
                yield return null;
            }
            battle.startdrop = false;
            if (battle.mainturn == null && battle.chompyattack == null)
            {
                battle.action = false;
            }
        }

        IEnumerator DoSeedlingStampede(bool usedByEnemy)
        {
            battle.dontusecharge = true;
            EntityControl[] seedlings = new EntityControl[40];
            Vector3 basePosition = new Vector3(usedByEnemy ? 20f : -20f, 0f);
            List<int> possibleIDs = new List<int>();
            possibleIDs.Add((int)MainManager.AnimIDs.Seedling);
            yield return null;

            switch (MainManager.map.areaid)
            {
                case MainManager.Areas.GoldenWay:
                case MainManager.Areas.GoldenSettlement:
                case MainManager.Areas.GoldenHills:
                case MainManager.Areas.ChomperCaves:
                    possibleIDs.Add((int)MainManager.AnimIDs.Acornling);
                    break;
                case MainManager.Areas.BarrenLands:
                case MainManager.Areas.TermiteCity:
                    possibleIDs.Add((int)MainManager.AnimIDs.Plumpling);
                    break;
                case MainManager.Areas.BugariaOutskirts:
                    possibleIDs.Add((int)MainManager.AnimIDs.Underling);
                    break;
                case MainManager.Areas.Desert:
                    possibleIDs.Add((int)MainManager.AnimIDs.Underling);
                    possibleIDs.Add((int)MainManager.AnimIDs.Cactus);
                    break;
                case MainManager.Areas.BanditHideout:
                case MainManager.Areas.StreamMountain:
                case MainManager.Areas.HoneyFactory:
                case MainManager.Areas.SandCastle:
                    possibleIDs.Add((int)MainManager.AnimIDs.Cactus);
                    break;
            }

            if ((NewMaps)MainManager.map.mapid == NewMaps.Pit100BaseRoom)
            {
                possibleIDs.Add((int)NewAnimID.Caveling +1);
                possibleIDs.Add((int)NewEnemies.Spineling);
            }

            MainManager.ShakeScreen(0.1f, -1f);
            MainManager.PlaySound("Whistle");
            for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
            {
                if (MainManager.instance.playerdata[i].hp > 0)
                {
                    MainManager.instance.playerdata[i].battleentity.overrideanim = true;
                    MainManager.instance.playerdata[i].battleentity.animstate = (int)MainManager.Animations.Surprized;
                    MainManager.instance.playerdata[i].battleentity.Emoticon(MainManager.Emoticons.Exclamation, 45);
                }
            }
            yield return EventControl.sec;
            bool gotGoldie = false;
            MainManager.PlaySound("Rumble", 0, 1.4f, 2f, true);

            for (int i = 0; i != seedlings.Length; i++)
            {
                int id;
                int goldieChance = MainManager.map.mapid == MainManager.Maps.SeedlingHaven ? 50 : 0;
                if (MainManager.BadgeIsEquipped((int)MainManager.BadgeTypes.Seedling))
                    goldieChance += 10;

                string name = "seedling" + i;
                if (UnityEngine.Random.Range(0, 200) >= goldieChance)
                {
                    id = possibleIDs[UnityEngine.Random.Range(0, possibleIDs.Count)];

                    if (id == (int)NewEnemies.Spineling)
                    {
                        name = "Spineling";
                        id = (int)MainManager.AnimIDs.Cactus;
                    }
                }
                else
                {
                    id = (int)MainManager.AnimIDs.GoldenSeedling;
                    gotGoldie = true;
                }

                Vector3 randomPos = new Vector3(UnityEngine.Random.Range(-5f, 5f), 0, UnityEngine.Random.Range(-2f, 2.5f));
                Vector3 seedPos = basePosition + randomPos;
                seedlings[i] = EntityControl.CreateNewEntity(name, id - 1, seedPos);

                seedlings[i].transform.parent = MainManager.battle.battlemap.transform;
                seedlings[i].height = 0;
                seedlings[i].gameObject.layer = 9;
                seedlings[i].flip = !usedByEnemy;
                seedlings[i].alwaysflip = !usedByEnemy;

                yield return null;
                seedlings[i].MoveTowards(new Vector3(usedByEnemy ? -15f : 15f, 0f, seedPos.z), 2f, 23, 0);
            }

            EntityControl farthestSeed = null;
            if (!usedByEnemy)
                farthestSeed = seedlings.OrderByDescending(t => t.transform.position.x).FirstOrDefault();
            else
                farthestSeed = seedlings.OrderBy(t => t.transform.position.x).FirstOrDefault();

            while (seedlings.Any(s => s.forcemove))
            {
                for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                {
                    if (MainManager.instance.playerdata[i].hp > 0)
                    {
                        bool seedPassed = false;

                        if (usedByEnemy)
                        {
                            seedPassed = farthestSeed.transform.position.x <= MainManager.instance.playerdata[i].battleentity.transform.position.x;
                        }
                        else
                        {
                            seedPassed = farthestSeed.transform.position.x >= MainManager.instance.playerdata[i].battleentity.transform.position.x;
                        }

                        if (seedPassed)
                        {
                            MainManager.instance.playerdata[i].battleentity.animstate = (int)MainManager.Animations.Hurt;
                            MainManager.instance.playerdata[i].battleentity.spin = new Vector3(0, 20);
                        }
                    }
                }

                foreach (var enemy in MainManager.battle.enemydata)
                {

                    bool seedPassed = false;

                    if (usedByEnemy)
                    {
                        seedPassed = farthestSeed.transform.position.x <= enemy.battleentity.transform.position.x;
                    }
                    else
                    {
                        seedPassed = farthestSeed.transform.position.x >= enemy.battleentity.transform.position.x;
                    }

                    if (enemy.position == BattleControl.BattlePosition.Ground && seedPassed)
                    {
                        enemy.battleentity.animstate = (int)MainManager.Animations.Hurt;
                        enemy.battleentity.spin = new Vector3(0, 20);
                    }
                }
                var randomSeed = seedlings[UnityEngine.Random.Range(0, seedlings.Length - 1)];
                if (randomSeed.transform.position.y <= 0 && randomSeed.height == 0)
                {
                    randomSeed.overrridejump = true;
                    randomSeed.Jump();
                }
                yield return null;
            }
            MainManager.screenshake = Vector3.zero;
            MainManager.StopSound("Rumble");
            foreach (var seedling in seedlings)
            {
                Destroy(seedling.gameObject);
            }

            for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
            {
                MainManager.instance.playerdata[i].battleentity.overrideanim = false;
                MainManager.instance.playerdata[i].battleentity.spin = Vector3.zero;
            }

            foreach (var enemy in MainManager.battle.enemydata)
            {
                enemy.battleentity.spin = Vector3.zero;
            }

            int damage = gotGoldie ? 8 : 5;

            if (!usedByEnemy)
            {
                for (int i = 0; i < MainManager.battle.enemydata.Length; i++)
                {
                    if (MainManager.battle.enemydata[i].hp > 0 && MainManager.battle.enemydata[i].position == BattleControl.BattlePosition.Ground)
                    {
                        battle.DoDamage(ref MainManager.battle.enemydata[i], damage, null);
                    }
                }
            }
            else
            {
                for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                {
                    if (MainManager.instance.playerdata[i].hp > 0)
                    {
                        battle.DoDamage(null, ref MainManager.instance.playerdata[i], damage, null, null, MainManager.battle.commandsuccess);
                    }
                }
            }

        }

        static int CheckNewItemAction()
        {
            if(battle.option >= 0 && battle.option < MainManager.instance.playerdata.Length)
                Instance.CheckMiniMegaMush(battle.selecteditem, ref MainManager.instance.playerdata[battle.option]);

            switch (battle.selecteditem)
            {
                case (int)Items.HardSeed:
                case (int)Items.PoisonDart:
                case (int)Items.NumbDart:
                case (int)NewItem.BeeBattery:
                case (int)Items.Ice:
                case (int)NewItem.WebWad:
                case (int)Items.FlameRock:
                case (int)NewItem.MysterySeed:
                case (int)NewItem.SucculentSeed:
                case (int)NewItem.SquashSeed:
                case (int)Items.SpicyBomb:
                case (int)Items.BurlyBomb:
                case (int)Items.PoisonBomb:
                case (int)Items.SleepBomb:
                case (int)Items.NumbBomb:
                case (int)Items.FrostBomb:
                case (int)NewItem.InkBomb:
                case (int)Items.ClearBomb:
                case (int)Items.CherryBomb:
                case (int)NewItem.CherryBomb2:
                case (int)NewItem.MysteryBomb:
                case (int)Items.Abombhoney:
                    return (int)NewSkill.ThrowableItems;

                case (int)NewItem.WhirlySeed:
                    return (int)NewSkill.WhirlySeed;

                case (int)NewItem.WhirlaRang:
                    return (int)NewSkill.WhirlaRang;

                case (int)NewItem.InkTrap:
                    return (int)NewSkill.InkTrap;

                case (int)NewItem.WhirlyBomb:
                    return (int)NewSkill.WhirlyBomb;
                case (int)NewItem.StickyBomb:
                    return (int)NewSkill.StickyBomb;
                case (int)NewItem.FlameBomb:
                    return (int)NewSkill.FlameBomb;

                case (int)NewItem.SeedlingWhistle:
                    return (int)NewSkill.SeedlingWhistle;

                case (int)NewItem.BubbleHoney:
                case (int)NewItem.InkyBrew:
                    return (int)NewSkill.InkBubbleItem;

                case (int)NewItem.PointSwap:
                    return (int)NewSkill.PointSwap;

                case (int)NewItem.Spiroll:
                    return (int)NewSkill.Spiroll;

            }
            return -1;
        }
        public static bool CanItemHitUnderground(int itemID)
        {
            if (itemID == (int)Items.LonglegSummoner ||
                itemID == (int)Items.CherryBomb ||
                itemID == (int)NewItem.CherryBomb2 ||
                itemID == (int)NewItem.MysteryBomb ||
                itemID == (int)NewItem.InkTrap)
            {
                return true;
            }
            return false;
        }

        //Weak stomach and flavor charger sprite when selecting player
        static string GetItemSelectSprite()
        {
            MainManager.BattleData player = MainManager.instance.playerdata[MainManager.battle.option];
            int playerid = player.trueid;
            string text = "|center|" + player.entityname;

            if (MainManager.battle.currentchoice == BattleControl.Actions.Item)
            {
                if (MainManager.BadgeIsEquipped(24, playerid) || MainManager.BadgeIsEquipped((int)Medal.FlavorCharger, playerid))
                {
                    text += " |size,1,0.6|";
                    if (MainManager.BadgeIsEquipped(24, playerid))
                        text += "|icon,184|";
                    if (MainManager.BadgeIsEquipped((int)Medal.FlavorCharger, playerid))
                        text += $"|icon,{(int)NewGui.FlavorCharger}|";
                }
            }
            return text;
        }

        //Vitiation Stuff
        public int realDamage = 0;


        static int SetRealDamage(int value)
        {
            Instance.realDamage += value;
            return value;
        }


        static IEnumerator CheckRevengarang()
        {
            var battle = MainManager.battle;
            Instance.entityAttacking = null;

            if (Instance.revengarangIsActive && battle.enemydata[actionID].position != BattlePosition.Underground)
            {
                EntityControl vi = MainManager.instance.playerdata[0].battleentity;
                int baseState = vi.animstate;
                if (!battle.IsStopped(MainManager.instance.playerdata[0]))
                {
                    vi.overrideanim = true;
                    vi.animstate = 105;
                }
                MainManager.PlaySound("Woosh", 8, 1.1f, 1f, true);
                GameObject beerang = Instantiate<GameObject>(Resources.Load("Prefabs/Objects/BeerangBattle") as GameObject);
                beerang.transform.position = vi.transform.position + Vector3.up;
                Vector3 targetPos = battle.enemydata[actionID].battleentity.sprite.transform.position + Vector3.up * 0.75f;
                Vector3 start;
                float a = 0;
                int hits = MainManager.BadgeIsEquipped((int)MainManager.BadgeTypes.Beemerang2) ? 2 : 1;

                for (int i = 0; i < hits; i++)
                {
                    float b = i == 0 ? 30f : 15f;
                    a = 0;
                    start = beerang.transform.position;
                    do
                    {
                        a += MainManager.framestep;
                        beerang.transform.position = MainManager.BeizierCurve3(start, targetPos, targetPos + Vector3.up * 2.5f + Vector3.back * 3f, a / b);
                        beerang.transform.localEulerAngles = new Vector3(80f, 0f, beerang.transform.localEulerAngles.z - MainManager.framestep * 20f);
                        yield return null;
                    }
                    while (a < b);

                    battle.DoDamage(null, ref battle.enemydata[actionID], Instance.revengarangDMG, i == 0 ? AttackProperty.Pierce : AttackProperty.NoExceptions, null, false);
                    Instance.revengarangDMG = Mathf.Clamp(Instance.revengarangDMG / 2, 1, 99);
                    if (i == 0 && hits > 1)
                    {
                        a = 0;
                        b = 15f;
                        start = beerang.transform.position;
                        do
                        {
                            a += MainManager.framestep;
                            beerang.transform.position = MainManager.BeizierCurve3(start, targetPos + Vector3.up * 3f, targetPos + Vector3.up * 2f + Vector3.forward * 2f, a / b);
                            beerang.transform.localEulerAngles = new Vector3(80f, 0f, beerang.transform.localEulerAngles.z - MainManager.framestep * 20f);
                            yield return null;
                        }
                        while (a < b);
                    }

                    AudioSource audioSource = MainManager.sounds[8];
                    audioSource.pitch += 0.07f;
                }

                Destroy(beerang);
                MainManager.StopSound(8, 0.1f);
                vi.overrideanim = false;
                vi.animstate = baseState;
                yield return EventControl.halfsec;

                if (battle.enemydata[actionID].hp <= 0)
                {
                    battle.selfsacrifice = true;
                }

            }
            Instance.revengarangIsActive = false;
        }

        public void CheckStrikeBlasters(BattleControl __instance, MainManager.BattleData target, int beforeDoDamageHp, Entity_Ext entity_Ext)
        {
            if (MainManager.BadgeIsEquipped((int)Medal.StrikeBlaster) && target.hp == 0 && target.eventondeath == -1 && !__instance.inevent && __instance.enemydata.Length > 1 && !entity_Ext.isPartner && !__instance.summonnewenemy)
            {
                if (!strikeBlasters.Where(s => s.entity == target.battleentity).Any())
                {
                    strikeBlasters.Add(new StrikeBlaster() { dmg = beforeDoDamageHp, entity = target.battleentity,
                        targetPos = target.battleentity.transform.position + Vector3.up*target.battleentity.height });
                    if (strikeBlasterManager == null)
                        strikeBlasterManager = StartCoroutine(ManageStrikeBlasters());
                }
            }
        }

        IEnumerator ManageStrikeBlasters()
        {
            for (int i = 0; i < strikeBlasters.Count; i++)
            {
                var strikeBlaster = strikeBlasters[i];
                Vector3 targetPos = strikeBlaster.targetPos;

                yield return new WaitUntil(() => strikeBlaster.entity == null || strikeBlaster.entity.dead);
                yield return new WaitUntil(() => !battle.summonnewenemy);

                yield return EventControl.halfsec;

                MainManager.PlayParticle("explosionsmall", targetPos);
                MainManager.PlaySound("Explosion");

                yield return EventControl.tenthsec;

                var hitEnemies = battle.enemydata.Where(e => e.hp > 0 && e.position != BattleControl.BattlePosition.Underground)
                                                .ToArray();

                if (!battle.inevent && battle.mainturn == null)
                {
                    if (hitEnemies.Length > 0)
                    {
                        int rest = strikeBlaster.dmg % hitEnemies.Length;
                        int damagePerEnemy = strikeBlaster.dmg / hitEnemies.Length;
                        for (int j = 0; j < hitEnemies.Length; j++)
                        {
                            int damageAmount = damagePerEnemy;
                            int id = hitEnemies[j].battleentity.battleid;

                            if (rest > 0)
                            {
                                damageAmount++;
                                rest--;
                            }

                            if (damageAmount > 0)
                            {
                                battle.DoDamage(null, ref battle.enemydata[id], damageAmount, BattleControl.AttackProperty.None, new DamageOverride[] { DamageOverride.NoFall }, false);
                            }
                        }
                    }

                    if (battle.checkingdead != null)
                        battle.StopCoroutine(battle.checkingdead);

                    battle.StartCoroutine(battle.CheckDead());
                    yield return EventControl.quartersec;
                }
            }
            strikeBlasterManager = null;
            yield return null;
        }


        class StrikeBlaster
        {
            public int dmg;
            public EntityControl entity;
            public Vector3 targetPos;
        }


        static void DoTrustFall()
        {
            var battle = MainManager.battle;
            Vector3 position = MainManager.instance.playerdata[battle.currentturn].battleentity.transform.position + new Vector3(0f, 0.5f);
            Instance.RemoveTP(-MainManager.instance.tp, position, position + Vector3.up * 2);
            MainManager.instance.tp = 0;

            Instance.trustFallTurn = battle.turns;

            battle.EndPlayerTurn();
            battle.CancelList();
        }

        static void CheckEnemyPos()
        {
            if (!MainManager.instance.inevent)
            {
                BattleControl_Ext.Instance.CheckEnemyItems();
            }
            else
            {
                if (!MainManager.instance.flags[901])
                {
                    var superbosses = MainManager_Ext.GetSuperBosses();
                    for (int i = 0; i < battle.enemydata.Length; i++)
                    {
                        if (superbosses.Contains(battle.enemydata[i].animid))
                        {
                            MainManager.instance.flags[901] = true;
                            break;
                        }
                    }
                }
            }

            //really fucking annoying but theres a light that they deactivate in the stratos delilah event before the fight
            if (MainManager.instance.flags[162])
            {
                int battleMap = battle.sdata.stage;

                if (battleMap == (int)MainManager.BattleMaps.UndergroundBar)
                {
                    MainManager.battle.battlemap.transform.GetChild(0).GetChild(2).gameObject.SetActive(false);
                }

                if (battleMap == (int)MainManager.BattleMaps.FinalBoss2)
                {
                    RenderSettings.fogColor = Color.Lerp(Color.black, Color.green, 0.25f);
                }

                if (battleMap == (int)MainManager.BattleMaps.AssociationHQ)
                {
                    if (battle.enemydata[0].animid == (int)MainManager.Enemies.HoloVi)
                        battle.enemydata[1].battlepos += new Vector3(0, 0, 0.85f);
                    RenderSettings.skybox = Resources.Load<Material>("materials/skybox/Black");
                }
            }

            if (MainManager.battle.enemydata.Any(e => e.animid == (int)NewEnemies.Mars) && MainManager.battle.enemydata.Length == 3)
            {
                MainManager.battle.enemydata[0].battlepos = new Vector3(0.6f, 0f, 0.35f);
                MainManager.battle.enemydata[1].battlepos = new Vector3(3.5f, 0f, 1.25f);
                MainManager.battle.enemydata[2].battlepos = new Vector3(5.4f, 0f, -0.8f);
            }

            if (MainManager.battle.enemydata.Any(e => e.animid == (int)NewEnemies.DynamoSpore) && MainManager.battle.enemydata.Length == 3)
            {
                MainManager.battle.enemydata[0].battlepos = new Vector3(0.9f, 0f, 0f);
                MainManager.battle.enemydata[1].battlepos = new Vector3(3.5f, 0f, 0.15f);
                MainManager.battle.enemydata[2].battlepos = new Vector3(6.2f, 0f, 0.3f);
            }

            if (MainManager.battle.enemydata.Any(e => e.animid == (int)NewEnemies.LeafbugShaman) && MainManager.battle.enemydata.Length == 3)
            {
                MainManager.battle.enemydata[1].basedef = MainManager.battle.enemydata[1].def;

                MainManager.battle.enemydata[0].battlepos = new Vector3(0.9f, 0f, 0f);
                MainManager.battle.enemydata[1].battlepos = new Vector3(4f, 0f, 0.15f);
                MainManager.battle.enemydata[2].battlepos = new Vector3(6.8f, 0f, 0.3f);
            }

            if (MainManager.battle.enemydata.Any(e => e.animid == (int)NewEnemies.JumpAnt) && MainManager.battle.enemydata.Length == 2)
            {
                var jumpAnt = MainManager.battle.enemydata.FirstOrDefault(e => e.animid == (int)NewEnemies.JumpAnt);
                Instance.jumpAntFightComp = jumpAnt.battleentity.gameObject.AddComponent<JumpAntFight>();

                MainManager.battle.enemydata[0].battlepos = new Vector3(3f, 0f, 0f);
                MainManager.battle.enemydata[1].battlepos = Instance.jumpAntFightComp.partnerPos;

                MainManager.battle.enemydata[1].position = BattlePosition.Flying;
                MainManager.battle.enemydata[1].battleentity.digging = false;
                MainManager.battle.enemydata[1].battleentity.height = 2;
                MainManager.battle.enemydata[1].battleentity.initialheight = 2;
            }

            if (MainManager.battle.enemydata.Any(e => e.animid == (int)NewEnemies.RedSeedling) && MainManager.battle.enemydata.Length == 2)
            {
                MainManager.SetCondition(MainManager.BattleCondition.AttackUp, ref MainManager.battle.enemydata[0], 999999);
                MainManager.SetCondition(MainManager.BattleCondition.DefenseUp, ref MainManager.battle.enemydata[1], 999999);
            }

            if (MainManager.lastevent == (int)NewEvents.JumpAntIntermission5 && MainManager.battle.enemydata.Length == 4)
            {
                MainManager.battle.enemydata[1].battlepos = new Vector3(2.9f, 0, 0.15f);
            }

            for (int i = 0; i < MainManager.battle.enemydata.Length; i++)
                battle.enemydata[i].battleentity.startpos = battle.enemydata[i].battlepos;
        }


        static int CheckStartState(int original_startState)
        {
            int state = Instance.startState;
            Instance.startState = -1;

            if (battle.enemy)
            {
                if (Entity_Ext.GetEntity_Ext(battle.enemydata[battle.actionid].entity).overrideDamageAnim)
                {
                    battle.enemydata[battle.actionid].entity.overrideanim = true;
                }
            }

            return state == -1 ? original_startState : state;
        }

        public static IEnumerator LerpPosition(float endTime, Vector3 startPos, Vector3 endPos, Transform obj)
        {
            yield return LerpStuff(endTime, startPos, endPos, obj, (startPosition, endPosition, transform, time, end) =>
            {
                obj.position = Vector3.Lerp(startPos, endPos, time / endTime);
            });
            obj.position = endPos;
        }

        public static IEnumerator LerpScale(float endTime, Vector3 startScale, Vector3 endScale, Transform obj)
        {
            yield return LerpStuff(endTime, startScale, endScale, obj, (startSize, endSize, transform, time, end) =>
            {
                transform.localScale = Vector3.Lerp(startSize, endSize, time / end);
            });
            obj.localScale = endScale;
        }

        public static IEnumerator LerpStuff(float endTime, Vector3 startPos, Vector3 endPos, Transform obj, Action<Vector3, Vector3, Transform, float, float> func)
        {
            float a = 0;
            do
            {
                func(startPos, endPos, obj, a, endTime);
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < endTime);
        }


        public static void StartStylishTimer(float startFrames, float endFrames, int stylishID = 0, float stylishGain = 0.1f, bool commandSuccess = true)
        {
            MainManager.battle.StartCoroutine(Instance.StylishTimer(startFrames, endFrames, stylishID, stylishGain, commandSuccess));
        }
        IEnumerator StylishTimer(float startFrames, float endFrames, int stylishID, float stylishGain, bool commandSuccess)
        {
            if (commandSuccess && !battle.commandsuccess)
            {
                yield break;
            }

            float a = 0f;
            Instance.failedStylish = false;

            EntityControl entity = Instance.entityAttacking;
            if (battle.chompyattack != null)
            {
                entity = battle.chompy;
            }
            else if (Instance.inPlayerDelayedProjs > -1)
            {
                entity = MainManager.instance.playerdata[Instance.inPlayerDelayedProjs].battleentity;
            }

            do
            {
                if (a >= 1)
                {
                    if (a < startFrames)
                    {
                        if (MainManager.GetKey(4, false))
                        {
                            Instance.failedStylish = true;
                            break;
                        }
                    }
                    else
                    {
                        if (MainManager.BadgeIsEquipped((int)Medal.TimingTutor))
                        {
                            entity.Emoticon(MainManager.Emoticons.Exclamation);
                        }

                        if (MainManager.GetKey(4, false))
                        {
                            Instance.failedStylish = false;
                            entity.Emoticon(MainManager.Emoticons.None);
                            yield return MainManager.battle.StartCoroutine(DoStylish(stylishID, stylishGain));
                            break;
                        }
                    }
                }
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < endFrames);
            entity.Emoticon(MainManager.Emoticons.None);
        }
        IStylish GetStylishType(int animid, int stylishID)
        {
            if (actionID == 5 || actionID == 26 || actionID == 27 || actionID == 31 || actionID == 46 || actionID == (int)NewSkill.RainDance)
                return new TeamStylish();

            switch (animid)
            {
                case 0:
                    return new ViStylish();
                case 1:
                    return new KabbuStylish();
                case 2:
                    return new LeifStylish();
            }

            return null;
        }
        IEnumerator DoStylish(int stylishID, float stylishGain)
        {
            Instance.inStylish = true;
            if (battle.chompyattack != null)
            {
                yield return DoChompyStylish(stylishGain);
            }
            else if (Instance.inPlayerDelayedProjs > -1)
            {
                yield return GetStylishType(Instance.inPlayerDelayedProjs, stylishID).DoStylish(-2, stylishID, stylishGain);
            }
            else
            {
                stylishCountThisAction++;
                yield return GetStylishType(Instance.entityAttacking.animid, stylishID).DoStylish(actionID, stylishID, stylishGain);
            }
            Instance.inStylish = false;
        }
        IEnumerator DoChompyStylish(float stylishGain)
        {
            EntityControl chompy = battle.chompy;
            StylishUtils.ShowStylish(1.2f, chompy, stylishGain);
            chompy.overrideflip = false;
            chompy.rigid.useGravity = true;
            chompy.animstate = (int)MainManager.Animations.Happy;
            chompy.Jump();
            chompy.spin = new Vector3(0, 20, 0);

            while (!chompy.onground)
            {
                yield return null;
            }
            yield return EventControl.halfsec;
            chompy.spin = Vector3.zero;
        }


        public static IEnumerator WaitStylish(float waitTime)
        {
            if (waitTime > 0)
                yield return new WaitForSeconds(waitTime);

            if (!Instance.failedStylish)
            {
                yield return new WaitUntil(() => !Instance.inStylish);
            }

            Instance.failedStylish = false;
        }

        public IEnumerator ShowStylishMessage(EntityControl entity, Vector3? off = null)
        {
            var battle = MainManager.battle;

            Vector3 up = Vector3.up * (Instance.entityAttacking == null ? 0 : Instance.entityAttacking.height);
            Vector3 offset = new Vector3(1f, 1.5f) + up + Vector3.forward * 10f;

            if (off != null)
                offset += off.Value;

            SpriteRenderer stylyshWord = MainManager.NewUIObject("word", battle.battlemap.transform, entity.transform.position + offset).AddComponent<SpriteRenderer>();
            stylyshWord.material.renderQueue = 50000;
            stylyshWord.transform.localScale = Vector3.zero;
            DialogueAnim dialogueAnim = stylyshWord.gameObject.AddComponent<DialogueAnim>();
            dialogueAnim.targetscale = Vector3.one * 0.5f;
            stylyshWord.sprite = MainManager_Ext.assetBundle.LoadAsset<Sprite>("Stylish");

            if (MainManager.BadgeIsEquipped(76))
            {
                stylyshWord.sprite = MainManager.battlemessage[3];
            }
            yield return EventControl.sec;
            dialogueAnim.targetscale = new Vector3(0.5f, 0f, 0.5f);
            dialogueAnim.shrink = true;
            Destroy(stylyshWord.gameObject, 1f);
            yield break;
        }

        static bool CheckTutorialStylish()
        {
            return Instance.inStylishTutorial;
        }

        public IEnumerator DoStylishTutorial(IStylish stylish)
        {
            Instance.inStylish = true;
            ButtonSprite button = new GameObject().AddComponent<ButtonSprite>().SetUp(4, -1, "", new Vector3(0f, -3f, 10f), Vector3.one, 1, MainManager.GUICamera.transform);
            MainManager.battle.DestroyHelpBox();
            yield return EventControl.tenthsec;
            while (!MainManager.GetKey(4))
            {
                if (button.basesprite != null)
                {
                    button.basesprite.color = Mathf.Sin(Time.time * 10f) * 10f > 0f ? Color.white : Color.gray;
                }
                yield return null;
            }
            Destroy(button.gameObject);
            battle.StartCoroutine(WaitTutorialStylish(stylish));
        }

        IEnumerator WaitTutorialStylish(IStylish stylish)
        {
            yield return stylish.DoStylish(-1, 0, 0.1f);
            Instance.inStylish = false;
        }


        static bool CanReUseItem()
        {
            return MainManager.instance.items[0].Count < MainManager.instance.maxitems;
        }

        public int CheckKineticEnergy(MainManager.BattleData attacker)
        {
            if (MainManager.BadgeIsEquipped((int)Medal.KineticEnergy, attacker.trueid))
            {
                int moves = -1 * attacker.cantmove + 1;
                return Math.Abs(moves / 3);
            }
            return 0;
        }

        public int CheckTeamGleam()
        {
            if (MainManager.BadgeIsEquipped((int)Medal.TeamGleam) && MainManager.instance.tp == MainManager.instance.maxtp)
                return 1;
            return 0;
        }

        public int CheckOddWarrior(BattleControl __instance, MainManager.BattleData attacker)
        {
            if (MainManager.BadgeIsEquipped((int)Medal.OddWarrior, attacker.trueid))
            {
                return (battle.turns + 1) % 2 == 0 ? -1 : 1;
            }
            return 0;
        }

        public int CalculateCleanseDamage(MainManager.BattleData target)
        {
            int liquidateMultiplier = 0;
            int infiniteStatus = 0;

            List<int> unclearables = new List<int>()
            {
                (int)MainManager.BattleCondition.EventStop,(int)MainManager.BattleCondition.Eaten,(int)MainManager.BattleCondition.Flipped,
                (int)MainManager.BattleCondition.Taunted, (int)MainManager.BattleCondition.Sturdy
            };

            if (MainManager.BadgeIsEquipped((int)Medal.PermanentInk))
            {
                unclearables.Add((int)MainManager.BattleCondition.Inked);
            }

            if (MainManager.BadgeIsEquipped((int)Medal.SturdyStrands))
            {
                unclearables.Add((int)MainManager.BattleCondition.Sticky);
            }

            int statusAmount = 0;
            foreach (var condition in target.condition)
            {
                if (condition[1] > 999)
                {
                    infiniteStatus++;
                    continue;
                }
                else if (!unclearables.Contains(condition[0]))
                {
                    if (condition[0] == (int)NewCondition.Vitiation)
                    {
                        liquidateMultiplier += condition[1] / 10;
                    }
                    else
                    {
                        liquidateMultiplier += condition[1];
                    }
                }

                if (!unclearables.Contains(condition[0]))
                {
                    statusAmount++;
                }
            }

            bool hasLiquidate = BadgeIsEquipped((int)Medal.Liquidate);

            if (target.charge > 0)
                statusAmount += hasLiquidate ? target.charge : 1;

            damageDeepCleanse = statusAmount * DAMAGE_DEEPCLEANSE;
            tpRegenCleanse = statusAmount * TP_REGEN_CLEANSE;
            if (hasLiquidate)
            {
                damageDeepCleanse = (DAMAGE_DEEPCLEANSE - 1) * liquidateMultiplier;
                tpRegenCleanse = (TP_REGEN_CLEANSE - 1) * liquidateMultiplier;
            }
            return statusAmount;
        }

        public void DealCleanseDamage(BattleControl __instance, ref MainManager.BattleData target)
        {
            bool deepCleaning = BadgeIsEquipped((int)Medal.DeepCleaning);
            bool rinseRegen = BadgeIsEquipped((int)Medal.RinseRegen);

            if (deepCleaning && damageDeepCleanse > 0)
            {
                int baseDamage = damageDeepCleanse;
                if (rinseRegen)
                    baseDamage = Mathf.FloorToInt(baseDamage / 2f);

                int id = battle.GetEnemyID(target.battleentity.transform);
                battle.DoDamage(ref __instance.enemydata[id], baseDamage, AttackProperty.Magic);
            }

            if (rinseRegen && tpRegenCleanse > 0)
            {
                int tpGain = tpRegenCleanse;
                if (deepCleaning)
                    tpGain = Mathf.CeilToInt(tpGain / 2f);

                RecoverPlayerTP(tpGain, MainManager.instance.playerdata[__instance.currentturn]);
            }
        }

        public IEnumerator ResetHoloID(BattleControl __instance)
        {
            holoSkillID = -1;
            if (oldAnimID != -1)
            {
                EntityControl playerEntity = MainManager.instance.playerdata[__instance.currentturn].battleentity;
                playerEntity.spin = new Vector3(0, 30, 0);
                yield return EventControl.quartersec;
                playerEntity.animid = oldAnimID;
                playerEntity.animstate = playerEntity.basestate;
                if (!MainManager.BadgeIsEquipped((int)MainManager.BadgeTypes.HoloCloak))
                {
                    playerEntity.hologram = false;
                    playerEntity.UpdateSpriteMat();
                }
                oldAnimID = -1;
                yield return EventControl.tenthsec;
                playerEntity.spin = Vector3.zero;
            }
        }

        static IEnumerator CheckNewEventDialogue(int id)
        {
            BattleControl battle = MainManager.battle;

            switch (id)
            {
                case (int)NewEventDialogue.MarsDeath:
                    int marsIndex = battle.EnemyInField((int)NewEnemies.Mars);

                    battle.enemydata[marsIndex].eventondeath = -1;
                    for (int i = 0; i < battle.enemydata.Length; i++)
                    {
                        if (i != marsIndex && battle.enemydata[i].battleentity.deathcoroutine == null)
                        {
                            battle.enemydata[i].battleentity.StartDeath();
                        }
                        battle.enemydata[i].hp = 0;
                    }

                    if (!battle.alreadyending)
                    {
                        battle.EndBattleWon(true, null);
                    }
                    yield return null;
                    break;

                case (int)NewEventDialogue.JesterSpitout:

                    int jesterId = battle.EnemyInField((int)NewEnemies.Jester);
                    bool dead = battle.enemydata[jesterId].hp <= 0;

                    if (dead)
                    {
                        battle.enemydata[jesterId].eventondeath = -1;
                    }

                    if (battle.enemydata[jesterId].ate != null)
                    {
                        yield return JesterAI.DoBugBullseye(battle.enemydata[jesterId].battleentity, jesterId, true);
                    }

                    if (dead)
                    {
                        battle.enemydata[jesterId].battleentity.iskill = true;
                        battle.enemydata[jesterId].battleentity.dead = true;
                        battle.enemydata[jesterId].battleentity.StartDeath();
                        yield return EventControl.sec;
                        yield return EventControl.halfsec;
                        if (!battle.alreadyending)
                        {
                            battle.EndBattleWon(true, null);
                        }
                    }
                    break;


                case (int)NewEventDialogue.PattonDeath:
                    EntityControl patton = battle.enemydata[battle.EnemyInField((int)NewEnemies.Patton)].battleentity;
                    patton.BreakIce();
                    patton.spin = new Vector3(0, 0, 20);
                    MainManager.PlaySound("ChargeDown2");
                    yield return patton.SlowSpinStop(new Vector3(0, 15), 60);

                    MainManager.PlaySound("Death3");
                    patton.spin = Vector3.zero;
                    patton.spritetransform.localEulerAngles = new Vector3(0, 0, -90);
                    patton.LockRigid(true);
                    patton.transform.localPosition += new Vector3(-1, 0.5f);

                    if (battle.AliveEnemies() == 0)
                        battle.EndBattleWon(true, null);
                    break;

                case (int)NewEventDialogue.JumpAntDeath:
                    int jumpAntId = battle.EnemyInField((int)NewEnemies.JumpAnt);
                    battle.SetData(jumpAntId, 4);
                    battle.enemydata[jumpAntId].data[3]++;
                    EntityControl jumpAnt = battle.enemydata[jumpAntId].battleentity;
                    jumpAnt.dead = true;
                    jumpAnt.BreakIce();
                    jumpAnt.animstate = (int)MainManager.Animations.Hurt;
                    jumpAnt.spin = new Vector3(0, 0, 20);
                    MainManager.PlaySound("ChargeDown2");
                    yield return jumpAnt.SlowSpinStop(new Vector3(0, 15), 60);

                    MainManager.PlaySound("Death3");
                    jumpAnt.spin = Vector3.zero;
                    jumpAnt.animstate = (int)MainManager.Animations.KO;
                    yield return EventControl.halfsec;
                    if (battle.AliveEnemies() == 0 && (Instance.jumpAntFightComp == null || !Instance.jumpAntFightComp.HasPartnerAlive()))
                        battle.EndBattleWon(true, null);
                    battle.ReorganizeEnemies();
                    break;

                case (int)NewEventDialogue.JumpAntRevive:
                    int reserveIndex = battle.reservedata.FindIndex(r => r.animid == (int)NewEnemies.JumpAnt);
                    EntityControl entity = battle.reservedata[reserveIndex].battleentity;

                    entity.startpos = entity.transform.position;
                    MainManager.PlaySound("ItemGet0");
                    var miracleSprite = MainManager.NewSpriteObject(entity.transform.position + Vector3.up * 2, battle.battlemap.transform, MainManager.itemsprites[1, (int)MainManager.BadgeTypes.MiracleMatter]);
                    yield return EventControl.halfsec;
                    Destroy(miracleSprite.gameObject);
                    entity.basestate = (int)MainManager.Animations.BattleIdle;
                    entity.overrideanim = true;
                    entity.animstate = 116;
                    yield return new WaitForSeconds(2.5f);
                    MainManager.PlaySound(MainManager_Ext.assetBundle.LoadAsset<AudioClip>("2up"));

                    battle.ReviveEnemy(reserveIndex, 0.2f, true, true);
                    entity.animstate = (int)MainManager.Animations.BattleIdle;
                    int index = battle.enemydata.Length - 1;
                    battle.enemydata[index].isdefending = false;
                    battle.enemydata[index].defenseonhit = 0;
                    battle.UpdateEntities();
                    yield return EventControl.halfsec;
                    break;

                case (int)NewEventDialogue.StylishTutorial:
                    yield return Instance.DoStylishTutorialEvent();
                    break;
            }
        }

        IEnumerator DoStylishTutorialEvent()
        {
            battle.StartCoroutine(MainManager.SetText(MainManager.commondialogue[203], true, Vector3.zero, MainManager.instance.playerdata[1].battleentity.transform, null));
            yield return new WaitUntil(() => !MainManager.instance.message);


            //wants to do the stylish tutorial
            if (MainManager.instance.option == 0)
            {
                MainManager.instance.flagvar[11] = 4;
                Instance.inStylishTutorial = true;
                battle.StartCoroutine(MainManager.SetText(MainManager.commondialogue[204], true, Vector3.zero, MainManager.instance.playerdata[1].battleentity.transform, null));
                yield return new WaitUntil(() => !MainManager.instance.message);

                battle.demomode = true;
                battle.target = 0;
                battle.avaliabletargets = battle.enemydata;
                battle.currentturn = 0;

                //vi stylishes
                battle.StartCoroutine(battle.DoAction(MainManager.instance.playerdata[0].battleentity, -1));
                yield return new WaitUntil(() => !battle.action);

                var startAngle = MainManager.instance.playerdata[1].battleentity.spritetransform.eulerAngles;

                battle.StartCoroutine(MainManager.SetText(MainManager.commondialogue[208], true, Vector3.zero, MainManager.instance.playerdata[0].battleentity.transform, null));
                yield return new WaitUntil(() => !MainManager.instance.message);

                //kabbu stylishes
                battle.currentturn = 1;
                battle.StartCoroutine(battle.DoAction(MainManager.instance.playerdata[1].battleentity, -1));
                yield return new WaitUntil(() => !battle.action);

                MainManager.instance.playerdata[1].battleentity.spritetransform.eulerAngles = startAngle;
                MainManager.instance.playerdata[1].battleentity.animstate = (int)MainManager.Animations.BattleIdle;

                battle.StartCoroutine(MainManager.SetText(MainManager.commondialogue[205], true, Vector3.zero, MainManager.instance.playerdata[1].battleentity.transform, null));
                yield return new WaitUntil(() => !MainManager.instance.message);

                for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                {
                    MainManager.instance.playerdata[i].cantmove = 1;
                }
                battle.demomode = false;
                Instance.inStylishTutorial = false;
            }
            else
            {
                battle.StartCoroutine(MainManager.SetText(MainManager.commondialogue[210], true, Vector3.zero, MainManager.instance.playerdata[0].battleentity.transform, null));
                yield return new WaitUntil(() => !MainManager.instance.message);
            }
            MainManager.instance.flags[963] = true;
            MainManager.instance.flagvar[11] = 3;
        }

        public void CreateStylishBar()
        {
            if (stylishBarHolder == null)
            {
                stylishBarHolder = MainManager.NewUIObject("barholder", null, new Vector3(5.29f, 3.38f, 9.75f), new Vector3(1f, 1f, 1f), MainManager.guisprites[81]).GetComponent<SpriteRenderer>();
                stylishBar = MainManager.NewUIObject("bar", stylishBarHolder.transform, Vector3.zero, new Vector3(stylishBarAmount, 1f, 1f), MainManager.guisprites[82]).GetComponent<SpriteRenderer>();

                if (stylishReward == StylishReward.None)
                {
                    GetStylishReward();
                }
                else
                {
                    ChangeStylishRewardIcon();
                }
                stylishBarHolder.transform.localScale = Vector3.one * 0.35f;
                stylishBarHolder.transform.rotation = Quaternion.Euler(0, 0, 358);
                stylishBar.sortingOrder = 1;
                stylishBar.color = Color.yellow;
                stylishBarHolder.transform.parent = MainManager.instance.hud[0];
                stylishBarHolder.transform.localPosition = new Vector3(12.29f, -0.92f, -0.25f);
            }
        }

        IEnumerator StylishStarMovement(GameObject star, float amount)
        {
            yield return StartCoroutine(MainManager.ArcMovement(star, star.transform.position, stylishBar.transform.position + new Vector3(1, 0.5f), new Vector3(0, 0, 15), 5, UnityEngine.Random.Range(15, 30f), true));

            if (stylishBarAmount < 1)
            {
                float a = 0f;
                float b = 20f;
                stylishBarAmount = Mathf.Clamp(amount + stylishBarAmount, 0, 1);
                do
                {
                    stylishBar.transform.localScale = new Vector3(Mathf.Lerp(stylishBar.transform.localScale.x, stylishBarAmount, a / b), 1f, 1f);
                    a += MainManager.TieFramerate(1f);
                    yield return null;
                } while (a < b);
            }

            if (stylishBarAmount >= 1)
            {
                stylishBar.color = Color.green;
            }
        }

        float GetStylishShroomMultiplier()
        {
            float basePerShroomItem = 0.25f;
            return MainManager.instance.items[0].Count(i => GetMushroomTinyHuge(i) != -1) * basePerShroomItem;
        }

        public IEnumerator IncreaseStylishBar(float amount, EntityControl entity)
        {
            stylishRoutine++;
            if (MainManager.BadgeIsEquipped((int)Medal.StylishShroom))
            {
                amount = Mathf.Clamp(amount * (1f + GetStylishShroomMultiplier()), amount, 1);
            }

            SpriteRenderer[] stars = new SpriteRenderer[(int)(amount * 100)];
            float amountPerStar = amount / stars.Length;
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i] = new GameObject().AddComponent<SpriteRenderer>();
                stars[i].transform.position = entity.transform.position + MainManager.RandomVector(0.5f, 0.5f, 0.5f);
                stars[i].sprite = MainManager.guisprites[100];
                stars[i].material = MainManager.spritemat;
                stars[i].material.renderQueue = 50000;
                stars[i].gameObject.layer = 14;
                stars[i].material.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.65f);
                stars[i].transform.localScale = Vector3.one * 0.45f;
                StartCoroutine(StylishStarMovement(stars[i].gameObject, amountPerStar));
                yield return null;
            }

            yield return new WaitUntil(() => stars.All(s => s == null));
            stylishRoutine--;
        }

        void GetStylishReward()
        {
            Dictionary<StylishReward, int> rewards = new Dictionary<StylishReward, int>()
            {
                { StylishReward.HPRegen, 3},
                { StylishReward.TPRegen, 3},
                { StylishReward.Buff, 1},
                { StylishReward.Debuff, 1},
                { StylishReward.Berries, 1}
            };
            stylishReward = MainManager_Ext.GetWeightedResult(rewards);
            ChangeStylishRewardIcon();
        }

        void ChangeStylishRewardIcon()
        {
            if (rewardIcon != null)
            {
                Destroy(rewardIcon.gameObject);
            }

            Dictionary<StylishReward, Sprite> icons = new Dictionary<StylishReward, Sprite>()
            {
                { StylishReward.HPRegen, MainManager.guisprites[120]},
                { StylishReward.TPRegen, MainManager.guisprites[119]},
                { StylishReward.Buff, MainManager.guisprites[(int)NewGui.BuffStylish]},
                { StylishReward.Debuff, MainManager.guisprites[(int)NewGui.DebuffStylish]},
                { StylishReward.Berries, MainManager.guisprites[29]}
            };

            Vector3 scale = Vector3.one * 1.5f;

            if (stylishReward == StylishReward.Buff || stylishReward == StylishReward.Debuff)
                scale = Vector3.one * 2;

            rewardIcon = MainManager.NewUIObject("icon", stylishBarHolder.transform, new Vector3(8.8f, 0, -0.1f), scale, icons[stylishReward]).GetComponent<SpriteRenderer>();
            rewardIcon.sortingOrder = 2;
        }

        IEnumerator DoStylishReward()
        {
            stylishBarAmount = 0;
            switch (stylishReward)
            {
                case StylishReward.HPRegen:
                    for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                    {
                        if (MainManager.instance.playerdata[i].hp > 0)
                            battle.Heal(ref MainManager.instance.playerdata[i], 1, false);
                    }
                    break;
                case StylishReward.TPRegen:
                    battle.HealTP(3);
                    break;
                case StylishReward.Buff:
                    GetRandomPlayerBuff();
                    break;
                case StylishReward.Debuff:
                    GetRandomEnemyDebuff();
                    break;
                case StylishReward.Berries:

                    for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                    {
                        if (MainManager.instance.playerdata[i].hp > 0)
                        {
                            yield return DoFallingObject(MainManager.instance.playerdata[i].battleentity, MainManager.itemsprites[0, (int)MainManager.Items.MoneyBig]);
                            MainManager.PlaySound("Money");
                            MainManager.instance.money = Mathf.Clamp(MainManager.instance.money + 10, 0, 999);
                            MainManager.instance.showmoney = 250f;
                            break;
                        }
                    }
                    break;
            }
            stylishBar.transform.localScale = new Vector3(0, 1, 1);
            stylishBar.color = Color.yellow;
            GetStylishReward();

            yield return EventControl.halfsec;
        }

        IEnumerator DoFallingObject(EntityControl target, Sprite objectSprite)
        {
            SpriteRenderer fallingObject = new GameObject().AddComponent<SpriteRenderer>();
            fallingObject.transform.position = target.transform.position + Vector3.up * 10;
            fallingObject.sprite = objectSprite;
            fallingObject.material = MainManager.spritemat;
            fallingObject.material.renderQueue = 50000;
            fallingObject.gameObject.layer = 14;
            fallingObject.gameObject.AddComponent<SpinAround>().itself = new Vector3(0, 0, 15);
            yield return LerpPosition(30, fallingObject.transform.position, target.transform.position + target.height * Vector3.up, fallingObject.transform);
            Destroy(fallingObject.gameObject);
        }

        void GetRandomEnemyDebuff()
        {
            MainManager.BattleCondition[] debuffs = { BattleCondition.AttackDown, BattleCondition.DefenseDown };
            int[] enemyIndexes = battle.enemydata.Select((e, index) => new { e.hp, index })
            .Where(x => x.hp > 0)
            .Select(x => x.index)
            .ToArray();

            if (enemyIndexes.Length > 0)
            {
                int enemyID = enemyIndexes[UnityEngine.Random.Range(0, enemyIndexes.Length)];
                MainManager.BattleCondition debuff = debuffs[UnityEngine.Random.Range(0, debuffs.Length)];
                int arrow = debuff == BattleCondition.AttackDown ? 2 : 3;
                MainManager.SetCondition(debuff, ref battle.enemydata[enemyID], 2);
                MainManager.PlaySound("StatDown");
                battle.StartCoroutine(battle.StatEffect(battle.enemydata[enemyID].battleentity, arrow));
            }
        }

        void GetRandomPlayerBuff()
        {
            MainManager.BattleCondition[] buffs = { BattleCondition.AttackUp, BattleCondition.DefenseUp };
            int[] partyIndexes = MainManager.instance.playerdata.Select((p, index) => new { p.hp, index })
            .Where(x => x.hp > 0)
            .Select(x => x.index)
            .ToArray();

            int partyId = partyIndexes[UnityEngine.Random.Range(0, partyIndexes.Length)];
            MainManager.BattleCondition buff = buffs[UnityEngine.Random.Range(0, buffs.Length)];
            MainManager.SetCondition(buff, ref MainManager.instance.playerdata[partyId], 2);
            MainManager.PlaySound("StatUp");
            battle.StartCoroutine(battle.StatEffect(MainManager.instance.playerdata[partyId].battleentity, buff == BattleCondition.AttackUp ? 0 : 1));
        }

        public void HideStylishBar()
        {
            stylishBarHolder.gameObject.SetActive(false);
        }

        public bool GetTauntedBy(ref int __result, BattleControl __instance)
        {
            if (BattleControl_Ext.actionID >= 0 && __instance.enemy && BattleControl_Ext.actionID < __instance.enemydata.Length)
            {
                var entityExt = Entity_Ext.GetEntity_Ext(__instance.enemydata[BattleControl_Ext.actionID].battleentity);
                if (entityExt.tauntedBy != -1 && entityExt.tauntedBy < MainManager.instance.playerdata.Length && MainManager.instance.playerdata[entityExt.tauntedBy].hp > 0)
                {
                    __result = entityExt.tauntedBy;
                    return false;
                }
            }
            return true;
        }

        public void RemoveTP(int tp, Vector3 startPos, Vector3 endPos)
        {
            battle.ShowDamageCounter(3, Mathf.Abs(tp), startPos, endPos);
            MainManager.instance.tp = Mathf.Clamp(MainManager.instance.tp + tp, 0, 99);
            MainManager.PlaySound("Heal2", -1, 0.5f, 1f, false);
        }

        public bool IsStatusImmune(MainManager.BattleData target, MainManager.BattleCondition condition)
        {
            MainManager.BattleCondition[] conditions = new MainManager.BattleCondition[]
            { MainManager.BattleCondition.Poison, MainManager.BattleCondition.Fire, MainManager.BattleCondition.Freeze,
                MainManager.BattleCondition.Numb, MainManager.BattleCondition.Sleep, MainManager.BattleCondition.Taunted,
                MainManager.BattleCondition.DefenseDown, MainManager.BattleCondition.AttackDown, MainManager.BattleCondition.Inked,
                MainManager.BattleCondition.Sticky, (MainManager.BattleCondition)NewCondition.Dizzy };

            if (target.battleentity != null)
            {
                if (MainManager.HasCondition(MainManager.BattleCondition.Sturdy, target) > -1 && conditions.Contains(condition))
                {
                    return true;
                }

                if (!target.battleentity.playerentity && target.battleentity.animid == (int)NewAnimID.IronSuit 
                    && condition == BattleCondition.Fire && 
                    target.battleentity.GetComponent<IronSuit>().currentSuit == IronSuit.Suit.Heart)
                {
                    return true;
                }
            }
            return false;
        }

        public void GoToItemList()
        {
            battle.UpdateAnim();
            battle.currentaction = BattleControl.Pick.ItemList;
            battle.itemarea = AttackArea.None;
            battle.excludeself = false;
            MainManager.SetUpList(0, true, false);
            MainManager.listammount = 5;
            MainManager.ShowItemList(0, MainManager.defaultlistpos, true, false);
            battle.UpdateText();
        }

        public IEnumerator WaitForActionGourmet(int playerId)
        {
            yield return new WaitUntil(() => !MainManager.battle.action);
            bool isStopped = battle.IsStoppedLite(MainManager.instance.playerdata[playerId]);
            int aliveEnemies = battle.AliveEnemies();

            if (MainManager.instance.playerdata[playerId].hp > 0 && aliveEnemies > 0 && !isStopped)
            {
                BattleControl_Ext.Instance.gourmetItemUse--;
                BattleControl_Ext.Instance.GoToItemList();
            }
            else
            {
                BattleControl_Ext.Instance.gourmetItemUse = -1;
            }
        }

        public void AddDelProjsPlayer(GameObject obj, DelProjType type, int targetpos, int damage, int turnstohit, 
            int areadamage, BattleControl.AttackProperty? property, List<DamageOverride> overrides, float framespeed,
            MainManager.BattleData summonedby, string hitsound, string hitparticle, string whilesound, bool usesAtkBonuses,
            List<BattlePosition> positions
            )
        {
            DelayedProjectileData delayedProjectileData = default;
            delayedProjectileData.obj = obj;
            delayedProjectileData.calledby = summonedby;
            delayedProjectileData.damage = damage;
            delayedProjectileData.turns = turnstohit + 1;
            delayedProjectileData.framestep = framespeed;
            delayedProjectileData.position = targetpos;
            delayedProjectileData.deathparticle = hitparticle;
            delayedProjectileData.deathsound = hitsound;
            delayedProjectileData.whilesound = whilesound;
            delayedProjectileData.areadamage = areadamage;
            delayedProjectileData.property = property;
            delayedProjectileData.obj.transform.parent = battle.battlemap.transform;

            DelayedProjExtra extra = delayedProjectileData.obj.AddComponent<DelayedProjExtra>();
            extra.type = type;

            if (targetpos >= 0 && targetpos < battle.enemydata.Length)
                extra.targetEntity = battle.enemydata[targetpos].battleentity;

            if (overrides == null) 
                overrides = new List<DamageOverride>();
            overrides.Add((DamageOverride)NewDamageOverride.DelayedDamage);
            extra.overrides = overrides;

            extra.possiblePositions.AddRange(positions);
            extra.delProjData = delayedProjectileData;

            if (usesAtkBonuses)
                DamagePipelineHandler.BonusATKForDelProjs(ref delayedProjectileData, extra, false);
            delProjsPlayer.Add(extra);
        }

        public int HasDelProjOnEnemy(EntityControl enemy)
        {
            return delProjsPlayer.FirstOrDefault(i => i.targetEntity == enemy)?.delProjData.turns ?? -1;
        }

        IEnumerator DoDelProjPlayer()
        {
            if (delProjsPlayer.Count != 0 && MainManager.GetAlivePlayerAmmount() > 0 && battle.AliveEnemies() > 0)
            {
                bool any = false;
                battle.nonphyscal = true;

                List<DelayedProjExtra> projToRemove = new List<DelayedProjExtra>();
                for (int i = 0; i < delProjsPlayer.Count; i++)
                {
                    inPlayerDelayedProjs = -1;

                    var data = delProjsPlayer[i].delProjData;
                    data.turns -= 1;
                    delProjsPlayer[i].delProjData = data;

                    if (delProjsPlayer[i].delProjData.turns <= 0)
                    {
                        projToRemove.Add(delProjsPlayer[i]);
                        int target = delProjsPlayer[i].delProjData.position;
                        float postHitWait = 0.6f;
                        any = true;

                        if (delProjsPlayer[i].delProjData.whilesound != null)
                        {
                            if (delProjsPlayer[i].delProjData.whilesound[0] == '@')
                            {
                                MainManager.PlaySound(delProjsPlayer[i].delProjData.whilesound.Replace("@", ""), -1, 1f, 1f);
                            }
                            else
                            {
                                MainManager.PlaySound(delProjsPlayer[i].delProjData.whilesound, -1, 1f, 1f, true);
                            }
                        }

                        if (delProjsPlayer[i].targetEntity != null)
                        {
                            target = delProjsPlayer[i].targetEntity.battleid;
                        }

                        if (target >= battle.enemydata.Length)
                        {
                            target = -1;
                            for (int j = 0; j < battle.enemydata.Length; j++)
                            {
                                if (battle.enemydata[j].hp > 0 && delProjsPlayer[i].possiblePositions.Contains(battle.enemydata[j].position))
                                {
                                    target = j;
                                }
                            }
                        }

                        bool noShadow = false;
                        Vector3 offset = Vector3.up;
                        Vector3 partoffset = Vector3.zero;
                        if (delProjsPlayer[i].delProjData.args != null)
                        {
                            string[] array2 = delProjsPlayer[i].delProjData.args.Split(new char[] { '@' });
                            for (int j = 0; j < array2.Length; j++)
                            {
                                string[] array3 = array2[j].Split(new char[] { ',' });
                                string text = array3[0];
                                if (!(text == "partoff"))
                                {
                                    if (!(text == "move"))
                                    {
                                        if (text == "noshadow")
                                        {
                                            noShadow = true;
                                        }
                                    }
                                    else
                                    {
                                        offset = MainManager.VectorFromString(new string[]
                                        {
                                        array3[1],
                                        array3[2],
                                        array3[3]
                                        });
                                    }
                                }
                                else
                                {
                                    partoffset = MainManager.VectorFromString(new string[]
                                    {
                                    array3[1],
                                    array3[2],
                                    array3[3]
                                    });
                                }
                            }
                        }
                        if (!noShadow)
                        {
                            delProjsPlayer[i].delProjData.obj.AddComponent<ShadowLite>();
                        }

                        float a = 0f;
                        Vector3 startPos = delProjsPlayer[i].delProjData.obj.transform.position;

                        if (target != -1 && delProjsPlayer[i].type != DelProjType.StickyBomb)
                        {
                            Vector3 targetPos = battle.enemydata[target].battleentity.transform.position + offset 
                                + Vector3.up * battle.enemydata[target].battleentity.height;
                            do
                            {
                                delProjsPlayer[i].delProjData.obj.transform.position = Vector3.Lerp(startPos, targetPos, 
                                    a / delProjsPlayer[i].delProjData.framestep);
                                a += MainManager.framestep;
                                yield return null;
                            }
                            while (a < delProjsPlayer[i].delProjData.framestep);
                        }

                        inPlayerDelayedProjs = delProjsPlayer[i].delProjData.calledby.animid;

                        if (delProjsPlayer[i].delProjData.whilesound != null)
                        {
                            MainManager.StopSound(delProjsPlayer[i].delProjData.whilesound);
                        }
                        if (delProjsPlayer[i].delProjData.deathsound != null)
                        {
                            MainManager.PlaySound(delProjsPlayer[i].delProjData.deathsound);
                        }
                        if (delProjsPlayer[i].delProjData.deathparticle != null)
                        {
                            MainManager.PlayParticle(delProjsPlayer[i].delProjData.deathparticle, delProjsPlayer[i].delProjData.obj.transform.position + partoffset);
                        }

                        if (target != -1)
                        {
                            if (delProjsPlayer[i].type == DelProjType.InkTrap && battle.enemydata[target].hp > 0)
                            {
                                if (battle.enemydata[target].position == BattlePosition.Ground || battle.enemydata[target].position == BattlePosition.Underground)
                                {
                                    battle.DoDamage(null, ref battle.enemydata[target], delProjsPlayer[i].delProjData.damage, delProjsPlayer[i].delProjData.property, delProjsPlayer[i].overrides.ToArray(), false);
                                }
                            }
                            if (delProjsPlayer[i].type == DelProjType.NeedleToss)
                            {
                                if (NeedleSkills.targetDamages == null)
                                {
                                    NeedleSkills.targetDamages = new int?[battle.enemydata.Length];
                                    NeedleSkills.targetHits = new int[battle.enemydata.Length];
                                }
                                if (battle.enemydata[target].hp > 0 && battle.enemydata[target].position != BattlePosition.Underground)
                                {
                                    if (NeedleSkills.targetDamages[target] == null)
                                    {
                                        NeedleSkills.targetDamages[target] = battle.DoDamage(null, ref battle.enemydata[target], delProjsPlayer[i].delProjData.damage, delProjsPlayer[i].delProjData.property, delProjsPlayer[i].overrides.ToArray(), false);
                                    }
                                    else
                                    {
                                        if (NeedleSkills.targetDamages[target].Value > 1)
                                            NeedleSkills.targetDamages[target] = Mathf.Max(1, Mathf.FloorToInt(NeedleSkills.targetDamages[target].Value / 2f));
                                        battle.DoDamage(null, ref battle.enemydata[target], NeedleSkills.targetDamages[target].Value, delProjsPlayer[i].delProjData.property, delProjsPlayer[i].overrides.ToArray(), false);
                                    }
                                    NeedleSkills.targetHits[target]++;
                                    NeedleSkills.NeedleStatuses_Target(ref battle.enemydata[target], 1, 35);
                                }
                                if (!battle.IsStopped(MainManager.instance.playerdata[delProjsPlayer[i].delProjData.calledby.trueid]))
                                {
                                    StartStylishTimer(0, 8, stylishGain: 0.02f, commandSuccess: false);
                                }
                                if (i != delProjsPlayer.Count - 1 && delProjsPlayer[i + 1].type == DelProjType.NeedleToss) // makes consecutive needles fall WAY faster
                                {
                                    postHitWait = 0.25f;
                                }
                            }
                        }

                        if (delProjsPlayer[i].type == DelProjType.StickyBomb)
                        {
                            MainManager.ShakeScreen(Vector3.one * 0.1f, 0.15f);

                            if (delProjsPlayer[i].delProjData.areadamage > 0)
                            {
                                for (int j = 0; j < battle.enemydata.Length; j++)
                                {
                                    EntityControl targetEntity = battle.enemydata[j].battleentity;
                                    bool isClose = MainManager.GetSqrDistance(targetEntity.transform.position + targetEntity.freezeoffset + Vector3.up * targetEntity.height, delProjsPlayer[i].delProjData.obj.transform.position) <= 15.5f;

                                    if (isClose && battle.enemydata[j].hp > 0 && battle.enemydata[j].position != BattlePosition.Underground)
                                    {
                                        int damage = j == target ? delProjsPlayer[i].delProjData.damage : delProjsPlayer[i].delProjData.areadamage;
                                        battle.DoDamage(null, ref battle.enemydata[j], damage, null, null, false);
                                        MainManager.SetCondition(BattleCondition.Sticky, ref battle.enemydata[j], 4);
                                        MainManager.PlayParticle("StickyGet", battle.enemydata[j].battleentity.transform.position + Vector3.up);
                                        MainManager.PlaySound("WaterSplash2", -1, 0.8f, 1f);
                                    }
                                }
                            }
                        }

                        Destroy(delProjsPlayer[i].delProjData.obj);
                        yield return WaitStylish(postHitWait);
                    }
                }

                foreach (var proj in projToRemove)
                    delProjsPlayer.Remove(proj);

                if (any)
                {
                    BattleControl.SetDefaultCamera();
                    yield return StartCoroutine(battle.CheckDead());
                    battle.action = true;

                    yield return EventControl.halfsec;
                }
            }
            inPlayerDelayedProjs = -1;
            NeedleSkills.targetDamages = null;
            NeedleSkills.targetHits = null;
        }

        public void ApplyStatus(MainManager.BattleCondition condition, ref MainManager.BattleData target, int turns, string sound, float soundPitch, float soundVolume, string particle, Vector3 particlePos, Vector3 particleScale)
        {
            MainManager.SetCondition(condition, ref target, turns);
            if (sound != null && MainManager.SoundIsPlaying(sound) == -1)
                MainManager.PlaySound(sound, -1, soundPitch, soundVolume);

            if (particle != null)
            {
                if (particleScale == null)
                    particleScale = Vector3.one;
                MainManager.PlayParticle(particle, particlePos).transform.localScale = particleScale;
            }
        }

        public void DoSmearcharge(ref MainManager.BattleData target)
        {
            MainManager.battle.StartCoroutine(battle.ItemSpinAnim(target.battleentity.transform.position + Vector3.up, MainManager.itemsprites[1, (int)Medal.Smearcharge], true));
            int chargeBuff = MainManager.BadgeHowManyEquipped((int)Medal.Smearcharge, target.trueid);
            target.charge = Mathf.Clamp(target.charge + chargeBuff, 1, MainManager_Ext.CheckMaxCharge(target.trueid));
        }

        static IEnumerator CheckDelayedConditionsPlayer()
        {
            MainManager.BattleData target = MainManager.instance.playerdata[battle.currentturn];
            EntityControl playerEntity = MainManager.instance.playerdata[battle.currentturn].battleentity;

            if (target.delayedcondition != null && target.delayedcondition.Count > 0)
            {
                MainManager.BattleCondition[] bannedConditions = { MainManager.BattleCondition.Topple, BattleCondition.Flipped, BattleCondition.Eaten, BattleCondition.EventStop };
                foreach (var condition in target.delayedcondition)
                {
                    MainManager.BattleCondition battleCondition = (MainManager.BattleCondition)condition;

                    switch (battleCondition)
                    {
                        //technically impossible
                        case BattleCondition.Freeze:
                            playerEntity.Freeze();
                            Instance.ApplyStatus(battleCondition, ref MainManager.instance.playerdata[battle.currentturn], 1, null, 1, 1, "mothicenormal", playerEntity.transform.position + Vector3.up, Vector3.one * 1.5f);
                            break;
                        case BattleCondition.Numb:
                            Instance.ApplyStatus(battleCondition, ref MainManager.instance.playerdata[battle.currentturn], 1, "Numb", 1, 1, null, Vector3.one, Vector3.one);
                            break;

                        //technically impossible
                        case BattleCondition.Sleep:
                            Instance.ApplyStatus(battleCondition, ref MainManager.instance.playerdata[battle.currentturn], 1, "Sleep", 1, 1, null, Vector3.one, Vector3.one);
                            MainManager.DeathSmoke(playerEntity.transform.position + Vector3.up, Vector3.one * 2f);
                            MainManager.instance.playerdata[battle.currentturn].isasleep = true;
                            break;

                        case BattleCondition.AttackDown:
                        case BattleCondition.AttackUp:
                        case BattleCondition.DefenseDown:
                        case BattleCondition.DefenseUp:
                            battle.StatusEffect(MainManager.instance.playerdata[battle.currentturn], battleCondition, 1, true, false);
                            break;

                        case BattleCondition.Poison:
                            Instance.ApplyStatus(battleCondition, ref MainManager.instance.playerdata[battle.currentturn], 1, "Poison", 1, 1, "PoisonEffect", playerEntity.transform.position, Vector3.one);
                            break;

                        case BattleCondition.Fire:
                            Instance.ApplyStatus(battleCondition, ref MainManager.instance.playerdata[battle.currentturn], 1, "Flame", 1, 1, "Fire", playerEntity.transform.position, Vector3.one);
                            break;

                        case BattleCondition.Inked:
                            Instance.ApplyStatus(battleCondition, ref MainManager.instance.playerdata[battle.currentturn], 1, "WaterSplash2", 1, 1, "InkGet", playerEntity.transform.position, Vector3.one);
                            break;

                        case BattleCondition.Sticky:
                            Instance.ApplyStatus(battleCondition, ref MainManager.instance.playerdata[battle.currentturn], 1, "AhoneynationSpit", 1, 1, "StickyGet", playerEntity.transform.position, Vector3.one);
                            break;

                        default:
                            if (!bannedConditions.Contains(battleCondition))
                                MainManager.SetCondition(battleCondition, ref MainManager.instance.playerdata[battle.currentturn], 1);
                            break;
                    }
                }
                MainManager.instance.playerdata[battle.currentturn].delayedcondition = null;
                yield return EventControl.halfsec;
            }
        }

        public void DoLoomLegsCheck(ref MainManager.BattleData target, bool superguarded)
        {
            if (MainManager.BadgeIsEquipped((int)Medal.Loomlegs))
            {
                if (superguarded)
                {
                    loomLegProgress++;
                    if (loomLegProgress >= 3)
                    {
                        MainManager.battle.StartCoroutine(battle.ItemSpinAnim(target.battleentity.transform.position + Vector3.up, MainManager.itemsprites[1, (int)Medal.Loomlegs], true));
                        if (MainManager.HasCondition(MainManager.BattleCondition.Sturdy, target) == -1)
                        {
                            ApplyStatus(BattleCondition.Sticky, ref target, 3, "AhoneynationSpit", 1, 1, "StickyGet", target.battleentity.transform.position, Vector3.one);
                        }
                        loomLegProgress = 0;
                    }
                }
            }
        }

        public void DoHoneyWebCheck(ref MainManager.BattleData target, bool superguarded)
        {
            if (MainManager.BadgeIsEquipped((int)Medal.Honeyweb, target.trueid) && superguarded)
            {
                battle.StartCoroutine(battle.ItemSpinAnim(target.battleentity.transform.position + Vector3.up, MainManager.itemsprites[1, (int)Medal.Honeyweb], true));
                int tpRegen = 1;
                tpRegen += MainManager.BadgeHowManyEquipped((int)BadgeTypes.SuperBlock, target.trueid);
                RecoverPlayerTP(tpRegen, target);
            }
        }

        public void DoPurifyingPulseCheck(ref MainManager.BattleData target, int conditionCleared)
        {
            if (MainManager.BadgeIsEquipped((int)Medal.PurifyingPulse, target.trueid))
            {
                battle.StartCoroutine(battle.ItemSpinAnim(target.battleentity.transform.position + Vector3.up, MainManager.itemsprites[1, (int)Medal.PurifyingPulse], false));
                battle.Heal(ref target, 2 * conditionCleared, false);
            }
        }

        public void DoRevitalizingRippleCheck(ref MainManager.BattleData target, int conditionCleared)
        {
            if (MainManager.BadgeIsEquipped((int)Medal.RevitalizingRipple, target.trueid))
            {
                battle.StartCoroutine(battle.ItemSpinAnim(target.battleentity.transform.position + Vector3.up, MainManager.itemsprites[1, (int)Medal.RevitalizingRipple], false));
                MainManager.PlaySound("StatUp", -1, 0.9f, 1f);
                battle.StartCoroutine(battle.StatEffect(target.battleentity, 4));
                target.charge = Mathf.Clamp(target.charge + conditionCleared, 1, MainManager_Ext.CheckMaxCharge(target.trueid));
            }
        }

        public void CheckHDWGHConditionAmount(MainManager.BattleData player, Entity_Ext entity_Ext)
        {
            if(!MainManager.instance.librarystuff[3, (int)NewAchievement.HDWGH])
            {
                int otherConditions = 0;

                if (player.charge > 0)
                    otherConditions++;

                if (player.moreturnnextturn > 0)
                    otherConditions++;

                if (player.condition.Count + otherConditions > MainManager.instance.flagvar[(int)NewFlagVar.MaxConditions])
                {
                    MainManager.instance.flagvar[(int)NewFlagVar.MaxConditions] = player.condition.Count + otherConditions;

                    if (MainManager.instance.flagvar[(int)NewFlagVar.MaxConditions] >= MainManager_Ext.HDWGH_CONDITIONS)
                    {
                        MainManager.UpdateJounal(MainManager.Library.Logbook, (int)NewAchievement.HDWGH);
                    }
                }
            }
        }

        public void CheckEntitiesSprites()
        {
            int entityCount = battle.enemydata.Length + MainManager.instance.playerdata.Length;

            if (entityCount != entity_Exts.Count)
            {
                entity_Exts.Clear();
                for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                {
                    if (MainManager.instance.playerdata[i].battleentity != null)
                        entity_Exts.Add(Entity_Ext.GetEntity_Ext(MainManager.instance.playerdata[i].battleentity));
                }

                for (int i = 0; i < battle.enemydata.Length; i++)
                {
                    if (battle.enemydata[i].battleentity != null)
                        entity_Exts.Add(Entity_Ext.GetEntity_Ext(battle.enemydata[i].battleentity));
                }
            }


            for (int i = 0; i < entity_Exts.Count; i++)
            {
                if (entity_Exts[i].entity != null && entity_Exts[i].entity.model)
                {
                    entity_Exts[i].UpdateModelSprite();
                }
            }
        }

        public void DoFakeDamage(int enemyId, int damage)
        {
            Instance.DoFakeDamage(ref battle.enemydata[enemyId], damage);
        }

        public void DoFakeDamage(ref MainManager.BattleData target, int damage)
        {
            MainManager.PlaySound("Damage0", -1, 0.8f, 0.5f);
            target.hp = Mathf.Clamp(target.hp - damage, 1, target.maxhp);
            Vector3 startPos = target.battleentity.transform.position + target.cursoroffset - new Vector3(0, 0.5f, 0);
            battle.ShowDamageCounter(0, damage, startPos, Vector3.up + Vector3.right);
        }

        public void CheckThinIce(EntityControl entity)
        {
            int thinIceHeal = MainManager.BadgeHowManyEquipped((int)Medal.ThinIce, entity.battleid);
            if (thinIceHeal > 0)
            {
                MainManager.battle.StartCoroutine(battle.ItemSpinAnim(entity.transform.position + Vector3.up, MainManager.itemsprites[1, (int)Medal.ThinIce], true));

                thinIceHeal += 1 + MainManager.BadgeHowManyEquipped((int)MainManager.BadgeTypes.HealPlus, entity.battleid);
                for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                {
                    if (i != entity.battleid && MainManager.instance.playerdata[i].hp > 0)
                    {
                        MainManager.battle.Heal(ref MainManager.instance.playerdata[i], thinIceHeal);
                    }
                }
            }
        }

        bool CheckHailstorm()
        {
            for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
            {
                if (MainManager.instance.playerdata[i].hp > 0 && MainManager.HasCondition(BattleCondition.Freeze, MainManager.instance.playerdata[i]) != -1)
                    return true;
            }

            for (int i = 0; i < battle.enemydata.Length; i++)
            {
                if (battle.enemydata[i].hp > 0 && MainManager.HasCondition(BattleCondition.Freeze, battle.enemydata[i]) != -1)
                    return true;
            }

            return false;
        }

        IEnumerator DoHailStorm(bool usedByEnemy)
        {
            GameObject storm = Instantiate(MainManager_Ext.assetBundle.LoadAsset<GameObject>("Hailstorm"));
            storm.transform.position = new Vector3(usedByEnemy ? 0 : -8, 10, -2f);

            Transform[] particles = { storm.transform, storm.transform.GetChild(0) };

            foreach (var part in particles)
            {
                ParticleSystem ps = part.GetComponent<ParticleSystem>();
                var velocity = ps.velocityOverLifetime;

                var velocityMultiplier = usedByEnemy ? -1 : 1;
                velocity.x = new ParticleSystem.MinMaxCurve(velocity.x.constantMin * velocityMultiplier, velocity.x.constantMax * velocityMultiplier);

                var collision = ps.collision;
                collision.SetPlane(0, battle.battlemap.transform.GetChild(0));
            }
            storm.GetComponent<ParticleSystem>().Play();

            MainManager.PlaySound(MainManager_Ext.assetBundle.LoadAsset<AudioClip>("Snowstorm"), -1, 1.2f);
            yield return new WaitForSeconds(2.5f);

            int stormDamage = 3;

            AttackProperty property;
            DamageOverride[] overrides = new[] {
                (DamageOverride)NewDamageOverride.Magic,
                (DamageOverride)NewDamageOverride.Pierce1
            };
            if (usedByEnemy)
            {
                for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
                {
                    if (MainManager.instance.playerdata[i].hp > 0 && MainManager.instance.playerdata[i].eatenby == null)
                    {
                        property = UnityEngine.Random.Range(0, 2) == 0 ? AttackProperty.None : AttackProperty.Freeze;
                        battle.DoDamage(null, ref MainManager.instance.playerdata[i], stormDamage, property, overrides, battle.commandsuccess);
                        yield return EventControl.tenthsec;
                    }
                }
            }
            else
            {
                for (int i = 0; i < battle.enemydata.Length; i++)
                {
                    if (battle.enemydata[i].hp > 0 && battle.enemydata[i].position != BattlePosition.Underground)
                    {
                        property = UnityEngine.Random.Range(0, 2) == 0 ? AttackProperty.None : AttackProperty.Freeze;
                        battle.DoDamage(null, ref battle.enemydata[i], stormDamage, property, overrides, false);
                        yield return EventControl.tenthsec;
                    }
                }
            }
            yield return EventControl.halfsec;
            Destroy(storm, 5);
        }

        public static int GetMultiHitDamage(int baseDamage, int index)
        {
            if (Instance.Invulnerable(battle.enemydata[battle.target]))
                return 0;

            const int baseHitCount = 4;
            int hitCount = MainManager.BadgeIsEquipped((int)MainManager.BadgeTypes.Beemerang2) ? 5 : 4;

            if (hitCount > baseHitCount && index >= baseHitCount)
                index = baseHitCount - 1;

            int totalDamage = baseDamage < 2 ? 0 : baseDamage == 2 ? 4 : baseDamage * 2 - 1;
            int baseHit = totalDamage / baseHitCount;
            int remainder = totalDamage % baseHitCount;
            int hitDamage = baseHit + (index < remainder ? 1 : 0);
            return Mathf.Clamp(hitDamage, baseDamage < 2 ? 0 : 1, 99);
        }

        public void DoTinyHugeEffect(MainManager.BattleCondition condition, ref MainManager.BattleData target, Entity_Ext entity_Ext)
        {
            if ((int)condition == (int)NewCondition.Tiny)
            {
                if (MainManager.HasCondition((BattleCondition)NewCondition.Huge, target) > -1)
                {
                    MainManager.RemoveCondition(condition, target);
                    MainManager.RemoveCondition((BattleCondition)NewCondition.Huge, target);
                    return;
                }

                if (!entity_Ext.scaleChanged)
                {
                    float scaleTinyChange = 0.66f;
                    target.battleentity.startscale *= scaleTinyChange;
                    target.battleentity.shadowsize *= scaleTinyChange;
                    entity_Ext.scaleChanged = true;
                }

                if (!entity_Ext.tinyMovesAdded && !battle.enemy)
                {
                    target.cantmove--;
                    entity_Ext.tinyMovesAdded = true;
                }
            }
            else if ((int)condition == (int)NewCondition.Huge)
            {
                if(battle.enemy)
                {
                    if(actionID == target.battleentity.battleid)
                        noHugeMoveReduction = true;
                }
                else
                {
                    if(battle.currentturn == target.trueid)
                        noHugeMoveReduction = true;
                }

                if (MainManager.HasCondition((BattleCondition)NewCondition.Tiny, target) > -1)
                {
                    MainManager.RemoveCondition(condition, target);
                    MainManager.RemoveCondition((BattleCondition)NewCondition.Tiny, target);
                    return;
                }

                if (!entity_Ext.scaleChanged)
                {
                    float scaleHugeChange = 1.5f;
                    target.battleentity.startscale *= scaleHugeChange;
                    target.battleentity.shadowsize *= scaleHugeChange;
                    entity_Ext.scaleChanged = true;
                }
            }
        }

        public void ResetTinyHugeEffect(EntityControl entity, Entity_Ext entity_Ext = null)
        {
            if (entity_Ext == null)
                entity_Ext = Entity_Ext.GetEntity_Ext(entity);
            entity_Ext.scaleChanged = false;
            entity.startscale = entity_Ext.baseScale;
            entity.shadowsize = entity_Ext.shadowBaseSize;
        }

        public int GetTinyHugeStat(MainManager.BattleData data)
        {
            if (MainManager.HasCondition((BattleCondition)NewCondition.Tiny, data) > -1)
                return -2;

            if (MainManager.HasCondition((BattleCondition)NewCondition.Huge, data) > -1)
                return 2;

            return 0;
        }

        public void CheckHugeAction(ref MainManager.BattleData data)
        {
            if (MainManager.HasCondition((BattleCondition)NewCondition.Huge, data) > -1 && !noHugeMoveReduction)
            {
                data.cantmove++;
            }
            noHugeMoveReduction = false;
        }

        public void DoConditionPierce(ref MainManager.BattleData target, int turns, MainManager.BattleCondition condition, string particleName, string sound, float pitch = 1, float volume = 1, Vector3? particlePos = null, Vector3? scale = null)
        {
            turns = GetConditionTurnPierce(target, turns);
            if (turns > -1)
            {
                if (particlePos == null)
                    particlePos = target.battleentity.transform.position;

                if (scale == null)
                    scale = Vector3.one;
                Instance.ApplyStatus(condition, ref target, turns, sound, pitch, volume, particleName, particlePos.Value, scale.Value);
            }
        }

        public void DoConditionPierce(ref MainManager.BattleData target, int turns, MainManager.BattleCondition condition)
        {
            turns = GetConditionTurnPierce(target, turns);
            if (turns > -1)
            {
                battle.TryCondition(ref target, condition, turns);
            }
        }

        public void DoConditionPierceStatEffect(ref MainManager.BattleData target, int turns, MainManager.BattleCondition condition, bool effect, bool force)
        {
            turns = GetConditionTurnPierce(target, turns);
            if (turns > 0)
            {
                battle.StatusEffect(target, condition, turns, effect, force);
            }
        }

        public int GetConditionTurnPierce(MainManager.BattleData target, int turns)
        {
            if (target.hp > 0)
            {
                if (battle.commandsuccess)
                {
                    turns--;
                    if (battle.GetSuperBlock(0) || battle.superblockedthisframe > 0f)
                        turns -= 1;
                }
                return turns;
            }
            return -1;
        }

        public void SetSturdy(ref MainManager.BattleData target, int turns, int enemyId = -1)
        {
            MainManager.PlaySound("Heal3");
            battle.ClearStatus(ref target);
            MainManager.SetCondition(MainManager.BattleCondition.Sturdy, ref target, turns);
            MainManager.PlayParticle("MagicUp", target.battleentity.transform.position);

            if (enemyId != -1)
            {
                battle.enemydata[enemyId].delayedcondition = null;
                if (battle.enemydata[enemyId].frostbitep != null)
                {
                    Destroy(battle.enemydata[enemyId].frostbitep.gameObject);
                }
            }
        }

        public void GetEnemyTpCharge(int tp, int targetId)
        {
            int charges = Mathf.CeilToInt(tp / 2);
            int maxPerEnemy = GetMaxEnemyCharge(battle.enemydata[targetId].battleentity);
            int index = targetId;

            int[] chargeAmounts = new int[battle.enemydata.Length];
            while (charges > 0 && battle.enemydata.Any(e => e.charge + chargeAmounts[index] < GetMaxEnemyCharge(battle.enemydata[index].battleentity)))
            {
                if (battle.enemydata[index].charge + chargeAmounts[index] < GetMaxEnemyCharge(battle.enemydata[index].battleentity))
                {
                    chargeAmounts[index]++;
                    charges--;
                }

                index = (index + 1) % battle.enemydata.Length;
            }

            MainManager.PlaySound("StatUp");
            for (int i = 0; i < chargeAmounts.Length; i++)
            {
                if (chargeAmounts[i] > 0 && battle.enemydata[i].charge < maxPerEnemy)
                {
                    battle.enemydata[i].charge = Mathf.Clamp(battle.enemydata[i].charge + chargeAmounts[i], 0, GetMaxEnemyCharge(battle.enemydata[i].battleentity));
                    battle.StartCoroutine(battle.StatEffect(battle.enemydata[i].battleentity, 4));
                }
            }
        }

        public int GetMaxEnemyCharge(EntityControl enemy)
        {
            return Entity_Ext.GetEntity_Ext(enemy).extraData.MaxCharge;
        }

        public void SetStartState(EntityControl entity, int state)
        {
            entity.animstate = state;
            entity.basestate = entity.animstate;
            Instance.startState = entity.animstate;
        }

        public static IEnumerator DoSpin(EntityControl entity, float frameTime, float targetAngle = 360f, bool returnToAngle = true)
        {
            Vector3 angle = entity.spritetransform.localEulerAngles;
            float a = 0;
            do
            {
                entity.spritetransform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(0f, targetAngle, a / frameTime));
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < frameTime);
            if (returnToAngle)
                entity.spritetransform.localEulerAngles = angle;
        }

        public void ApplyHuge(EntityControl entity, ref MainManager.BattleData target, int turns)
        {
            BattleControl_Ext.Instance.ApplyStatus(
                (MainManager.BattleCondition)NewCondition.Huge, ref target, turns,
                "Grow", 0.8f, 1, NewParticle.HugePart.ToString(), entity.transform.position + new Vector3(0, 0, -0.1f),
                Vector3.one);
        }

        public void ApplyTiny(EntityControl entity, ref MainManager.BattleData target, int turns)
        {
            BattleControl_Ext.Instance.ApplyStatus(
                (MainManager.BattleCondition)NewCondition.Tiny, ref target, turns,
                "Shot2", 1.2f, 1, NewParticle.TinyPart.ToString(), entity.transform.position + new Vector3(0, 3, -0.1f),
                Vector3.one);
        }
        public int GetMushroomTinyHuge(int itemId)
        {
            int[] hugeItems = {
                (int)MainManager.Items.Mushroom, (int)MainManager.Items.GlazedShroom,
                (int)MainManager.Items.CookedShroom, (int)MainManager.Items.HoneyShroom, (int)MainManager.Items.MushroomStick,
                (int)MainManager.Items.HeartyBreakfast, (int)NewItem.HeartyBreakfast2, (int)MainManager.Items.MushroomCandy,
                (int)NewItem.AgaricDots, (int)NewItem.StickySoup
            };
            int[] tinyItems = {
                (int)MainManager.Items.DangerShroom, (int)MainManager.Items.CookedDanger,  (int)MainManager.Items.HoneyDanger,
                (int)NewItem.Cottoncap, (int)NewItem.Napcap,(int)MainManager.Items.ShockShroom,(int)NewItem.JoltMush,
                (int)NewItem.DynamoDish,
            };

            if (hugeItems.Contains(itemId))
                return (int)NewCondition.Huge;

            if (tinyItems.Contains(itemId))
                return (int)NewCondition.Tiny;

            return -1;
        }

        public void CheckMiniMegaMush(int itemId, ref MainManager.BattleData target)
        {
            if (MainManager.BadgeIsEquipped((int)Medal.MiniMegaMush))
            {
                int buffType = GetMushroomTinyHuge(itemId);
                int baseTurns = 2;
                switch (buffType)
                {
                    case (int)NewCondition.Huge:
                        Instance.ApplyHuge(target.battleentity, ref target, baseTurns);
                        break;

                    case (int)NewCondition.Tiny:
                        Instance.ApplyTiny(target.battleentity, ref target, baseTurns);
                        break;
                }
            }

        }

        public int FindAlivePlayer(int startIndex = 0)
        {
            for (int i = startIndex; i < battle.partypointer.Length; i++)
            {
                if (MainManager.instance.playerdata[battle.partypointer[i]].hp > 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public IEnumerator WaitUntilReorganize(int enemyDataLength, EntityControl entity)
        {
            yield return new WaitUntil(() => enemyDataLength != MainManager.battle.enemydata.Length);
            Destroy(entity.gameObject);
        }

        public void CheckMothflower(bool superBlocked, bool block, MainManager.BattleData target)
        {
            if (BadgeIsEquipped((int)Medal.Mothflower) && target.trueid == 2 && block)
            {
                BattleControl_Ext.Instance.mothFlowerBlocks += BadgeHowManyEquipped((int)Medal.Mothflower);

                if (superBlocked && BattleControl_Ext.Instance.mothFlowerSuperBlocks < BadgeHowManyEquipped((int)BadgeTypes.SuperBlock, target.trueid))
                {
                    BattleControl_Ext.Instance.mothFlowerSuperBlocks++;
                }
            }
        }

        public void ApplyDizzy(ref MainManager.BattleData target, int turns)
        {
            Instance.ApplyStatus((BattleCondition)NewCondition.Dizzy, ref target, turns, "Toss8", 1, 1, null, Vector3.one, Vector3.one);
        }

        public void TryDizzy(MainManager.BattleData? attacker, ref MainManager.BattleData target, int turns, float statusBoost = 0)
        {
            if (attacker.HasValue && attacker.Value.battleentity.CompareTag("Player") 
                && MainManager.BadgeIsEquipped((int)BadgeTypes.StatusBoost, attacker.Value.trueid))
            {
                statusBoost += 15;
            }

            var targetExt = Entity_Ext.GetEntity_Ext(target.battleentity);
            int dizzyRes = targetExt.extraData.DizzyRes;
            if (dizzyRes < 100 && UnityEngine.Random.Range(0, 100) >= dizzyRes - statusBoost)
            {
                ApplyDizzy(ref target, turns);
                if (targetExt.isPlayer)
                {
                    battle.UpdateAnim(true);
                    if (attacker != null && BadgeIsEquipped((int)BadgeTypes.StatusMirror, target.trueid))
                    {
                        TryDizzy(null, ref battle.enemydata[attacker.Value.battleentity.battleid], 2);
                    }
                }
                else
                {
                    targetExt.extraData.DizzyRes += battle.HardMode() ? 10 : 8;
                }
                return;
            }
        }

        public int GetPlayerBehind(int playerId)
        {
            int playerPos = battle.partypointer.Select((value, index) => new { value, index }).First(i => i.value == playerId).index;
            for (int i = playerPos; i < battle.partypointer.Length; i++)
            {
                int playerPointer = battle.partypointer[i];
                if (playerPointer != playerId && MainManager.instance.playerdata[playerPointer].hp > 0)
                {
                    return playerPointer;
                }
            }
            return -1;
        }

        public static bool IsLeafBugSplotchSpider(EntityControl entity)
        {
            bool isInLeafbugMap =
                   (int)MainManager.map.mapid == (int)NewMaps.LeafbugVillage
                || (int)MainManager.map.mapid == (int)NewMaps.LeafbugShamanHut;

            bool isInSpecificBattle = MainManager.battle != null
                && MainManager.battle.sdata.enemies.
                            Where(e => e == (int)NewEnemies.LeafbugShaman || e == (int)NewEnemies.JumpAnt).Any();

            return isInLeafbugMap || entity.name.Contains("Leafbug") || isInSpecificBattle
                || MainManager.lastevent == (int)NewEvents.JumpAntIntermission5;
        }

        public static bool CanSwap(int playerId)
        {
            return !Entity_Ext.GetEntity_Ext(MainManager.instance.playerdata[playerId].battleentity).cantSwap;
        }

        public static bool EveryoneCanSwap()
        {
            for(int i=0; i < MainManager.instance.playerdata.Length; i++)
            {
                if (!CanSwap(i))
                    return false;
            }
            return true;
        }

        public static bool CanBypassSwapRestrictions()
        {
            return MainManager.instance.playerdata.Any(p => Entity_Ext.GetEntity_Ext(p.battleentity).canBypassSwapRestrictions);
        }

        public bool Invulnerable(BattleData target, List<DamageOverride> overrides = null)
        {
            return battle.Invulnerable(target) && 
                (overrides == null || !overrides.Contains((DamageOverride)NewDamageOverride.IgnoreInvulnerable));
        }

        public bool IsDotDamage(DamageOverride[] overrides)
        {
            return overrides != null && overrides.Contains((DamageOverride)NewDamageOverride.StatusDamage);
        }

        static BattleControl.DamageOverride[] GetStatusDamageOverrides(BattleControl.DamageOverride[] overrides)
        {
            return overrides.AddToArray((BattleControl.DamageOverride)NewDamageOverride.StatusDamage);
        }
    }
}
