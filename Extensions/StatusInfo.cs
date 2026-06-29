using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BFPlus.Extensions
{
    public class StatusInfo
    {
        public DialogueAnim backgroundBox;
        public StatusHolder[] holders = new StatusHolder[7];
        public Transform cursor;
        SpriteRenderer helpBar;
        int option = 0;
        float cursorSpeed = 0.1f;
        Vector3 cursorOffset = new Vector3(-0.5f, 0, -0.1f);
        public StatusInfo()
        {
            backgroundBox = MainManager.Create9Box(new Vector3(0f, -10f, 9f), new Vector2(15f, 9f), 2, 900, Color.white, true).GetComponent<DialogueAnim>();
            backgroundBox.transform.parent = MainManager.GUICamera.transform;
            backgroundBox.speed = 0.1f;
            backgroundBox.targetpos = new Vector3(0, -10, 9);

            cursor = new GameObject("cursor").transform;
            cursor.gameObject.AddComponent<SpriteRenderer>().sprite = MainManager.cursorsprite[0];
            cursor.gameObject.AddComponent<SpriteBounce>().MessageBounce();
            cursor.transform.parent = backgroundBox.transform;
            cursor.gameObject.layer = LayerMask.NameToLayer("UI");
            cursor.GetComponent<SpriteRenderer>().sortingOrder = 900;
            cursor.transform.eulerAngles = new Vector3(0f, 0f, 0f);

            var nameBar = MainManager.NewUIObject("NameBar", backgroundBox.transform, new Vector3(0, 4f, -0.1f)).AddComponent<SpriteRenderer>();
            nameBar.sprite = MainManager.guisprites[0];
            nameBar.color = new Color(1f, 1f, 1f, 0.5f);
            nameBar.transform.localScale = new Vector3(1f, 1f, 1f);
            nameBar.sortingOrder = 900;

            MainManager.battle.StartCoroutine(WaitUntilExpCounterCreation());

            MainManager.battle.StartCoroutine(MainManager.SetText("|sort,900|Logbook", 0, new float?((float)99999), false, false, new Vector3(-1.3f, -0.2f, -0.4f), Vector3.zero, Vector2.one, nameBar.transform, null));

            //95 leif
            //102 vi
            //183 kabbu
            Vector3 lastPos = Vector3.zero;

            for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
            {
                int portraitId = 150;
                Color color = new Color(1, 1, 0, 0.5f);

                if (MainManager.instance.playerdata[i].battleentity.animid == 1)
                {
                    portraitId = 183;
                    color = new Color(0, 1, 0, 0.5f);
                }
                else if (MainManager.instance.playerdata[i].battleentity.animid == 2)
                {
                    portraitId = 151;
                    color = new Color(0f, 0.6877f, 0.902f, 0.5f);
                }

                lastPos = new Vector3(-6, 3f - i, -0.1f);
                holders[i] = StatusHolder.SetupHolder(MainManager.librarysprites[portraitId], backgroundBox.transform, lastPos, MainManager.instance.playerdata[i].battleentity);
                var strip = MainManager.Create9Box(Vector3.zero, new Vector2(14f, 1.2f), 3, 900, color, true);
                strip.transform.parent = backgroundBox.transform;
                strip.transform.localPosition = new Vector3(0, 3f - i, -0.05f);
            }

            for (int i = 0; i < MainManager.battle.enemydata.Length; i++)
            {
                lastPos = lastPos - new Vector3(0, 1, 0);
                holders[i + 3] = StatusHolder.SetupHolder(MainManager.librarysprites[MainManager.GetEnemyPortrait(MainManager.battle.enemydata[i].animid)], backgroundBox.transform, lastPos, MainManager.battle.enemydata[i].battleentity);
            }
        }

        IEnumerator WaitUntilExpCounterCreation()
        {
            yield return new WaitUntil(() => MainManager.battle.hexpcounter != null);

            helpBar = MainManager.NewUIObject("NameBar", MainManager.battle.hexpcounter.transform, new Vector3(-15f, 0f, 0f)).AddComponent<SpriteRenderer>();
            helpBar.sprite = MainManager.guisprites[0];
            helpBar.color = new Color(1f, 1f, 1f, 0.5f);
            helpBar.transform.localScale = new Vector3(1f, 2.3f, 1f);
            helpBar.sortingOrder = 0;

            MainManager.battle.StartCoroutine(MainManager.SetText("|size,1.5,0.9|Info", 0, new float?((float)99999), false, false, new Vector3(0.6f, -0.2f, -0.1f), Vector3.zero, new Vector3(1, 1, 1), helpBar.transform, null));


            var buttonSprite = new GameObject("button").AddComponent<ButtonSprite>().SetUp(9, -1, null, new Vector3(-1.3f, 0, -0.1f), new Vector3(0.9f, 0.4f, 0.9f), 0, helpBar.transform);
        }

        public void DestroyHelpBox()
        {
            UnityEngine.Object.Destroy(helpBar?.gameObject);
        }

        public void RefreshEnemyIcons()
        {
            for (int i = 3; i < holders.Length; i++)
            {
                if (holders[i] != null)
                    UnityEngine.Object.Destroy(holders[i]);
            }

            Vector3 lastPos = Vector3.zero;
            for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
            {
                if (holders[i] != null)
                    lastPos = holders[i].icon.transform.localPosition;
            }

            for (int i = 0; i < MainManager.battle.enemydata.Length; i++)
            {
                lastPos = lastPos - new Vector3(0, 1, 0);
                holders[i + 3] = StatusHolder.SetupHolder(MainManager.librarysprites[MainManager.GetEnemyPortrait(MainManager.battle.enemydata[i].animid)], backgroundBox.transform, lastPos, MainManager.battle.enemydata[i].battleentity);
                holders[i + 3].RefreshStatusIcons();
            }

            cursor.transform.localPosition = holders[0].icon.transform.localPosition + cursorOffset;
        }

        public void GetInput()
        {
            int maxOptions = GetMaxOption();

            if (MainManager.GetKey(0, false) || MainManager.KeyHold(MainManager.Directions.Up))
            {
                MainManager.PlaySound("Scroll", 10);
                option--;

                if (option < 0)
                {
                    option = maxOptions-1;
                    while (holders[option] == null)
                    {
                        option--;
                    }
                }

                for (; option > 0; option--)
                {
                    if (holders[option] != null)
                        break;
                }
            }
            else if (MainManager.GetKey(1, false) || MainManager.KeyHold(MainManager.Directions.Down))
            {
                MainManager.PlaySound("Scroll", 10);
                option++;

                for (; option < holders.Length; option++)
                {
                    if (holders[option] != null)
                        break;
                }

                if (option >= maxOptions)
                {
                    option = 0;
                }
            }

            int maxRow = holders[option].GetConditionsAmount();
            int rowOption = holders[option].options;
            if (MainManager.GetKey(2, false) || MainManager.KeyHold(MainManager.Directions.Left))
            {
                if (holders[option].options > 0)
                {
                    MainManager.PlaySound("Scroll", 10);
                    holders[option].options--;
                }
            }
            else if (MainManager.GetKey(3, false) || MainManager.KeyHold(MainManager.Directions.Right))
            {
                if (maxRow > StatusHolder.maxIcons && holders[option].options < maxRow - StatusHolder.maxIcons)
                {
                    MainManager.PlaySound("Scroll", 10);
                    holders[option].options++;
                }
            }
            else
            {
                MainManager.ResetKeyHold();
            }

            if (rowOption != holders[option].options)
            {
                holders[option].ScrollRow(holders[option].options - rowOption);
            }

            cursor.transform.localPosition = Vector3.Lerp(cursor.transform.localPosition, holders[option].icon.transform.localPosition + cursorOffset, MainManager.TieFramerate(cursorSpeed));
        }

        int GetMaxOption()
        {
            return MainManager.instance.playerdata.Length + MainManager.battle.enemydata.Length;
        }

        public IEnumerator ShowStatusInfo()
        {
            RefreshEnemyIcons();
            option = 0;
            MainManager.battle.action = true;
            BattleControl_Ext.Instance.statusInfo.backgroundBox.targetpos = BattleControl_Ext.Instance.statusInfo.backgroundBox.targetpos + Vector3.up * 10;
            yield return EventControl.tenthsec;

            while (!MainManager.GetKey(9, false) && !MainManager.GetKey(5, false))
            {
                BattleControl_Ext.Instance.statusInfo.GetInput();
                yield return null;
            }

            BattleControl_Ext.Instance.statusInfo.backgroundBox.targetpos = BattleControl_Ext.Instance.statusInfo.backgroundBox.targetpos + Vector3.up * -10;
            MainManager.battle.action = false;
        }

        public class StatusHolder : MonoBehaviour
        {
            Vector3 iconScale = Vector3.one * 0.7f;
            public SpriteRenderer icon;
            List<Transform> conditions = new List<Transform>();
            public EntityControl parentEntity;
            public Transform row;
            public GameObject[] moreIcon = new GameObject[2];
            Vector3 rowOffset = new Vector3(1, 0, 0);
            public const int maxIcons = 10;
            public int options = 0;

            public static StatusHolder SetupHolder(Sprite iconSprite, Transform parent, Vector3 position, EntityControl parentEntity)
            {
                StatusHolder holder = parentEntity.gameObject.AddComponent<StatusHolder>();
                holder.icon = MainManager.NewUIObject("entityIcon", parent, position, holder.iconScale, iconSprite, 900).GetComponent<SpriteRenderer>();

                holder.parentEntity = parentEntity;
                holder.row = new GameObject("row").transform;
                holder.row.transform.parent = holder.icon.transform;
                holder.row.transform.localPosition = holder.rowOffset;
                holder.row.transform.localScale = Vector3.one;

                for (int i = 0; i < holder.moreIcon.Length; i++)
                {
                    holder.moreIcon[i] = MainManager.NewUIObject("moreIcon" + i, holder.icon.transform, new Vector3(i == 0 ? 1.25f : 17.5f, 0, -0.1f), Vector3.one, MainManager.guisprites[1], 900);
                    holder.moreIcon[i].gameObject.SetActive(false);
                    holder.moreIcon[i].transform.localEulerAngles = new Vector3(0, 0, i == 0 ? -90 : 90);
                }
                return holder;
            }

            public void RefreshStatusIcons()
            {
                for (int i = 0; i < conditions.Count; i++)
                {
                    Destroy(conditions[i].gameObject);
                }
                conditions.Clear();

                if (parentEntity.statusicons != null)
                {
                    for (int i = 0; i < parentEntity.statusicons.Length; i++)
                    {
                        if (parentEntity.statusicons[i] != null)
                        {
                            conditions.Add(Instantiate(parentEntity.statusicons[i]));
                            int lastAdded = conditions.Count - 1;

                            conditions[lastAdded].parent = row.transform;
                            ChangeLayer(conditions[lastAdded]);

                            if (conditions[lastAdded].childCount > 0)
                            {
                                var child = conditions[lastAdded].GetChild(0);
                                child.localPosition = new Vector3(child.localPosition.x, child.localPosition.y, -0.1f);
                            }

                            conditions[lastAdded].localPosition = new Vector3(1.5f + 1.5f * lastAdded, 0, 0);
                            conditions[lastAdded].localScale = Vector3.one;
                            conditions[lastAdded].gameObject.SetActive(lastAdded < maxIcons);
                        }
                    }

                    moreIcon[1].gameObject.SetActive(conditions.Count > maxIcons);
                    row.transform.localPosition = rowOffset;
                    options = 0;
                }
            }

            public void ScrollRow(int index)
            {
                row.transform.localPosition += new Vector3(index * -1.5f, 0);

                for (int i = 0; i < conditions.Count; i++)
                {
                    conditions[i].gameObject.SetActive(i >= options && i < maxIcons + options);
                }

                moreIcon[0].gameObject.SetActive(options > 0);
                moreIcon[1].gameObject.SetActive(options < GetConditionsAmount() - maxIcons);
            }

            public int GetConditionsAmount()
            {
                return conditions.Count;
            }

            void ChangeLayer(Transform transform)
            {
                transform.gameObject.layer = LayerMask.NameToLayer("UI");
                var renderer = transform.gameObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sortingOrder = 900;
                }

                foreach (Transform t in transform)
                {
                    ChangeLayer(t);
                }
                ;
            }

            void OnDestroy()
            {
                if (icon != null)
                    Destroy(icon.gameObject);
            }
        }
    }
}
