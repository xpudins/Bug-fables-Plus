using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MainManager;

namespace BFPlus.Extensions.EnemyAI
{
    public class JumpAntFight : MonoBehaviour
    {
        public bool isSwappingPartner = false;
        public static int[] partnerIds = new int[]
        {
            (int)NewEnemies.Caveling, (int)NewEnemies.MechaJaw, (int)NewEnemies.Moeruki,
            (int)NewEnemies.Frostfly, (int)NewEnemies.Mothmite, (int)NewEnemies.BatteryShroom,
            (int)NewEnemies.PirahnaChomp, (int)NewEnemies.SplotchSpider
        };

        List<int> inventory = new List<int>()
        {
            (int)MainManager.Items.Mushroom, (int)MainManager.Items.HoneyShroom, (int)MainManager.Items.CookedShroom,
            (int)MainManager.Items.DangerShroom, (int)MainManager.Items.HoneyDanger, (int)MainManager.Items.CookedDanger,
            (int)MainManager.Items.HoneydLeaf,(int)MainManager.Items.SpicyCandy, (int)NewItem.MurkyPizza,(int)MainManager.Items.SquashPie,
            (int)MainManager.Items.BerrySmoothie, (int)MainManager.Items.JaydeStew, (int)MainManager.Items.BerryJuice, (int)MainManager.Items.CherryBomb,
            (int)MainManager.Items.Mistake, (int)MainManager.Items.ProteinShake, (int)NewItem.SquashberrySoda, (int)NewItem.GoldenLeaf,
            (int)MainManager.Items.MushroomStick, (int)NewItem.PointSwap
        };
        Dictionary<int, MainManager.BattleData?> partners = new Dictionary<int, MainManager.BattleData?>();
        int currentPartner = (int)NewEnemies.Caveling;
        public readonly Vector3 partnerPos = new Vector3(6f, 0f, 0.15f);
        void Start()
        {
            SetupDefaultPartners();
            int foundPartner = -1;
            foreach (var id in partnerIds)
            {
                foundPartner = MainManager.battle.EnemyInField(id);
                if (foundPartner != -1)
                    break;
            }

            if (foundPartner != -1)
            {
                currentPartner = MainManager.battle.enemydata[foundPartner].animid;
                partners[currentPartner] = MainManager.battle.enemydata[foundPartner];
                Entity_Ext.GetEntity_Ext(MainManager.battle.enemydata[foundPartner].battleentity).isPartner = true;
                GetPartnerBuff(ref MainManager.battle.enemydata[foundPartner]);
            }
        }

        public bool HasItem(int item = -1)
        {
            if (item == -1)
                return inventory.Count > 0;
            return inventory.Contains(item);
        }

        public int GetRandomItem()
        {
            var temp = new List<int>(inventory);
            temp.Remove((int)NewItem.PointSwap);
            int item = temp[UnityEngine.Random.Range(0, temp.Count)];
            inventory.Remove(item);
            return item;
        }

        public void RemovePointSwap()
        {
            inventory.Remove((int)NewItem.PointSwap);
        }

        void SetupDefaultPartners()
        {
            foreach (var id in partnerIds)
            {
                partners.Add(id, default);
            }
        }

        public int GetNextPartnerId()
        {
            var tempPartners = partners.Where(p => p.Key != currentPartner && (p.Value == null || p.Value.Value.hp > 0)).ToArray();
            return tempPartners.Length > 0 ? tempPartners[UnityEngine.Random.Range(0, tempPartners.Length)].Key : -1;
        }

        public bool HasPartnerAlive()
        {
            return partners.Any(p => p.Key != currentPartner && (p.Value == null || p.Value.Value.hp > 0));
        }

        public bool IsPartner(int enemyId)
        {
            return partnerIds.Contains(enemyId);
        }

        public IEnumerator SwapPartner(bool cantmove, bool dead)
        {
            isSwappingPartner = true;
            if (HasPartnerAlive())
            {
                int jumpAntId = battle.EnemyInField((int)NewEnemies.JumpAnt);
                Vector3? targetPos = null;
                if (jumpAntId == -1)
                {
                    targetPos = battle.reservedata.FirstOrDefault(r => r.animid == (int)NewEnemies.JumpAnt).battleentity?.transform?.position;
                }
                else
                {
                    targetPos = battle.enemydata[jumpAntId].battleentity.transform.position;
                }

                EntityControl partnerEntity = partners[currentPartner].Value.battleentity;
                partnerEntity.LockRigid(true);

                int nextPartner = GetNextPartnerId();
                bool diedFromDizzy = Entity_Ext.GetEntity_Ext(partnerEntity).diedFromDizzy == true;
                if (nextPartner != -1 && targetPos != null)
                {
                    partners[currentPartner] = battle.enemydata[partnerEntity.battleid];

                    if (dead)
                    {
                        yield return new WaitUntil(() => partnerEntity.deathcoroutine == null);
                    }
                    else
                    {
                        yield return MovePartner(partnerEntity, targetPos.Value, new Vector3(0, 20), Vector3.one, Vector3.zero, 30);
                        partnerEntity.gameObject.SetActive(false);
                        var temp = battle.enemydata.ToList();
                        temp.RemoveAt(partnerEntity.battleid);
                        battle.enemydata = temp.ToArray();
                    }

                    int newId;

                    if (partners[nextPartner] == null)
                    {
                        yield return EventControl.quartersec;
                        newId = battle.AddNewEnemy(nextPartner, targetPos.Value).GetComponent<EntityControl>().battleid;
                        battle.enemydata[newId].battleentity.spritetransform.gameObject.SetActive(false);
                        yield return EventControl.tenthsec;

                        Entity_Ext.GetEntity_Ext(battle.enemydata[newId].battleentity).isPartner = true;
                        BattleControl_Ext.Instance.jumpAntFightComp.GetPartnerBuff(ref battle.enemydata[newId]);
                    }
                    else
                    {
                        var temp = battle.enemydata.ToList();
                        temp.Add(partners[nextPartner].Value);
                        battle.enemydata = temp.ToArray();
                        newId = battle.enemydata.Length - 1;
                    }
                    currentPartner = nextPartner;
                    partners[currentPartner] = battle.enemydata[newId];

                    battle.enemydata[newId].position = BattleControl.BattlePosition.Ground;
                    battle.enemydata[newId].battleentity.height = 0;

                    partnerEntity = battle.enemydata[newId].battleentity;
                    if (cantmove && battle.mainturn != null || (dead && diedFromDizzy))
                    {
                        battle.enemydata[newId].cantmove = 1;
                    }

                    partnerEntity.startscale = Vector3.zero;

                    partnerEntity.LockRigid(true);
                    partnerEntity.spritetransform.gameObject.SetActive(true);
                    partnerEntity.gameObject.SetActive(true);

                    MainManager.PlaySound("Switch", 1.2f);
                    yield return MovePartner(partnerEntity, partnerPos, new Vector3(0, 20), Vector3.zero, Vector3.one, 30);
                    partnerEntity.transform.localEulerAngles = Vector3.zero;
                    partnerEntity.startscale = Vector3.one;

                    partnerEntity.LockRigid(false);
                }
            }
            isSwappingPartner = false;
        }

        public static IEnumerator MovePartner(EntityControl entity, Vector3 targetPos, Vector3 spin, Vector3 startScale, Vector3 targetScale, float endTime)
        {
            float a = 0;
            Vector3 startPos = entity.transform.position;
            entity.spin = spin;
            do
            {
                entity.transform.position = MainManager.BeizierCurve3(startPos, targetPos, 5, a / endTime);
                entity.startscale = Vector3.Lerp(startScale, targetScale, a / endTime);
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < endTime);
            entity.spin = Vector3.zero;
        }

        public void GetPartnerBuff(ref MainManager.BattleData partner)
        {
            switch (partner.animid)
            {
                case (int)NewEnemies.Caveling:
                    partner.hp += 11;
                    break;

                case (int)NewEnemies.Frostfly:
                    partner.hp += 4;
                    break;

                case (int)NewEnemies.BatteryShroom:
                    partner.hp += 6;
                    partner.moves += 1;
                    break;

                case (int)NewEnemies.PirahnaChomp:
                    partner.hp += 4;
                    break;
            }

            partner.maxhp = partner.hp;
        }

        public IEnumerator DoUseItem(EntityControl entity, int actionid, JumpAntFight fightComp)
        {
            int item = fightComp.GetRandomItem();
            yield return BattleControl_Ext.Instance.UseItem(entity, actionid, battle, (MainManager.Items)item);
            fightComp.DoTinyHugeItem(item, entity, actionid);
        }

        void DoTinyHugeItem(int item, EntityControl entity, int targetId)
        {
            switch (item)
            {
                case (int)MainManager.Items.Mushroom:
                case (int)MainManager.Items.HoneyShroom:
                    BattleControl_Ext.Instance.ApplyHuge(entity, ref battle.enemydata[targetId], 4);
                    break;

                case (int)MainManager.Items.CookedShroom:
                    BattleControl_Ext.Instance.ApplyHuge(entity, ref battle.enemydata[targetId], 6);
                    break;

                case (int)MainManager.Items.DangerShroom:
                case (int)MainManager.Items.HoneyDanger:
                    BattleControl_Ext.Instance.ApplyTiny(entity, ref battle.enemydata[targetId], 4);
                    break;

                case (int)MainManager.Items.CookedDanger:
                    BattleControl_Ext.Instance.ApplyTiny(entity, ref battle.enemydata[targetId], 6);
                    break;
            }
        }

        public int GetItemTargetId(int jumpAntId) => battle.HPPercent(battle.enemydata[jumpAntId]) <= 0.8f ? jumpAntId : -1;
        public void AddPartnerAttacks<T>(Dictionary<T, int> attacks, int enemyId, int jumpAntId, int itemTargetId,
            T relay, T useItem)
        {
            if (jumpAntId != -1)
            {
                if (!battle.IsStopped(battle.enemydata[jumpAntId]) && BattleControl_Ext.Instance.CanRelay(battle.enemydata[enemyId].battleentity,battle))
                    attacks.Add(relay, 20);

                if (HasItem() && itemTargetId != -1)
                    attacks.Add(useItem, 10);
            }
        }
    }
}
