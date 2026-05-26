using BFPlus.Extensions.EnemyAI;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace BFPlus.Extensions
{
    public class MechaJawComp : MonoBehaviour
    {
        EntityControl entity;
        GameObject fire;
        float spinSpeed = 5;
        Transform turnKey;
        int fuseTimer = 10;
        public bool turntUp = false;
        const int FIRE_APPEAR_TURN = 5;
        bool flipDebuff = false;
        const int FLIP_SLEEP_RES = 75;
        const int FLIP_NUMB_RES = 75;
        const int FLIP_FREEZE_RES = 75;
        const int FLIP_DIZZY_RES = 75;
        void Start()
        {
            entity = GetComponent<EntityControl>();
            turnKey = entity.model.Find("Body/LeafBase/TurnKey");
            fire = turnKey.parent.Find("Flame").gameObject;
            fire.gameObject.SetActive(false);
        }

        void Update()
        {
            if (fuseTimer > 0)
                turnKey.Rotate(0f, spinSpeed * MainManager.TieFramerate(1f), 0);
        }

        public IEnumerator DecreaseFuseTimer(MainManager.BattleData target)
        {
            int decrease = 1;
            if (MainManager.HasCondition(MainManager.BattleCondition.Fire, target) != -1)
            {
                decrease++;
            }

            decrease += target.charge;

            SpriteRenderer holder = MainManager.NewSpriteObject(new Vector3(-0.7f, 2.5f, -0.1f), transform, MainManager.guisprites[47]);
            holder.transform.localScale = Vector3.one * 1.2f;
            holder.transform.parent = MainManager.battle.battlemap.transform;
            holder.material = MainManager.spritedefaultunity;

            int spriteIndex = fuseTimer == 10 ? (int)NewGui.SpycardsTen : 48 + fuseTimer;

            SpriteRenderer countDown = MainManager.NewSpriteObject(new Vector3(0, 0.14f, -0.1f), holder.transform, MainManager.guisprites[spriteIndex]);
            countDown.material = MainManager.spritedefaultunity;

            Color startColor = Color.green;
            Color targetColor = Color.red;
            countDown.material.color = GetCountdownColor();
            yield return EventControl.quartersec;

            entity.animstate = (int)MainManager.Animations.Jump;
            for (int i = 0; i < decrease; i++)
            {
                entity.Jump();
                fuseTimer--;
                if (fuseTimer <= FIRE_APPEAR_TURN)
                {
                    turntUp = true;
                    fire.gameObject.SetActive(true);
                }
                MainManager.PlaySound("WoodHit", 0.8f, 1);
                countDown.sprite = MainManager.guisprites[48 + fuseTimer];
                countDown.material.color = GetCountdownColor();
                yield return EventControl.quartersec;
                MainManager.PlaySound("WoodHit", 1.2f, 1);
                yield return new WaitUntil(() => entity.onground);

                if (fuseTimer <= 0)
                    break;
            }
            Destroy(holder.gameObject);

            //if fuse <= 0 kaboom
            if (fuseTimer <= 0)
            {
                MainManager.PlaySound("DLBetaSpike");
                spinSpeed *= 2;
                fire.GetComponent<Animator>().Play("FlameT");
                Vector3 targetPos = turnKey.position + new Vector3(10, 0, 0);
                turnKey.localEulerAngles = Vector3.zero;
                entity.StartCoroutine(MainManager.ArcMovement(turnKey.gameObject, turnKey.position, targetPos, new Vector3(0, 0, 20), 5, 50, true));
                MainManager.BattleCondition status = MainManager.battle.sdata.enemies.Contains((int)NewEnemies.Patton) ?
                    MainManager.BattleCondition.Numb :
                    MainManager.BattleCondition.Fire;
                yield return MainManager.battle.StartCoroutine(MechaJawAI.DoKaboom(entity, entity.battleid, status));
            }
        }

        Color GetCountdownColor() => Color.Lerp(Color.green, Color.red, (float)(10 - fuseTimer) / 10);

        public void CheckFlippedRes(ref MainManager.BattleData data)
        {
            if (!flipDebuff)
            {
                Entity_Ext extBomi = Entity_Ext.GetEntity_Ext(data.battleentity);
                extBomi.extraData.DizzyRes = Mathf.Clamp(extBomi.extraData.DizzyRes - FLIP_DIZZY_RES, 0, 999);
                data.sleepres = Mathf.Clamp(data.sleepres - FLIP_SLEEP_RES, 0, 999);
                data.freezeres = Mathf.Clamp(data.freezeres - FLIP_FREEZE_RES, 0, 999);
                data.numbres = Mathf.Clamp(data.numbres - FLIP_NUMB_RES, 0, 999);
                flipDebuff = true;
            }
        }

        public void ResetFlippedRes(ref MainManager.BattleData data)
        {
            if (flipDebuff)
            {
                Entity_Ext extBomi = Entity_Ext.GetEntity_Ext(data.battleentity);
                extBomi.extraData.DizzyRes = Mathf.Clamp(extBomi.extraData.DizzyRes + FLIP_DIZZY_RES, 0, 999);
                data.sleepres = Mathf.Clamp(data.sleepres + FLIP_SLEEP_RES, 0, 999);
                data.freezeres = Mathf.Clamp(data.freezeres + FLIP_FREEZE_RES, 0, 999);
                data.numbres = Mathf.Clamp(data.numbres + FLIP_NUMB_RES, 0, 999);
                flipDebuff = false;
            }
        }
    }
}
