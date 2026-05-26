using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MainManager;
using static BattleControl;
using static BFPlus.Extensions.BattleControl_Ext;
using static UnityEngine.Object;

namespace BFPlus.Extensions.BattleStuff.Skills
{
    internal class StealToss
    {
        static float hitRegLeniency = 1f;
        static int successfullHits;
        static int didDamage;
        static Vector3 viCenterPos;
        static Vector3 rangDest;
        static int targetID;
        static float floorRicochet;
        static int totalBerries;
        static bool didNotHitAnyEnemy;
        static SpriteRenderer enemyItem;

        static int hits;
        static int baseATK;
        static AttackProperty property;
        static List<DamageOverride> overrides;

        internal static IEnumerator DoStealToss(EntityControl vi)
        {
            vi.animstate = 103;
            yield return new WaitForSeconds(0.17f);
            vi.animstate = 104;

            BattleData target = battle.avaliabletargets[battle.target];
            targetID = target.battleentity.battleid;
            hits = 2 + BadgeHowManyEquipped((int)BadgeTypes.Beemerang2);
            baseATK = Mathf.FloorToInt(battle.GetPlayerAttack(vi.animid, true) * 0.75f);
            property = AttackProperty.None;
            overrides = new List<DamageOverride>() { (DamageOverride)NewDamageOverride.Beemerang };

            successfullHits = 0;
            didDamage = 0;
            enemyItem = null;

            viCenterPos = battle.CenterPos(MainManager.instance.playerdata[vi.battleid], true);
            GameObject beemerang = Instantiate(Resources.Load("Prefabs/Objects/BeerangBattle") as GameObject);
            beemerang.transform.position = viCenterPos;
            beemerang.transform.localScale = Vector3.zero;
            beemerang.transform.localEulerAngles = new Vector3(90f, 0, 0);
            beemerang.AddComponent<SpinAround>().itself = new Vector3(0f, 0f, 20f);

            Transform crosshair = battle.TempCrosshair(target, groundpos: false);
            SpriteRenderer[] reticles = new SpriteRenderer[3];
            for (int i = 0; i < reticles.Length; i++)
            {
                reticles[i] = NewUIObject("crosshair", null, default).AddComponent<SpriteRenderer>();
                reticles[i].gameObject.layer = 15;
                if (i == 0)
                {
                    reticles[i].sprite = guisprites[41];
                    continue;
                }
                reticles[i].transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, i / (reticles.Length + 1f)) * 0.75f;
                reticles[i].sprite = guisprites[42];
            }

            float accelRate = 0;
            float accel = 0;
            PlaySound("Crosshair", 9, 0.9f, 0.35f, loop: true);
            do
            {
                reticles[0].transform.position = viCenterPos + new Vector3(
                    1f + Mathf.Abs(Mathf.Cos(Mathf.PI * (Time.time + accel))) * 2f,
                    Mathf.Sin(Mathf.PI * (Time.time + accel)) * 2.55f,
                    0f);

                for (int r = 0; r < reticles.Length; r++)
                {
                    reticles[r].transform.Rotate(0, 0, framestep * -6f);
                    if (r > 0) reticles[r].transform.position = Vector3.Lerp(viCenterPos, reticles[0].transform.position, Mathf.InverseLerp(0, reticles.Length, r));
                }

                sounds[9].pitch = Mathf.Lerp(0.9f, 2f, Mathf.InverseLerp(0.2f, 1, Mathf.Pow(accelRate, 3f)));
                accelRate += 1 / 2400f;
                accel += framestep * 0.1f * Mathf.InverseLerp(0.2f, 1, Mathf.Pow(accelRate, 3f));

                yield return null;
            }
            while (!GetKey(4) && accelRate < 1);

            //Waited too long before pressing a, get...this...
            if (accelRate >= 1)
            {
                yield return DoStealTossPunishment(vi, reticles, beemerang, crosshair);
                yield break;
            }

            for (int r = 0; r < reticles.Length; r++)
                reticles[r].gameObject.SetActive(false);
            crosshair.gameObject.SetActive(false);

            vi.animstate = 105;
            StopSound(9);
            beemerang.transform.localScale = Vector3.one;
            yield return new WaitForSeconds(0.05f);

            PlaySound("Woosh", 8, 1.1f, 1f, true);

            Vector3 throwDir = (reticles[0].transform.position - viCenterPos).normalized;

            rangDest = viCenterPos + throwDir * (crosshair.position - viCenterPos).magnitude * 1.1f;
            Vector3[] trickshotSpots = null;
            Vector3 spinAround = rangDest - viCenterPos;

            didNotHitAnyEnemy = true;
            floorRicochet = -1;

            // initial fly-out
            float a = 0f;
            float b = (crosshair.position - viCenterPos).magnitude * 3f;
            do
            {
                if (trickshotSpots == null)
                {
                    beemerang.transform.position = Vector3.Lerp(viCenterPos, rangDest, a / b);
                    if (beemerang.transform.position.y < 0.2f)
                    {
                        b *= 1.2f;
                        floorRicochet = a / b;
                        Vector3 ricochetAngle = spinAround * (1f - floorRicochet);
                        ricochetAngle.y *= -1f;
                        trickshotSpots = new Vector3[]
                        {
                            beemerang.transform.position,
                            rangDest = beemerang.transform.position + ricochetAngle
                        };
                        PlaySound("WoodHit", 0.5f);
                    }
                }
                else
                {
                    beemerang.transform.position = (a / b <= floorRicochet) ?
                        Vector3.Lerp(viCenterPos, trickshotSpots[0], a / b / floorRicochet) :
                        Vector3.Lerp(trickshotSpots[0], trickshotSpots[1], Mathf.InverseLerp(floorRicochet, 1, a / b));
                }

                if (successfullHits == 0 && Vector2.Distance(beemerang.transform.position, crosshair.position) <= hitRegLeniency)
                {
                    didNotHitAnyEnemy = false;
                    DealStealTossDamage();
                    StartStylishTimer(0, 15f, stylishGain: floorRicochet > -1 ? 0.04f : 0.02f, commandSuccess: false);
                    successfullHits++;
                    if (a < b) // on hit, immediately moves on
                    {
                        if (floorRicochet > -1)
                            floorRicochet /= a / b;
                        b *= a / b;
                        rangDest = beemerang.transform.position;
                    }
                }

                a += framestep;

                yield return null;
            }
            while (a < b + 1f);

            if (hits > 2) // extra hits
            {
                yield return DoExtraHits(beemerang, crosshair);
            }

            Vector3[] trickshotSpots2;
            if (trickshotSpots == null)
            {
                spinAround = (rangDest - viCenterPos).normalized * 4f;
                trickshotSpots2 = null;
            }
            else
            {
                spinAround = (trickshotSpots[1] - trickshotSpots[0]).normalized * 4f;
                trickshotSpots2 = new Vector3[0];
            }

            a = 0;
            float c = 40f;
            do // final hit
            {
                float f = Mathf.Max(0, Mathf.Sin(Mathf.PI * a / c));
                if (trickshotSpots2 == null || trickshotSpots2.Length == 0)
                {
                    beemerang.transform.position = rangDest + spinAround * f;
                    if (trickshotSpots2 == null && beemerang.transform.position.y < 0.2f && a / c < 0.4f)
                    {
                        c *= 1.2f;
                        floorRicochet = Mathf.Sin(Mathf.PI * a / c);
                        Vector3 ricochetAngle = spinAround * (1f - floorRicochet);
                        ricochetAngle.y *= -1f;
                        trickshotSpots2 = new Vector3[] { beemerang.transform.position, beemerang.transform.position + ricochetAngle };
                        PlaySound("WoodHit", 0.5f);
                    }
                }
                else
                {
                    beemerang.transform.position = (f <= floorRicochet) ?
                        Vector3.Lerp(rangDest, trickshotSpots2[0], f / floorRicochet) :
                        Vector3.Lerp(trickshotSpots2[0], trickshotSpots2[1], Mathf.InverseLerp(floorRicochet, 1, f));
                }

                a += framestep;
                yield return null;
            }
            while (a < c + 1f);

            yield return StealItem(beemerang, crosshair, b, trickshotSpots);

            StopSound(8, 0.1f);

            for (int i = 0; i < reticles.Length; i++)
                Destroy(reticles[i].gameObject);
            Destroy(beemerang.gameObject);
            Destroy(crosshair.gameObject);

            bool noInventorySpace = MainManager.instance.items[0].Count >= MainManager.instance.maxitems;
            AddItem(vi, noInventorySpace);

            if (totalBerries > 0)
                MainManager.instance.showmoney = 200f + totalBerries * 10f;

            a = 0;
            c = 50f;
            while (totalBerries > 0 || a < c)
            {
                if (totalBerries > 0)
                {
                    totalBerries--;
                    PlaySound("Money");
                    if (MainManager.instance.money < 999)
                        MainManager.instance.money++;
                }
                if (noInventorySpace && a >= c / 2f)
                {
                    noInventorySpace = false;
                    vi.animstate = (int)Animations.Flustered;
                }
                a += framestep;
                yield return null;
            }
            Destroy(enemyItem?.gameObject);

            yield return WaitStylish(0.1f);
        }

        static void AddItem(EntityControl vi, bool noInventorySpace)
        {
            if (enemyItem == null)
            {
                if (battle.commandsuccess || (didNotHitAnyEnemy && Instance.stylishCountThisAction > 0))
                    vi.animstate = (int)Animations.Happy;
                else
                {
                    vi.animstate = (int)Animations.Angry;
                    PlaySound("Fail");
                }
            }
            else
            {
                Entity_Ext targExt = Entity_Ext.GetEntity_Ext(battle.enemydata[targetID].battleentity);

                if (battle.caller != null)
                    NPCControl_Ext.GetNPCControl_Ext(battle.caller).items[battle.target] = -1;

                if (!noInventorySpace)
                {
                    PlaySound("ItemGet0");
                    vi.animstate = (int)Animations.ItemGet;
                    MainManager.instance.items[0].Add(targExt.itemId);
                    enemyItem.transform.parent = battle.battlemap.transform;
                    enemyItem.transform.position = viCenterPos + new Vector3(0, 1, -0.1f);
                }
                else
                {
                    PlaySound("Fail");
                    vi.animstate = (int)Animations.Surprized;
                }

                targExt.itemId = -1;
            }
        }

        static IEnumerator DoExtraHits(GameObject beemerang, Transform crosshair)
        {
            float r = 10f;
            float a = Mathf.Max(0, hits - 2) * 360f;
            float hitCooldown = 170f;
            Vector3 spinAround = (rangDest - viCenterPos).normalized;
            spinAround = beemerang.transform.position + new Vector3(-spinAround.y, spinAround.x, spinAround.z) * 2f;
            Vector3 spinDir = beemerang.transform.position - spinAround;
            while (a > -r)
            {
                spinDir = Utils.RotateVector(spinDir, -Mathf.Min(r, Mathf.Max(a, 0)));
                beemerang.transform.position = spinAround + spinDir;
                if (successfullHits < hits - 1)
                {
                    if (hitCooldown > 0)
                    {
                        hitCooldown -= r;
                    }
                    else if (Vector2.Distance(beemerang.transform.position, crosshair.position) <= hitRegLeniency)
                    {
                        hitCooldown = 170f;
                        DealStealTossDamage();
                        successfullHits++;

                        float gain;
                        if (Instance.stylishCountThisAction == 0 && didNotHitAnyEnemy)
                            gain = (floorRicochet > -1) ? 0.20f : 0.14f;
                        else
                            gain = (floorRicochet > -1) ? 0.04f : 0.02f;
                        StartStylishTimer(0, 15f, stylishGain: gain, commandSuccess: false);
                    }
                }
                a -= r;
                yield return null;
            }
        }

        static IEnumerator StealItem(GameObject beemerang, Transform crosshair, float startTime, Vector3[] trickshotSpots)
        {
            List<Rigidbody> berries = new List<Rigidbody>();
            List<Vector3> berryOffsets = new List<Vector3>();
            totalBerries = 0;

            if (Vector2.Distance(beemerang.transform.position, crosshair.position) <= hitRegLeniency)
            {
                DealStealTossDamage();
                successfullHits++;

                Entity_Ext entityExt = Entity_Ext.GetEntity_Ext(battle.enemydata[targetID].battleentity);

                battle.commandsuccess = successfullHits == hits;
                StartStylishTimer(0, 15f, stylishID: 1, stylishGain: (floorRicochet > -1) ? 0.08f : 0.06f);

                float stealPitch = 1f;

                totalBerries = successfullHits + UnityEngine.Random.Range(0, 4) +
                        + BadgeHowManyEquipped((int)BadgeTypes.BerryFinder) * 2
                        + BadgeHowManyEquipped((int)BadgeTypes.Beemerang2) * 2;

                if (battle.commandsuccess && entityExt.itemId != -1)
                {
                    enemyItem = Instantiate(entityExt.item);
                    enemyItem.enabled = true;
                    Destroy(entityExt.item.gameObject);
                    stealPitch += 0.3f;
                }

                stealPitch += totalBerries * 0.02f;

                int spend = totalBerries;
                while (spend > 0)
                {
                    berryOffsets.Insert(0, UnityEngine.Random.insideUnitCircle);
                    berries.Insert(0, new GameObject().AddComponent<Rigidbody>());
                    Items i;
                    if (spend >= 10)
                    {
                        spend -= 10;
                        i = Items.MoneyBig;
                        berryOffsets[0] *= 0.5f;
                    }
                    else if (spend >= 5)
                    {
                        spend -= 5;
                        i = Items.MoneyMedium;
                        berryOffsets[0] *= 0.75f;
                    }
                    else
                    {
                        spend--;
                        i = Items.MoneySmall;
                    }
                    berryOffsets[0] = new Vector3(berryOffsets[0].x, Mathf.Abs(berryOffsets[0].y) - 0.2f, berryOffsets[0].z) * 0.7f;
                    berries[0].gameObject.AddComponent<SpriteRenderer>().sprite = itemsprites[0, (int)i];
                    berries[0].transform.parent = battle.battlemap.transform;
                    berries[0].transform.position = beemerang.transform.position;
                    berries[0].transform.localScale = Vector3.one;
                }

                PlaySound("ItemStolen", stealPitch, 1);
            }

            //beemerang returns to vi
            float a = startTime;
            bool woodHit = false;
            do
            {
                if (trickshotSpots == null)
                {
                    beemerang.transform.position = Vector3.Lerp(viCenterPos, rangDest, a / startTime);
                }
                else if (a / startTime > floorRicochet)
                {
                    beemerang.transform.position = Vector3.Lerp(trickshotSpots[0], trickshotSpots[1], Mathf.InverseLerp(floorRicochet, 1, a / startTime));
                }
                else
                {
                    if (!woodHit)
                    {
                        PlaySound("WoodHit", 0.5f);
                        woodHit = true;
                    }
                    beemerang.transform.position = Vector3.Lerp(viCenterPos, trickshotSpots[0], a / startTime / floorRicochet);
                }

                if (enemyItem != null)
                    enemyItem.transform.position = beemerang.transform.position + Vector3.up * 0.1f;

                if (berries.Count > 0)
                {
                    for (int s = 0; s < berries.Count; s++)
                        berries[s].transform.position = beemerang.transform.position + Vector3.up * 0.1f + berryOffsets[s];
                }
                a -= framestep;
                yield return null;
            }
            while (a > -1f);

            for (int i = 0; i < berries.Count; i++)
            {
                Destroy(berries[i].gameObject);
            }
        }

        static void DealStealTossDamage()
        {
            if (battle.enemydata[targetID].position != BattlePosition.Underground && battle.enemydata[targetID].hp > 0)
            {
                if (didDamage > 0)
                {
                    int finalDamage = baseATK;
                    if (finalDamage > 1)
                        finalDamage = Mathf.Max(1, Mathf.FloorToInt(finalDamage * (Mathf.Max(1, 3 - didDamage) / 3f)));
                    battle.DoDamage(MainManager.instance.playerdata[battle.currentturn], ref battle.enemydata[targetID], finalDamage, property, overrides.ToArray(), false);
                }
                else
                {
                    List<DamageOverride> firstHitOverrides = new List<DamageOverride>();
                    firstHitOverrides.Add((DamageOverride)NewDamageOverride.FlipNoPierce);
                    firstHitOverrides.AddRange(overrides);
                    baseATK = battle.DoDamage(MainManager.instance.playerdata[battle.currentturn], ref battle.enemydata[targetID], baseATK, property, overrides.ToArray(), false);
                }
                didDamage++;

                if (property == AttackProperty.None)
                    property = AttackProperty.NoExceptions;
            }
        }

        static IEnumerator DoStealTossPunishment(EntityControl vi, SpriteRenderer[] reticles, GameObject beemerang, Transform crosshair)
        {
            vi.animstate = 106;
            StopSound(9);
            for (int r = 0; r < reticles.Length; r++)
                Destroy(reticles[r].gameObject);

            Destroy(beemerang);
            Destroy(crosshair.gameObject);

            yield return new WaitForSeconds(0.25f);
            PlaySound("WoodHit", 0.8f, 1);
            yield return new WaitForSeconds(0.5f);

            ParticleSystem component = PlayParticle("explosion", viCenterPos + Vector3.right).GetComponent<ParticleSystem>();

            var main = component.main;
            if (component != null)
            {
                main.startColor = Color.Lerp(Color.red, Color.yellow, 0.5f);
                main.startSize = main.startSize.constant * 2.5f;
            }

            PlaySound("Explosion", 0.85f, 1.5f);
            ShakeScreen(Vector3.one, 0.5f);
            yield return new WaitForSeconds(1.6f);

            int maxBoom = 3;
            for (int BOOM = 0; BOOM < maxBoom; BOOM++)
            {
                component = PlayParticle("explosion", viCenterPos + Vector3.right).GetComponent<ParticleSystem>();
                main = component.main;

                if (component != null)
                {
                    main.startColor = Color.Lerp(Color.red, Color.yellow, 0.5f);
                    main.startSize = main.startSize.constant * (BOOM + 1f);
                }

                PlaySound("Explosion", 0.85f, 1.5f);
                ShakeScreen(Vector3.one * BOOM, BOOM + 0.5f);
                if (BOOM < 2)
                    PlaySound("Explosion", 0.75f + BOOM * 0.15f, 1.5f);
                else
                    PlaySound("Explosion", 0.6f, 2f);
                yield return new WaitForSeconds(0.3f);
            }
            yield return new WaitForSeconds(1.6f);
            yield break;
        }
    }
}
