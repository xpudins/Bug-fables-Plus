using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BFPlus.Patches.BattleControlTranspilers;
using static UnityEngine.Object;
using static MainManager;
using static BattleControl;
using System.Linq;
using System;

namespace BFPlus.Extensions.BattleStuff.StatusStuff
{
    public static class Dizzy
    {
        static Color[] DizzyColors = new[]
        {
            new Color(115/255f, 047/255f, 134/255f), // darkest
            new Color(160/255f, 064/255f, 153/255f),
            new Color(209/255f, 095/255f, 140/255f),
            new Color(240/255f, 129/255f, 129/255f),
            new Color(255/255f, 192/255f, 180/255f), // lightest
        };
        static bool inTurnadoSteal = false;
        public static bool dizzyKO = false;
        public static List<Vector3> dizzyKOpositions = new List<Vector3>();
        static int CORKSCREW_MAX = 2;
        // - - - - - - - - - - - - - - - - - - - -
        // default effects
        public static void CheckRecoilDamage(BattleData? attacker, int damageDone)
        {
            if (attacker == null && BattleControl_Ext.Instance.entityAttacking != null)
            {
                if (battle.enemy && BattleControl_Ext.Instance.entityAttacking.battleid < battle.enemydata.Length)
                    attacker = battle.enemydata[BattleControl_Ext.Instance.entityAttacking.battleid];
                else if (battle.currentturn != -1)
                    attacker = instance.playerdata[battle.currentturn];
            }

            if (attacker != null && HasCondition((BattleCondition)NewCondition.Dizzy, attacker.Value) > -1)
            {
                Entity_Ext extEnt = Entity_Ext.GetEntity_Ext(attacker.Value.battleentity);

                float dmgMult = 0.5f;
                if (extEnt.isPlayer)
                    dmgMult += BadgeHowManyEquipped((int)Medal.Spinout, attacker.Value.trueid) * 0.5f;

                int recoilDamage = Mathf.Clamp(Mathf.FloorToInt(damageDone * dmgMult), 1, 99);

                PlaySound("Damage0", -1, 0.8f, 0.5f);

                DamageOverride[] overrides = {
                    DamageOverride.NoFall,
                    DamageOverride.NoIceBreak,
                    DamageOverride.NoDamageAnim,
                    DamageOverride.DontAwake,
                    DamageOverride.IgnoreNumb,
                    (DamageOverride)NewDamageOverride.StatusDamage
                };

                if (extEnt.isPlayer)
                {
                    extEnt.dizzyRecoil +=
                        battle.DoDamage(null,
                            ref instance.playerdata[battle.currentturn],
                            recoilDamage,
                            AttackProperty.NoExceptions, overrides, false
                        );
                    if (instance.playerdata[battle.currentturn].hp <= 0)
                    {
                        dizzyKO = true;
                        extEnt.diedFromDizzy = false;
                    }
                }
                else
                {
                    int enemyId = attacker.Value.battleentity.battleid;
                    extEnt.dizzyRecoil +=
                        battle.DoDamage(null,
                            ref battle.enemydata[enemyId],
                            recoilDamage,
                            AttackProperty.NoExceptions, overrides, false
                     );

                    if (battle.enemydata[enemyId].hp <= 0)
                    {
                        dizzyKO = true;
                        extEnt.diedFromDizzy = false;
                    }
                }
            }
        }
        public static void DoAfterEffect(ref BattleData target, Entity_Ext entityExt, bool wasDizzy)
        {
            if (entityExt.extraData.dizzyAfter != null)
            {
                int dizzyAfterCount = entityExt.extraData.dizzyAfter.Count;
                for (int i = 0; i < dizzyAfterCount; i++)
                {
                    entityExt.extraData.dizzyAfter[i]--;
                    if (entityExt.extraData.dizzyAfter[i] <= 0)
                    {
                        BattleControl_Ext.Instance.TryDizzy(null, ref target, 2);
                    }
                }
                entityExt.extraData.dizzyAfter.RemoveAll(i => i <= 0);
            }
            if (wasDizzy && DamagePipelineHandler.FlipTarget(ref target))
            {
                DizzyFlipParticle(target);
            }
        }

        static void CheckDizzyKO(int entityID, bool isPlayer)
        {
            EntityControl entity = isPlayer ?
                instance.playerdata[entityID].battleentity :
                battle.enemydata[entityID].battleentity;
            Entity_Ext extEnt = Entity_Ext.GetEntity_Ext(entity);

            if (extEnt.diedFromDizzy == null)
                return;

            if (!isPlayer)
                dizzyKOpositions.Add(entity.transform.position);

            if (isPlayer && BadgeIsEquipped((int)Medal.Spinout, instance.playerdata[entityID].trueid) && !extEnt.didSpinout)
            {
                battle.StartCoroutine(DoSpinout(instance.playerdata[entityID], extEnt, isPlayer));
                return;
            }

            PlaySound("Toss8");
            battle.StartCoroutine(DizzyWhirl(entity, 1f));
            extEnt.diedFromDizzy = true;
            extEnt.dizzyRecoil = 0;
        }

        static IEnumerator WaitForDizzyKO(bool player)
        {
            yield return new WaitUntil(() => player ?
                instance.playerdata.All(p => p.battleentity == null || (Entity_Ext.GetEntity_Ext(p.battleentity)?.diedFromDizzy ?? true)) :
                battle.enemydata.All(e => e.battleentity == null || (Entity_Ext.GetEntity_Ext(e.battleentity)?.diedFromDizzy ?? true)));
        }

        // turnado
        public static IEnumerator DoTurnado(int relayTarg)
        {
            PatchBattleControlRelay.waitForEffects++;
            int targetID = FindTurnadoTarget(out int stealableID);
            int finalTargetID = targetID > -1 ? targetID : battle.enemydata.Length - 1;
            battle.StartCoroutine(DizzyWhirl(battle.enemydata[finalTargetID].battleentity, 2f));
            PlaySound("Woosh3", 0.7f, 1);
            yield return EventControl.tenthsec;
            if (targetID > -1)
                battle.enemybounce = new Coroutine[1] { battle.StartCoroutine(battle.BounceEnemy(battle.enemydata[targetID], 0, 0, 1f)) };

            if (stealableID > -1)
                battle.StartCoroutine(TurnadoItemSteal(targetID, relayTarg));

            yield return new WaitForSeconds(0.25f);

            if (targetID > -1)
                TurnadoDamage(targetID);

            if (battle.enemybounce != null)
            {
                yield return new WaitUntil(() => ArrayIsEmpty(battle.enemybounce, false));
            }

            if (targetID > -1)
            {
                if (battle.enemydata[targetID].hp <= 0)
                    yield return battle.StartCoroutine(battle.CheckDead());

                if (battle.EnemyDropping())
                {
                    battle.startdrop = true;
                    while (battle.EnemyDropping())
                    {
                        battle.startdrop = true;
                        yield return null;
                    }
                    battle.startdrop = false;
                }
            }

            if (inTurnadoSteal)
                yield return new WaitUntil(() => !inTurnadoSteal);
            PatchBattleControlRelay.waitForEffects--;
        }
        static IEnumerator TurnadoItemSteal(int targetID, int relayTarg)
        {
            inTurnadoSteal = true;
            Entity_Ext extEnt = Entity_Ext.GetEntity_Ext(battle.enemydata[targetID].battleentity);
            if (extEnt.itemId > -1)
            {
                PlaySound("Fall");
                SpriteRenderer stolenItem = Instantiate(extEnt.item);
                stolenItem.enabled = true;
                Vector3 relayeeCenterPos = battle.CenterPos(instance.playerdata[relayTarg], true) + new Vector3(0, 1.5f, -0.1f);
                if (instance.items[0].ToArray().Length < instance.maxitems)
                {
                    yield return ArcMovement(stolenItem.gameObject, extEnt.item.transform.position, relayeeCenterPos, default, 4f, 25f, false);
                    PlaySound("ItemGet0");
                    instance.items[0].Add(extEnt.itemId);
                    instance.playerdata[relayTarg].battleentity.animstate = (int)Animations.ItemGet;
                    yield return new WaitForSeconds(0.65f);
                    Destroy(stolenItem.gameObject);
                }
                else
                {
                    battle.ItemDrop(extEnt.item.gameObject.transform);
                }
                extEnt.itemId = -1;
                Destroy(extEnt.item.gameObject);
            }
            inTurnadoSteal = false;
        }
        static void TurnadoDamage(int targetID)
        {
            List<DamageOverride> overrides = new List<DamageOverride>() {
                    (DamageOverride)NewDamageOverride.FlipNoPierce,
                    (DamageOverride)NewDamageOverride.Pierce1,
                    (DamageOverride)NewDamageOverride.Pierce1,
                    (DamageOverride)NewDamageOverride.Pierce1
                };
            battle.DoDamage(null, ref battle.enemydata[targetID], 2, null, overrides.ToArray(), false);
        }
        static int FindTurnadoTarget(out int stealableID)
        {
            int targetID = battle.enemydata.Length - 1;
            stealableID = -1;
            while (targetID >= 0)
            {
                if (battle.enemydata[targetID].hp > 0)
                {
                    if (Entity_Ext.GetEntity_Ext(battle.enemydata[targetID].battleentity).itemId != -1)
                    {
                        stealableID = targetID;
                    }
                    if (battle.enemydata[targetID].position != BattlePosition.Underground)
                    {
                        break;
                    }
                }
                targetID--;
            }
            return targetID;
        }

        // corkscrew
        public static IEnumerator DoCorkscrew(int relayTarg)
        {
            PatchBattleControlRelay.waitForEffects++;
            EntityControl relayedBug = instance.playerdata[relayTarg].battleentity;
            Entity_Ext extEnt = Entity_Ext.GetEntity_Ext(relayedBug);

            Vector3 targCenterPos = battle.CenterPos(instance.playerdata[relayTarg], true);
            targCenterPos.y += 0.5f + 0.75f * (extEnt.corkscrewRelays / CORKSCREW_MAX);
            targCenterPos.z -= 0.5f;
            SpriteRenderer spriteRenderer = NewSpriteObject("effect", targCenterPos, Vector3.zero, null, itemsprites[1, (int)Medal.Corkscrew], spritematlit);
            spriteRenderer.color = Color.white;

            Vector3 startPos = spriteRenderer.transform.position;
            Vector3 targetPos = startPos;
            targetPos.y += 0.75f / 4f;
            Vector3 startRot = spriteRenderer.transform.localEulerAngles;
            Vector3 targetAngle = startRot;
            targetAngle.y += 180f;
            float a = 0;
            float b = 30f;
            PlaySound("Creak", -1, 1.35f + extEnt.corkscrewRelays * 0.15f, 1);
            do
            {
                spriteRenderer.transform.position = Vector3.Lerp(startPos, targetPos, a / b);
                spriteRenderer.transform.localEulerAngles = Vector3.Lerp(startRot, targetAngle, a / b);
                a += framestep;
                yield return null;
            }
            while (a < b + 6f);

            extEnt.corkscrewRelays++;
            if (extEnt.corkscrewRelays >= CORKSCREW_MAX)
            {
                extEnt.corkscrewRelays = 0;
                PlaySound("Pop", -1, 0.8f, 1);
                int extraTurns = BadgeHowManyEquipped((int)Medal.Corkscrew, instance.playerdata[relayTarg].trueid);
                BattleControl_Ext.Instance.HustleUp(ref instance.playerdata[relayTarg], extraTurns, 0.4f, immediately: true);
                relayedBug.animstate = (int)Animations.Jump;
                relayedBug.Jump();
                targetAngle = spriteRenderer.transform.localEulerAngles;
                targetAngle.z += 45f;
                targetPos = spriteRenderer.transform.position + new Vector3(0.33f, 0.67f, 0) * 1.5f;
                a = 0;
                b = 12f;
                do
                {
                    spriteRenderer.transform.position = Vector3.Lerp(spriteRenderer.transform.position, targetPos, a / b);
                    spriteRenderer.transform.localEulerAngles = Vector3.Lerp(spriteRenderer.transform.localEulerAngles, targetAngle, a / b);
                    a += framestep;
                    yield return null;
                }
                while (a < b + 1f || !relayedBug.onground);
            }
            PatchBattleControlRelay.waitForEffects--;

            yield return MainManager_Ext.LerpSpriteColor(spriteRenderer, 15, new Color(1, 1, 1, 0));
            Destroy(spriteRenderer.gameObject);
        }

        //carousel
        public static void DoCarousel(int playerId)
        {
            int carouselsEquipped = BadgeHowManyEquipped((int)Medal.Carousel, instance.playerdata[playerId].trueid);
            if(carouselsEquipped > 0 && instance.playerdata[playerId].hp > 0)
            {
                EntityControl entity = instance.playerdata[playerId].battleentity;
                int dizzyTurns = carouselsEquipped;

                battle.StartCoroutine(battle.ItemSpinAnim(entity.transform.position + Vector3.up, itemsprites[1, (int)Medal.Carousel], true));

                BattleControl_Ext.Instance.TryDizzy(null, ref instance.playerdata[playerId], dizzyTurns);
                instance.playerdata[playerId].tired--;
                Entity_Ext ext = Entity_Ext.GetEntity_Ext(entity);
                ext.cantSwap = true;
                ext.canBypassSwapRestrictions = false;

            }
        }

        // spinout
        public static IEnumerator DoSpinout(BattleData spinnerOuter, Entity_Ext spinnerOuterExt, bool isPlayer)
        {
            PlaySound("Toss8", -1, 0.6f, 0.7f);
            PlaySound("Toss8", -1, 1f, 0.7f);
            PlaySound("Toss8", -1, 1.4f, 0.7f);
            GameObject hurricanePart = PlayParticle(NewParticle.SpinoutHurricane.ToString(), spinnerOuter.battleentity.transform.position + new Vector3(0, spinnerOuter.battleentity.height - 1f));
            hurricanePart.transform.parent = spinnerOuter.battleentity.spritetransform;
            hurricanePart.transform.localScale = Vector3.one;
           
            int spinouts = isPlayer ? BadgeHowManyEquipped((int)Medal.Spinout, spinnerOuter.trueid) : 1;
            Vector3 startPos = spinnerOuter.battleentity.transform.position;
            float a = 0;
            float b = 30f;
            float c = 80f;
            do
            {
                float prog = a / c;
                hurricanePart.transform.localScale = Vector3.one;
                hurricanePart.transform.position = spinnerOuter.battleentity.transform.position + new Vector3(0, spinnerOuter.battleentity.height - 1f) * (1f - prog);
                spinnerOuter.battleentity.spin = Vector3.up * Mathf.Lerp(15f, 25f + spinouts * 5f, Mathf.Pow(a / c, 2.5f));

                if (a < b)
                {
                    float posSkew = Mathf.InverseLerp(0, b, a);
                    posSkew = 1f + Utils.RotateVector(Vector2.left, 180f * posSkew).x;
                    spinnerOuter.battleentity.transform.position = new Vector3(startPos.x + posSkew, startPos.y, startPos.z);
                }
                else
                {
                    float posSkew = Mathf.InverseLerp(b, c, a);
                    posSkew = Utils.RotateVector(Vector2.right * 2f, 180f * posSkew).x;
                    spinnerOuter.battleentity.transform.position = new Vector3(startPos.x + posSkew, startPos.y, startPos.z);
                }
                a += framestep;
                yield return null;
            }
            while (a < c + 1f);
            spinnerOuter.battleentity.spin = Vector3.zero;
            PlaySound("Drop", 0.7f, 1.1f);
            yield return null;

            Destroy(hurricanePart, 2);
            SpinoutEffects(ref spinnerOuter, isPlayer, spinnerOuterExt.dizzyRecoil);

            a = 0;
            b = 40f;
            do
            {
                float posSkew = Mathf.Pow(a/b, 0.5f);
                posSkew = 1f + Utils.RotateVector(Vector2.left, 180f * posSkew).x - 2f;
                spinnerOuter.battleentity.transform.position = new Vector3(startPos.x + posSkew, startPos.y, startPos.z);
                a += framestep;
                yield return null;
            } while (a < b);
            spinnerOuterExt.diedFromDizzy = true;
            spinnerOuter.battleentity.transform.position = startPos;
            spinnerOuterExt.dizzyRecoil = 0;
            spinnerOuterExt.didSpinout = true;
            battle.RefreshAllData();
            battle.UpdateEntities();
        }
        public static void SpinoutEffects(ref BattleData spinnerOuter, bool isPlayer, int recoil)
        {
            if (isPlayer)
            {
                if (instance.playerdata.Length <= 1)
                    return;

                int spinouts = BadgeHowManyEquipped((int)Medal.Spinout, spinnerOuter.trueid);
                BattleControl_Ext.Instance.RecoverPlayerTP(recoil * spinouts, spinnerOuter);
                for (int i = 0; i < instance.playerdata.Length; i++)
                {
                    if (instance.playerdata[i].hp <= 0 || battle.IsStopped(instance.playerdata[i]) 
                        || instance.playerdata[i].trueid == spinnerOuter.trueid)
                        continue;
                    BattleControl_Ext.Instance.HustleUp(ref instance.playerdata[i], spinouts, 0.4f, immediately: true);
                }
            }
            else
            {
                if (battle.enemydata.Length <= 1)
                    return;

                int spinouts = 1;
                BattleControl_Ext.Instance.RecoverEnemyTp(recoil, spinnerOuter.battleentity.battleid);
                for (int i = 0; i < battle.enemydata.Length; i++)
                {
                    if (battle.enemydata[i].hp <= 0 || battle.IsStopped(battle.enemydata[i]) || i == spinnerOuter.battleentity.battleid)
                        continue;
                    BattleControl_Ext.Instance.HustleUp(ref battle.enemydata[i], spinouts, 0.4f, immediately: true);
                }
            }
        }

        // - - - - - - - - - - - - - - - - - - - -
        // visuals
        public static IEnumerator DizzyWhirl(EntityControl target, float particleRate)
        {
            GameObject hurricanePart = PlayParticle("HurricaneBig", target.transform.position + new Vector3(0, target.height - 1f));
            hurricanePart.transform.parent = target.spritetransform;
            hurricanePart.transform.localScale = Vector3.one;
            ParticleSystem particleSystem = hurricanePart.GetComponent<ParticleSystem>();
            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.angle = 15;
            shape.radius = 0.075f;
            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTime = 12.5f * particleRate;
            ParticleSystem.ColorOverLifetimeModule colOverLifetime = particleSystem.colorOverLifetime;
            colOverLifetime.enabled = true;
            ParticleSystem.MinMaxGradient colorGradient = colOverLifetime.color;
            colorGradient.mode = ParticleSystemGradientMode.Gradient;
            colorGradient.gradientMin = colorGradient.gradientMax = colorGradient.gradient = new Gradient()
            {
                mode = GradientMode.Blend,
                colorKeys = new GradientColorKey[] {
                    new GradientColorKey(DizzyColors[0], 0.02f),
                    new GradientColorKey(DizzyColors[1], 0.04f),
                    new GradientColorKey(DizzyColors[2], 0.07f),
                    new GradientColorKey(DizzyColors[3], 0.10f),
                    new GradientColorKey(DizzyColors[4], 0.13f),
                }
            };
            ParticleSystem.MainModule mainModule = particleSystem.main;
            mainModule.startColor = colorGradient;
            mainModule.startLifetime = 0.8f;
            yield return new WaitForSeconds(0.4f);

            emission.enabled = false;
            Destroy(hurricanePart, 4f);
        }

        public static void DizzyFlipParticle(BattleData target)
        {
            PlaySound("Fail", UnityEngine.Random.Range(0.8f, 1.2f), 1);
            GameObject obj = Instantiate(Resources.Load("Prefabs/Particles/Mistake"), battle.CenterPos(target, true), Quaternion.Euler(-90f, 0f, 0f)) as GameObject;
            ParticleSystem ps = obj.GetComponent<ParticleSystem>();
            ParticleSystem.ColorOverLifetimeModule colOverLifetime = ps.colorOverLifetime;
            colOverLifetime.enabled = true;
            ParticleSystem.MinMaxGradient colorGradient = colOverLifetime.color;
            colorGradient.mode = ParticleSystemGradientMode.Gradient;
            colorGradient.gradientMin = colorGradient.gradientMax = colorGradient.gradient = new Gradient()
            {
                mode = GradientMode.Blend,
                colorKeys = new GradientColorKey[] {
                    new GradientColorKey(DizzyColors[0], 0),
                    new GradientColorKey(DizzyColors[1], 0.04f),
                    new GradientColorKey(DizzyColors[2], 0.08f),
                    new GradientColorKey(DizzyColors[3], 0.12f),
                    new GradientColorKey(DizzyColors[4], 0.16f),
                }
            };
            ParticleSystem.MainModule mainModule = ps.main;
            mainModule.startColor = colorGradient;
            Destroy(obj, 1.5f);
        }
    }
}