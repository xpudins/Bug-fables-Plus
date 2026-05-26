using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MainManager;
using static BattleControl;

namespace BFPlus.Extensions.EnemyAI
{
    class MechaJawAI : AI
    {
        enum Attacks
        {
            Chomp,
            CannonShots,
            Kaboom,
            Relay,
            UseItem
        }
        BattleControl battle;
        MechaJawComp comp;
        const int CHOMP_DAMAGE = 4;
        const int CHOMP_AMOUNT = 3;
        const int CANNON_DAMAGE = 3;
        const int CANNON_AMOUNT = 5;
        const int INK_BUBBLE = 1;
        const int TP_REGEN = 3;
        const int CANNON_HEAL = 2;
        const int CANNON_TP = 2;
        const int KABOOM_DAMAGE = 20;
        const int KABOOM_BURN = 6;
        const int KABOOM_NUMB = 3;
        int jumpAntId = -1;
        public override IEnumerator DoBattleAI(EntityControl entity, int actionid)
        {
            battle = MainManager.battle;

            battle.dontusecharge = true;
            comp = entity.GetComponent<MechaJawComp>();
            comp.ResetFlippedRes(ref battle.enemydata[actionid]);

            Dictionary<Attacks, int> attacks = new Dictionary<Attacks, int>()
            {
                { Attacks.Chomp, 60},
                { Attacks.CannonShots, 40},
            };

            jumpAntId = battle.EnemyInField((int)NewEnemies.JumpAnt);
            JumpAntFight fightComponent = BattleControl_Ext.Instance.jumpAntFightComp;
            int itemTargetId = -1;

            if (jumpAntId != -1)
            {
                itemTargetId = fightComponent.GetItemTargetId(jumpAntId);
                fightComponent.AddPartnerAttacks(attacks, actionid, jumpAntId, itemTargetId, Attacks.Relay, Attacks.UseItem);
            }

            Attacks attack = MainManager_Ext.GetWeightedResult(attacks);


            switch (attack)
            {
                case Attacks.Chomp:
                    yield return DoChomp(entity, actionid);
                    break;

                case Attacks.CannonShots:
                    yield return DoCannonShots(entity, actionid);
                    break;

                case Attacks.Relay:
                    yield return battle.EnemyRelay(entity, jumpAntId);
                    break;

                case Attacks.UseItem:
                    yield return fightComponent.DoUseItem(entity, itemTargetId, fightComponent);
                    break;
            }
        }

        IEnumerator DoChomp(EntityControl entity, int actionid)
        {
            float jumpSpeed = comp.turntUp ? 25f : 30f;
            float waitTime = comp.turntUp ? 0.25f : 0.30f;

            Vector3 basePos = entity.transform.position;
            int targetIndex = BattleControl_Ext.Instance.FindAlivePlayer();

            Vector3 playerOffset = new Vector3(1.5f, 0f, -0.1f);
            int target = battle.partypointer[targetIndex];
            Vector3 targetPos = MainManager.instance.playerdata[target].battleentity.transform.position + playerOffset;

            entity.animstate = 108;
            entity.StartCoroutine(entity.ShakeSprite(0.1f, 15f));
            yield return EventControl.quartersec;

            entity.animstate = 110;
            entity.LockRigid(true);
            entity.overrridejump = true;
            MainManager.PlaySound("AhoneynationHopJump", 1.2f, 1f);
            yield return MainManager.ArcMovement(entity.gameObject, entity.transform.position, targetPos, Vector3.zero, 3, jumpSpeed, false);

            entity.animstate = 0;
            int tpRegen = 0;
            int lastDamaged = target;

            AudioClip chompSound = MainManager_Ext.assetBundle.LoadAsset<AudioClip>("MechaChomp");

            for (int i = 0; i < CHOMP_AMOUNT; i++)
            {
                if (MainManager.GetAlivePlayerAmmount() == 0)
                    break;

                target = battle.partypointer[targetIndex];
                EntityControl targetEntity = MainManager.instance.playerdata[target].battleentity;
                if (lastDamaged != target)
                {
                    targetPos = targetEntity.transform.position + playerOffset;
                    yield return MainManager.ArcMovement(entity.gameObject, entity.transform.position, targetPos, Vector3.zero, 3, jumpSpeed, false);
                }
                entity.animstate = 0;
                yield return new WaitForSeconds(waitTime);

                entity.PlaySound("Bite3", 1, 1.2f);
                entity.animstate = 104;
                yield return EventControl.thirdsec;
                MainManager.PlaySound(chompSound);

                Vector3 startPos = targetEntity.transform.position + Vector3.up * 2f;
                Vector3 endPos = targetEntity.transform.position + Vector3.up * 4f;

                bool moveOnToNextTarget = !battle.commandsuccess || battle.IsStopped(instance.playerdata[target]);
                int damageDealt = battle.DoDamage(actionid, target, CHOMP_DAMAGE, BattleControl.AttackProperty.Pierce, battle.commandsuccess);
                if (damageDealt > 0)
                {
                    int drain = Mathf.FloorToInt(damageDealt * 0.5f);
                    BattleControl_Ext.Instance.RemoveTP(-drain, startPos, endPos);
                    tpRegen += drain;
                }

                lastDamaged = target;
                if (moveOnToNextTarget)
                {
                    int foundIndex = BattleControl_Ext.Instance.FindAlivePlayer(targetIndex + 1);
                    targetIndex = foundIndex == -1 ? targetIndex : foundIndex;
                }
                else
                {
                    yield return EventControl.thirdsec;
                }
            }

            yield return EventControl.halfsec;

            entity.animstate = 108;
            MainManager.PlaySound("AhoneynationHopJump", 1.2f, 1f);
            yield return MainManager.ArcMovement(entity.gameObject, entity.transform.position, basePos, Vector3.zero, 3, jumpSpeed, false);
            entity.transform.position = basePos;
            entity.animstate = 0;

            entity.LockRigid(false);
            entity.overrridejump = false;

            if (tpRegen > 0)
                BattleControl_Ext.Instance.RecoverEnemyTp(tpRegen, actionid);
            yield return EventControl.halfsec;
        }

        IEnumerator DoCannonShots(EntityControl entity, int actionid)
        {
            battle.nonphyscal = true;
            bool hardmode = battle.HardMode();
            float cannonSpeed = comp.turntUp ? 30f : 40f;

            entity.animstate = 100; //open cannon

            yield return EventControl.tenthsec;
            MainManager.PlaySound("B33BotHatchOpen", 1.2f, 1);

            yield return new WaitForSeconds(0.6f);
            DamageOverride[] CANNON_PLAYER_OVERRIDES = new[] { (DamageOverride)NewDamageOverride.IgnorePaintball };

            bool hitItself = false;
            for (int i = 0; i < CANNON_AMOUNT; i++)
            {
                if (MainManager.GetAlivePlayerAmmount() == 0)
                    break;

                if (i == CANNON_AMOUNT - 1 && MainManager.HasCondition(MainManager.BattleCondition.GradualTP, battle.enemydata[actionid]) == -1)
                {
                    entity.flip = false;
                    entity.animstate = 103;
                    //regen;
                    yield return EventControl.halfsec;
                    MainManager.PlaySound("Explosion", 1.2f, 1);
                    MainManager.PlayParticle("explosionsmall", entity.transform.position);
                    entity.Jump(5);
                    EnemyCannonEffect(hardmode, actionid);
                    hitItself = true;
                    yield return EventControl.sec;
                    break;
                }

                bool targetEnemy = i > 0 && Random.value < 0.25f && battle.enemydata.Length != 1;
                EntityControl targetEntity;
                int targetId = -1;
                if (targetEnemy)
                {
                    //target Enemy
                    int[] enemies = battle.enemydata
                        .Select((e, Index) => new { e, Index })
                        .Where(e => e.Index != actionid)
                        .Select(e => e.Index).ToArray();
                    targetId = enemies[Random.Range(0, enemies.Length)];
                    targetEntity = battle.enemydata[targetId].battleentity;
                }
                else
                {
                    battle.GetSingleTarget();
                    targetId = battle.playertargetID;
                    targetEntity = battle.playertargetentity;
                }

                entity.FaceTowards(targetEntity.transform.position);
                yield return EventControl.tenthsec;

                entity.animstate = 102; //shoot cannon
                yield return EventControl.tenthsec;

                Vector3 targetPos = targetEntity.transform.position + new Vector3(0, 0, -0.1f);
                MainManager.PlaySound("b33BotInstantMissle", 1.2f, 1);
                GameObject exploPart = MainManager.PlayParticle("explosionsmall",
                    entity.transform.position + new Vector3(!targetEnemy ? -1 : 1, 2, -0.1f));
                exploPart.transform.localEulerAngles = new Vector3(330, 270, 270);
                yield return ThrowBubble(new Color(0.22f, 0f, 0.33f), entity.transform.position + new Vector3(-1, 2, -0.1f), targetPos, cannonSpeed, 5, Vector3.one * 0.4f);

                if (targetEnemy)
                {
                    EnemyCannonEffect(hardmode, targetId);
                }
                else
                {
                    battle.DoDamage(actionid, targetId, CANNON_DAMAGE, null, CANNON_PLAYER_OVERRIDES, battle.commandsuccess);

                    Vector3 particlePos = targetEntity.transform.position + Vector3.up;
                    if (MainManager.instance.playerdata[targetId].hp > 0)
                    {
                        //ink bubble
                        BattleControl_Ext.Instance.
                            ApplyStatus((MainManager.BattleCondition)NewCondition.Paintball, ref MainManager.instance.playerdata[targetId], INK_BUBBLE,
                            "Shield", 0.8f, 1, "InkGet", particlePos, Vector3.one);
                    }
                }
                yield return EventControl.quartersec;
                entity.animstate = 101;
            }

            if (!hitItself)
            {
                entity.animstate = 111;
                yield return EventControl.halfsec;
            }

            entity.animstate = 0;

            entity.flip = false;
            yield return null;
        }

        void EnemyCannonEffect(bool heal, int enemyId)
        {
            BattleControl_Ext.Instance.RecoverEnemyTp(CANNON_TP, enemyId);
            if (heal)
            {
                battle.Heal(ref battle.enemydata[enemyId], CANNON_HEAL);
            }
            MainManager.PlayParticle("MagicUp", null, battle.enemydata[enemyId].battleentity.transform.position);
            BattleControl_Ext.Instance.AddEnemyBuff(enemyId, MainManager.BattleCondition.GradualTP, TP_REGEN, "Heal3", -1);
        }

        public static IEnumerator DoKaboom(EntityControl entity, int actionid, BattleCondition status)
        {
            Vector3 startPos = entity.transform.position;
            BattleControl battle = MainManager.battle;
            entity.animstate = 112;
            yield return null;

            var particles = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Particles/Flame")) as GameObject;
            particles.transform.parent = entity.spritetransform;
            particles.transform.localPosition = new Vector3(0f, 0.75f);
            AudioSource alarmSound = MainManager.PlaySound(MainManager_Ext.assetBundle.LoadAsset<AudioClip>("AlarmBell"), 9, 1, 0.8f, true);

            yield return new WaitForSeconds(2);

            if (alarmSound != null)
                alarmSound.volume = MainManager.soundvolume * 0.55f;

            entity.LockRigid(true);
            entity.overrridejump = true;
            MainManager.SetCamera(new Vector3(0, 1, -2), MainManager.battlecamangle, MainManager.battlecampos, 0.1f);
            entity.animstate = 105;
            yield return new WaitForSeconds(0.41f);
            Vector3 targetPos = new Vector3(7, 0, -0.1f);
            MainManager.PlaySound("AhoneynationHopJump", 1.2f, 1f);
            yield return MainManager.ArcMovement(entity.gameObject, startPos, targetPos, Vector3.zero, 3, 30, false);
            battle.StartCoroutine(BattleControl_Ext.DoSpin(entity, 20, 460, false));

            targetPos = battle.partymiddle + new Vector3(0.5f, 0, -0.1f);
            MainManager.PlaySound("AhoneynationBodySlamJump", 1.2f, 1f);
            yield return MainManager.ArcMovement(entity.gameObject, entity.transform.position, targetPos, Vector3.zero, 15, 80, false);

            for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
            {
                if (MainManager.instance.playerdata[i].hp > 0)
                {
                    battle.DoDamage(null, ref MainManager.instance.playerdata[i], KABOOM_DAMAGE, BattleControl.AttackProperty.Pierce, battle.commandsuccess);
                    if (status == BattleCondition.Numb)
                    {
                        BattleControl_Ext.Instance.DoConditionPierce(ref MainManager.instance.playerdata[i], KABOOM_NUMB, status);
                    }
                    else
                    {
                        BattleControl_Ext.Instance.DoConditionPierce(ref MainManager.instance.playerdata[i], KABOOM_BURN, status, "Flame", "Fire");
                    }
                }
            }

            var ps = MainManager.PlayParticle(NewParticle.MechaJawKaboom.ToString(), battle.partymiddle);
            ps.GetComponent<ParticleSystemRenderer>().material = MainManager.spritemat;
            foreach (Transform p in ps.transform)
            {
                p.GetComponent<ParticleSystemRenderer>().material = MainManager.spritemat;
            }

            MainManager.StopSound(9);
            MainManager.PlaySound("EverLastingKingArtifactDestroy", 1.2f, 1);
            MainManager.PlaySound(MainManager_Ext.assetBundle.LoadAsset<AudioClip>("MechaBoom"));
            MainManager.PlayParticle("explosion", battle.partymiddle);
            MainManager.ShakeScreen(0.1f, 0.5f);

            entity.sprite.gameObject.SetActive(false);
            entity.shadow.gameObject.SetActive(false);
            battle.enemydata[actionid].hp = 0;
            battle.enemydata[actionid].battleentity.destroytype = NPCControl.DeathType.Sink;
            yield return EventControl.halfsec;
            BattleControl.SetDefaultCamera();
        }
    }
}
