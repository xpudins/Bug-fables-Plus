using System.Collections;
using UnityEngine;
using static UnityEngine.Object;
using static MainManager;
using static BattleControl;
using System;

namespace BFPlus.Extensions.BattleStuff.StatusStuff
{
    public static class Sleep
    {
        // medal effects
        public static void DoYawnCheck(BattleData entity, BattleCondition condition)
        {
            int yawnBug = BattleControl_Ext.GetEquippedMedalBug(Medal.Yawn);
            if (yawnBug == -1 || (entity.battleentity.playerentity && entity.trueid == instance.playerdata[yawnBug].trueid))
            {
                return;
            }

            if (condition == BattleCondition.Sleep && HasCondition(BattleCondition.Sturdy, instance.playerdata[yawnBug]) == -1)
                SetCondition(BattleCondition.Sleep, ref instance.playerdata[yawnBug], 2);
        }

        public static void CheckSleepSchedule()
        {
            for (int i = 0; i != instance.playerdata.Length; i++)
            {
                var entityExt = Entity_Ext.GetEntity_Ext(instance.playerdata[i].battleentity);
                if (entityExt.sleepScheduleTurns == 1 && entityExt.sleepScheduled)
                {
                    entityExt.sleepScheduled = false;
                    PlaySound("Sleep");
                    SetCondition(BattleCondition.Sleep, ref instance.playerdata[i], 3);
                }
                else
                {
                    entityExt.sleepScheduleTurns--;
                }
            }
        }
        public static IEnumerator DoSleepSchedule(EntityControl entity)
        {
            entity.animstate = 4;
            PlaySound("ItemHold");
            SpriteRenderer itemSprite = new GameObject().AddComponent<SpriteRenderer>();
            itemSprite.transform.position = entity.transform.position + new Vector3(0f, 2.5f, -0.1f);
            itemSprite.sprite = itemsprites[1, (int)Medal.SleepSchedule];
            itemSprite.material.renderQueue = 50000;
            itemSprite.gameObject.layer = 14;
            yield return EventControl.sec;
            Destroy(itemSprite.gameObject);
            PlaySound("Sleep");
            DeathSmoke(entity.transform.position);
            var entityExt = Entity_Ext.GetEntity_Ext(entity);
            entityExt.sleepScheduled = true;
            entityExt.sleepScheduleTurns = 1;
        }

        public static void CheckSweetDreams(BattleData target)
        {
            var battleEntity = target.battleentity;

            if (battleEntity.CompareTag("Player") && BadgeIsEquipped((int)Medal.SweetDreams, target.trueid))
            {
                var entityExt = Entity_Ext.GetEntity_Ext(target.battleentity);

                if (target.isasleep)
                {
                    entityExt.asleepTurns += 1;
                }
                else
                {
                    if (entityExt.asleepTurns != 0)
                    {
                        BattleControl_Ext.Instance.RecoverPlayerTP(entityExt.asleepTurns * 3 * BadgeHowManyEquipped((int)Medal.SweetDreams, target.trueid), target);
                        entityExt.asleepTurns = 0;
                    }
                }
            }
        }
        public static void DoBadDreams(ref BattleData target)
        {
            var damageOverrides = new DamageOverride[] { DamageOverride.NoFall, DamageOverride.NoIceBreak, DamageOverride.FakeAnim, DamageOverride.DontAwake, DamageOverride.IgnoreNumb, (DamageOverride)NewDamageOverride.StatusDamage };
            int damage = Mathf.Clamp(Mathf.CeilToInt(target.maxhp / 7.5f) - 1, 2, 3);

            battle.DoDamage(null, ref target, damage, AttackProperty.NoExceptions, damageOverrides, false);
            if (target.hp == 0)
            {
                target.battleentity.overrideanim = false;
                target.battleentity.Invoke("OverrideOver", 1f);
            }
            target.hp = Mathf.Clamp(target.hp, 1, target.maxhp);
        }
        public static IEnumerator DoNightmare(BattleControl battle, BattleData asleepPlayer)
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
                a += TieFramerate(1f);
                yield return null;
            }
            while (a < b + 1f);

            PlaySound("OmegaEye", 1.5f);
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

            PlaySound("OmegaMove");
            var startPos = hand.transform.position;
            a = 0f;
            b = 60f;
            do
            {
                hand.transform.position = SmoothLerp(startPos, targetPos, a / b);
                a += TieFramerate(1f);
                yield return null;
            }
            while (a < b + 1f);

            hand.Play("0");

            DamageOverride[] overrides = new[] { DamageOverride.NoFall, DamageOverride.DontAwake };
            if (playerTarget)
                battle.DoDamage(null, ref instance.playerdata[asleepPlayer.battleentity.battleid], 3, AttackProperty.Pierce, overrides, false);
            else
                battle.DoDamage(null, ref battle.enemydata[randomTarget.battleentity.battleid], 3, AttackProperty.Pierce, overrides, false);
            yield return EventControl.halfsec;

            hand.Play("1");
            startPos = hand.transform.position;
            targetPos = hand.transform.position + new Vector3(0, 20);
            a = 0f;
            b = 60f;
            do
            {
                hand.transform.position = SmoothLerp(startPos, targetPos, a / b);
                RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, baseAmbientColor, a / b);
                RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, baseFogColor, a / b);
                RenderSettings.skybox?.SetColor("_Tint", Color.Lerp(Color.black, baseSkyboxColor.Value, a / b));
                a += TieFramerate(1f);
                yield return null;
            }
            while (a < b + 1f);

            Destroy(hand.gameObject);
            yield return null;
        }

        public static bool CheckDestinyDream() => GetDestinyDreamBug() != -1;
        public static int GetDestinyDreamBug() => BattleControl_Ext.GetEquippedMedalBug(Medal.DestinyDream, (i) => HasCondition(BattleCondition.Sleep, instance.playerdata[i]) > -1 && instance.playerdata[i].hp > 0);
        public static bool DestinyDreamSetHP()
        {
            int destinyDreamBug = GetDestinyDreamBug();
            if (destinyDreamBug > -1)
            {
                instance.playerdata[destinyDreamBug].hp -= Mathf.Abs(instance.flagvar[0]);
                return true;
            }
            return false;
        }
        public static bool DestinyDreamChangeHud()
        {
            int destinyDreamBug = GetDestinyDreamBug();
            if (destinyDreamBug > -1)
            {
                hudsprites[destinyDreamBug].color = Color.red;
                return true;
            }
            return false;
        }
        public static void CreateDestinySkillSprite(SpriteRenderer parent)
        {
            int destinyDreamBug = GetDestinyDreamBug();
            if (destinyDreamBug != -1)
            {
                int guispritesID = instance.playerdata[destinyDreamBug].trueid + 5;
                NewUIObject("destinyDream", parent.transform, new Vector3(2.55f, 0f), new Vector3(0.45f, 0.5f, 1f) * 0.35f, guisprites[guispritesID], 11).GetComponent<SpriteRenderer>();
            }
        }
    }
}