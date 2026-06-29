using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MainManager;
namespace BFPlus.Extensions.EnemyAI
{
    public class FireAntAI : AI
    {
        const int FIREBALL_DAMAGE = 5;
        int lastThrowType;
        int throwTypeRepeats;

        public override IEnumerator DoBattleAI(EntityControl entity, int actionid)
        {
            battle.SetData(actionid, 1);

            battle.nonphyscal = true;

            float ATK_SPD = 1f;
            if (MainManager.HasCondition(MainManager.BattleCondition.Fire, battle.enemydata[actionid]) > -1)
            {
                ATK_SPD += 0.3f;
                if (MainManager.BadgeIsEquipped((int)Medal.HeatingUp))
                {
                    Entity_Ext extEnt = Entity_Ext.GetEntity_Ext(entity);
                    ATK_SPD += extEnt.fireDamage * 0.05f;
                }
            }
            int volley_amount = Mathf.Max(0, 1 - battle.enemydata[actionid].cantmove);
            int max_charge = BattleControl_Ext.Instance.GetMaxEnemyCharge(entity);
            int currentCharge = battle.enemydata[actionid].charge;
            battle.enemydata[actionid].charge = 0;

            for (int v = 0; v < volley_amount; v++)
            {
                if (MainManager.GetAlivePlayerAmmount() == 0)
                    break;

                if (Random.value < 0.1f && v < volley_amount - 1 && currentCharge <= 0)
                {
                    currentCharge = max_charge;
                    if (Random.value < 0.2f)
                    {
                        currentCharge = Mathf.CeilToInt(currentCharge / 2f);
                    }
                    if (currentCharge > 0)
                    {
                        yield return DoCharge(entity, actionid, currentCharge);
                        continue;
                    }
                }

                int throwType = TryThrowType(Random.value < 0.5f ? 0 : 1); // 50-50 shot between lob and straight throw
                float pitch = 0.7f + throwType * 0.3f;

                if (currentCharge <= Mathf.CeilToInt(max_charge / 2f) && // always uses normal throws if it has more than 2 charge
                        (Random.value < 0.2f || currentCharge > 0))      // always uses double-throw if it has 1-2 charge
                {                                                                            // without charge, it's a 1 in 5 charge of using double-throw
                    throwType = TryThrowType(2);
                    pitch = 1.3f;
                }
                bool doubleFireBall = throwType == 2;

                MainManager.PlaySound("WaspKingMFireball1", pitch, 1f);
                MainManager.PlaySound("Blosh", pitch, 1f);
                entity.animstate = 100;
                yield return new WaitForSeconds(0.15f / ATK_SPD);
                entity.animstate = 103;

                Vector3 startPos = entity.transform.position + new Vector3(0.1f, 1.75f);
                List<Transform> newFireballs = new List<Transform>();
                for (int i = doubleFireBall ? 2 : 1; i > 0; i--)
                {
                    newFireballs.Add((Object.Instantiate(Resources.Load("Prefabs/Particles/Fireball"), startPos, Quaternion.identity, battle.battlemap.transform) as GameObject).transform);
                }

                float scaleFac = doubleFireBall ? 0.7f : 1f;
                float a = 0f;
                float b = 20f;
                Vector3 targetScale = Vector3.one * 0.75f * scaleFac;
                Vector3 rotate = throwType == 2 ? Vector3.left * 0.4f : Vector3.up * 0.6f;
                do
                {
                    float prog = Mathf.Pow(Mathf.Clamp01(a / b), 0.35f);
                    for (int f = 0; f < newFireballs.Count; f++)
                    {
                        newFireballs[f].position = startPos + (Vector3)Utils.RotateVector(rotate, 360f * prog + Mathf.Lerp(0, 360, f / (float)newFireballs.Count)) * (throwType == 2 ? prog : Mathf.Sin(prog * Mathf.PI));
                        newFireballs[f].localScale = Vector3.Lerp(Vector3.zero, targetScale, a / b);
                    }
                    a += MainManager.framestep * ATK_SPD;
                    yield return null;
                }
                while (a < b + 1f);

                yield return new WaitForSeconds(0.1f / ATK_SPD);
                entity.animstate = 101;
                MainManager.PlaySound("Chew", Mathf.Lerp(ATK_SPD, 1, 0.9f), 1f);
                yield return new WaitForSeconds(0.1f / ATK_SPD);

                float TRAVEL_TIME = throwType == 0 ? 20f : 30f;
                float ARC_HEIGHT = throwType == 0 ? 0.35f : 2.5f;
                int DMG = Mathf.Max(1, FIREBALL_DAMAGE - 2 * (newFireballs.Count - 1)) + currentCharge;
                currentCharge = 0;

                for (int i = 0; i < newFireballs.Count; i++)
                {
                    if (MainManager.GetAlivePlayerAmmount() == 0)
                    {
                        for (int j = i; j < newFireballs.Count; j++)
                        {
                            battle.StartCoroutine(BattleControl_Ext.LerpScale(15, newFireballs[i].localScale, Vector3.zero, newFireballs[i]));
                            Object.Destroy(newFireballs[i].gameObject, 5);
                        }
                        break;
                    }

                    battle.StartCoroutine(battle.Projectile(DMG, BattleControl.AttackProperty.Fire, battle.enemydata[actionid],
                        battle.GetRandomAvaliablePlayer(), newFireballs[i],
                        TRAVEL_TIME, ARC_HEIGHT, "SepPart@2@4", "Fire", "WaspKingMFireball2",
                        null, Vector3.zero, false));

                    TRAVEL_TIME += 16f;
                    ARC_HEIGHT += 2f;
                }

                yield return new WaitForSeconds(0.5f / ATK_SPD);
                entity.animstate = 0;
                yield return new WaitForSeconds(v == volley_amount - 1 ? 0.2f / ATK_SPD : 0);
            }


            battle.enemydata[actionid].cantmove = 0;
            battle.enemydata[actionid].data[0] = 1;
            yield return EventControl.quartersec;
        }

        IEnumerator DoCharge(EntityControl entity, int actionid, int currentCharge)
        {
            entity.StartCoroutine(entity.ShakeSprite(0.1f, 45f));
            MainManager.PlaySound("Charge7");
            yield return EventControl.halfsec;
            battle.StartCoroutine(BattleControl_Ext.Instance.MultiArrow(battle.enemydata[actionid].battleentity, 4, currentCharge, (0.15f + currentCharge * 0.05f) / (float)currentCharge));
            yield return EventControl.quartersec;
        }

        public static void FireAntHustle(ref BattleData target)
        {
            battle.StartCoroutine(battle.StatEffect(target.battleentity, 5));
            float pitch = 1 - target.cantmove + target.moreturnnextturn;
            pitch = 0.75f + 0.05f * pitch;
            battle.SetData(target.battleentity.battleid, 1);
            BattleControl_Ext.Instance.HustleUp(ref target, 1, 0, target.data[0] == 0, sfxPitch:pitch);
        }

        // prevents the same throw type from repeating too many times
        // i had fire ants use the same throw like 7 times in a row, MULTIPLE TIMES
        public int TryThrowType(int tryType)
        {
            if (tryType == lastThrowType)
            {
                throwTypeRepeats++;
                if (throwTypeRepeats >= 3)
                {
                    throwTypeRepeats = 0;
                    tryType += Random.value < 0.5f ? -1 : 1;
                    if (tryType < 0)
                    {
                        tryType = 2;
                    }
                    else if (tryType > 2)
                    {
                        tryType = 0;
                    }
                }
            }
            lastThrowType = tryType;
            return tryType;
        }

    }
}
