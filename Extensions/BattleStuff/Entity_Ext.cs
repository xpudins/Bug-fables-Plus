using BFPlus.Extensions.BattleStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using static BattleControl;

namespace BFPlus.Extensions
{
    public class Entity_Ext : MonoBehaviour
    {
        public BattleDataExtra extraData;
        public int fireDamage = 0;
        public int sleepScheduleTurns = 1;
        public int asleepTurns = 0;
        public int tauntedBy = -1;
        public bool vitiation = false;
        public bool sleepScheduled = false;
        public int healedThisTurn = 0;
        public int id = -1;
        public int lastHp = -1;
        public int itemId = -1;
        public bool inkDebuffed = false;
        public bool permanentInkTriggered = false;
        public bool slugskinActive = false;
        public bool smearchargeActive = false;
        public bool overrideDamageAnim = false;
        public int lastTurnHp = -1;
        public SpriteRenderer item;
        Stat[] resStats = new Stat[5];
        Transform iconHolder;
        public EntityControl entity;
        public bool isPlayer = false;
        SpriteRenderer[] modelSprites = null;
        public int[] oldRes = new int[4] { 90, 90, 90, 90 };
        public GameObject slugskin = null;
        public DialogueAnim slugParent;
        Vector3 slugskinDefaultRotation = new Vector3(90, 0, 0);
        public bool inkWellActive = false;
        public bool adrenalineUsed = false;
        public bool inkblotActive = false;
        public Vector3 baseScale = Vector3.one;
        public float shadowBaseSize = 1;
        public bool scaleChanged = false;
        public bool tinyMovesAdded = false;
        public bool isPartner = false;
        public DialogueAnim inkBubble = null;
        public bool inkBubbleEnabled = false;
        public int piercedStatusRes;
        public int corkscrewRelays;

        public bool hasDizzyAnim = false;
        DialogueAnim vitiationShield = null;

        //Dizzy Stuff
        GameObject dizzyStarParent;
        SpriteRenderer[] dizzyStars;
        float dizzyStarRotation = 120f;
        private float _zVelocity;
        public float smoothTime = 0.5f;
        float dizzySpinMax = 10;
        float dizzySpinSpeed = 0.5f;
        public bool isDizzy = false;
        bool lifelustIsEquipped = false;
        public bool cantSwap = false;
        public int dizzyRecoil = 0;
        public bool? diedFromDizzy = null;
        public bool canBypassSwapRestrictions = false;
        public bool didSpinout = false;

        void Start()
        {
            entity = GetComponent<EntityControl>();
            isPlayer = CompareTag("Player");
            if (entity.model)
            {
                modelSprites = entity.model.GetComponentsInChildren<SpriteRenderer>();
            }

            if(isPlayer && id != -1 && id < MainManager.instance.playerdata.Length)
            {
                lifelustIsEquipped = MainManager.BadgeIsEquipped((int)Medal.LifeLust, MainManager.instance.playerdata[id].trueid);

                canBypassSwapRestrictions =MainManager.BadgeIsEquipped((int)Medal.Carousel, MainManager.instance.playerdata[id].trueid);
            }
            
            StartCoroutine(WaitUntilAnimSet());
        }

        IEnumerator WaitUntilAnimSet()
        {
            yield return new WaitUntil(() => entity.anim != null);
            yield return EventControl.tenthsec;
            hasDizzyAnim = entity.anim != null && 
                entity.anim.HasState(0, Animator.StringToHash(MainManager.Animations.Woobly.ToString()));

            if (entity.originalid > -1)
                baseScale = MainManager.endata[entity.originalid + 1].startscale;
            else
                baseScale = Vector3.one;

            shadowBaseSize = entity.shadowsize;

            if (!isPlayer)
                extraData = MainManager_Ext.Instance.GetEnemyExtraData(MainManager.battle.enemydata[entity.battleid].animid);
            else
                extraData = MainManager_Ext.Instance.GetPlayerExtraData(MainManager.instance.playerdata[id].trueid);
        }

        void Update()
        {
            if (isPlayer)
            {
                if (lifelustIsEquipped)
                {
                    if (lastHp < MainManager.instance.playerdata[id].hp)
                    {
                        healedThisTurn += MainManager.instance.playerdata[id].hp - lastHp;
                    }
                    lastHp = MainManager.instance.playerdata[id].hp;
                }
            }
            else
            {
                if (MainManager.instance.inbattle && entity != null && !entity.dead)
                {
                    if (Time.frameCount % 3 == 0)
                    {
                        BattleControl battle = MainManager.battle;
                        int battleId = entity.battleid;

                        if (battleId < battle.enemydata.Length)
                        {
                            if (MainManager_Ext.showResistance && !battle.hideenemyhp && (MainManager.instance.librarystuff[1, battle.enemydata[battleId].animid] || battle.scopeequipped || battle.HPBarOnOther(battle.enemydata[battleId].animid)))
                            {
                                if (CheckResIcons(battle))
                                {
                                    foreach (var stat in resStats)
                                        stat.CheckStat(battleId);
                                }
                            }

                            if (entity.animid == (int)NewAnimID.IronSuit || entity.animid == (int)NewAnimID.MechaJaw)
                            {
                                UpdateOldRes(battleId);
                            }
                        }
                    }
                }
            }

            if (MainManager.instance.inbattle && entity != null)
            {
                if (isDizzy && dizzyStarParent != null)
                {
                    SetDizzyStarPos();
                    dizzyStarParent.transform.Rotate(0f, 0f, dizzyStarRotation * Time.deltaTime);
                }
            }
        }

        void LateUpdate()
        {
            if (MainManager.instance.inbattle && entity != null)
            {
                if (!entity.dead)
                {
                    if (item != null)
                    {
                        item.enabled = !MainManager.battle.action && entity.deathcoroutine == null;
                        item.transform.position = entity.spritetransform.position + new Vector3(0.6f, 0.5f, -0.1f);
                    }

                    int battleId = entity.battleid;

                    if ((isPlayer || battleId < MainManager.battle.enemydata.Length) && (!MainManager.battle.action || MainManager.battle.currentaction == Pick.Chompy) && !MainManager.battle.cancelupdate)
                    {
                        if (isDizzy)
                        {
                            if (dizzyStarParent == null)
                            {
                                CreateDizzyStar();
                            }

                            foreach (var star in dizzyStars)
                            {
                                star.transform.rotation = Quaternion.Euler(0f, 0f, -dizzyStarParent.transform.eulerAngles.z);
                            }

                            if (CanDoDizzyAnim(battleId))
                            {
                                float targetZ = Mathf.Lerp(
                                    -dizzySpinMax,
                                    dizzySpinMax,
                                    Mathf.PingPong(Time.time * dizzySpinSpeed, 1f)
                                );

                                float smoothZ = Mathf.SmoothDampAngle(
                                    entity.spritetransform.localEulerAngles.z,
                                    targetZ,
                                    ref _zVelocity,
                                    smoothTime
                                );

                                entity.spritetransform.localEulerAngles = new Vector3(0f, 0f, smoothZ);
                            }
                        }
                    }
                }

                dizzyStarParent?.gameObject.SetActive(!MainManager.battle.action && isDizzy && !entity.dead && entity.icecube == null);

                if (inkBubble != null)
                {
                    inkBubble.shrink = !inkBubbleEnabled || entity.dead;
                    inkBubble.transform.localPosition = inkBubble.transform.localScale.magnitude > 0.15f ? extraData.inkBubbleOffset
                        : new Vector3(0f, -999f);
                }

                if (slugskin != null && slugParent != null)
                {
                    slugParent.shrink = !slugskinActive;
                    slugParent.transform.localPosition = slugParent.transform.localScale.magnitude > 0.15f ? extraData.slugskinOffset
                        : new Vector3(0f, -999f);
                    slugParent.transform.localEulerAngles = slugskinDefaultRotation;
                }

                if (vitiationShield != null)
                {
                    vitiationShield.shrink = !vitiation;
                    vitiationShield.transform.localPosition = vitiationShield.transform.localScale.magnitude > 0.15f ? extraData.vitiationOffset
                        : new Vector3(0f, -999f);
                }
            }
        }

        public bool CanDoDizzyAnim(int battleId)
        {
            return !isPlayer && !extraData.noDizzySpinAnim &&
                !MainManager.battle.enemydata[battleId].isnumb &&
                !MainManager.battle.enemydata[battleId].isasleep &&
                entity.icecube == null && !hasDizzyAnim && entity.model == null;
        }

        public void ResetDizzyAngle(bool resetAnim = false)
        {
            if (!isPlayer)
            {
                entity.spritetransform.localEulerAngles = Vector3.zero;

                if(resetAnim)
                    entity.animstate = entity.basestate;
            }
        }

        public void CreateSlugskin()
        {
            if (slugskin == null)
            {
                slugParent = new GameObject("slugParent").AddComponent<DialogueAnim>();
                slugParent.transform.parent = entity.sprite.transform;
                slugParent.shrink = true;
                slugParent.shrinkspeed = 0.075f;
                slugParent.transform.localPosition = extraData.slugskinOffset;
                slugParent.transform.localScale = Vector3.zero;
                slugParent.targetscale = extraData.slugskinScale;

                slugskin = Instantiate(MainManager_Ext.assetBundle.LoadAsset<GameObject>("SlugskinShield"));
                slugskin.transform.parent = slugParent.transform;
                slugskin.transform.localScale = Vector3.zero;
                slugskin.transform.localEulerAngles = slugskinDefaultRotation;
                slugskin.transform.localPosition = Vector3.zero;

                Renderer component = slugskin.GetComponent<Renderer>();
                component.material.color = new Color(1f, 1f, 1f, 0.55f);
                component.material.renderQueue = 2505;
            }
        }

        public void CreateInkBubble()
        {
            if (inkBubble == null)
            {
                inkBubble = Instantiate(MainManager_Ext.assetBundle.LoadAsset<GameObject>("InkBubble")).AddComponent<DialogueAnim>();
                inkBubble.transform.parent = entity.sprite.transform;
                inkBubble.transform.localScale = Vector3.zero;
                inkBubble.shrink = true;
                inkBubble.shrinkspeed = 0.075f;
                inkBubble.targetscale = extraData.inkBubbleScale;
                inkBubble.transform.localScale = Vector3.zero;
                inkBubble.transform.localPosition = extraData.inkBubbleOffset;

                var staticModelAnim = inkBubble.gameObject.AddComponent<StaticModelAnim>();
                staticModelAnim.bobangle = new Vector3(1, 2, 1);
                staticModelAnim.bobfreq = new Vector3(1, 2, 1);
                staticModelAnim.bobspeed = new Vector3(1, 2, 1);

                Renderer component = inkBubble.GetComponent<Renderer>();
                component.material.renderQueue = 2505;

                GameObject shieldTemp = Instantiate(Resources.Load("Prefabs/Objects/BubbleShield")) as GameObject;
                Material outline = shieldTemp.GetComponent<MeshRenderer>().materials[1];
                Destroy(shieldTemp);

                MeshRenderer mr = inkBubble.GetComponent<MeshRenderer>();
                mr.materials = new Material[] { mr.material, outline };
                mr.materials[1].SetColor("_OutlineColor", new Color(0.2f, 0f, 0.2f));
            }
        }

        public void CreateVitiationShield()
        {
            if (vitiationShield == null)
            {
                vitiationShield = (Instantiate(Resources.Load("Prefabs/Objects/BubbleShield")) as GameObject).AddComponent<DialogueAnim>();
                vitiationShield.transform.parent = entity.sprite.transform;
                vitiationShield.transform.localEulerAngles = new Vector3(0, 90, 0);
                vitiationShield.shrink = true;
                vitiationShield.shrinkspeed = 0.075f;
                vitiationShield.transform.localScale = Vector3.zero;
                vitiationShield.targetscale = extraData.vitationScale;
                vitiationShield.transform.localPosition = extraData.vitiationOffset;

                Renderer component = vitiationShield.GetComponent<Renderer>();
                vitiationShield.GetComponent<MeshFilter>().mesh = (MainManager_Ext.assetBundle.LoadAsset("vitiation") as GameObject).GetComponent<MeshFilter>().mesh;
                component.material.color = new Color(1f, 1f, 1f, 0.55f);
                component.material.renderQueue = 2505;

                var mats = component.materials;
                mats[0].color = new Color(0.37f, 0, 0, 0.3585f);
                mats[0].SetColor("_EmissionColor", new Color(1f, 0f, 0f, 0.5f));
                mats[1].SetColor("_OutlineColor", new Color(0.5f, 0f, 0f, 1));
            }
        }

        void SetDizzyStarPos()
        {
            dizzyStarParent.transform.localPosition = extraData.dizzyStarOffset == Vector3.zero ?
                entity.freezesize.y * Vector3.up : extraData.dizzyStarOffset;
        }

        void CreateDizzyStar()
        {
            if (dizzyStarParent == null)
            {
                dizzyStarParent = new GameObject("dizzyStars");
                dizzyStarParent.transform.parent = entity.spritetransform;
                dizzyStarParent.transform.localEulerAngles = new Vector3(110, -90, -90);
                dizzyStarParent.transform.localScale = Vector3.one * 0.5f;
                SetDizzyStarPos();

                Sprite originalSprite = MainManager.guisprites[100];
                Rect spriteRect = originalSprite.rect;
                Sprite newSprite = Sprite.Create(
                    originalSprite.texture,
                    spriteRect,
                    new Vector2(0.5f, 0.5f),
                    originalSprite.pixelsPerUnit,
                    0,
                    SpriteMeshType.Tight,
                    originalSprite.border
                ); ;

                float radius = 1f;
                dizzyStars = new SpriteRenderer[3];
                for (int i = 0; i < dizzyStars.Length; i++)
                {
                    float angle = i * Mathf.PI * 2f / dizzyStars.Length;

                    Vector3 offset = new Vector3(
                        Mathf.Cos(angle),
                        Mathf.Sin(angle),
                        0f
                    ) * radius;

                    dizzyStars[i] = MainManager.NewSpriteObject(offset, dizzyStarParent.transform, newSprite);
                    dizzyStars[i].transform.localPosition = offset;
                    dizzyStars[i].material.color = new Color(0.54f + i * 0.2f, 0.0f, 0.54f + i * 0.2f);
                    dizzyStars[i].transform.localEulerAngles = new Vector3(90, 0, 0);
                    dizzyStars[i].transform.localScale = Vector3.one * 1;
                }
            }
        }

        void UpdateOldRes(int battleId)
        {
            MainManager.BattleData enemy = MainManager.battle.enemydata[battleId];
            if (enemy.poisonres < 999)
                oldRes[0] = enemy.poisonres;
            if (enemy.freezeres < 999)
                oldRes[1] = enemy.freezeres;
            if (enemy.numbres < 999)
                oldRes[2] = enemy.numbres;
            if (enemy.sleepres < 999)
                oldRes[3] = enemy.sleepres;
        }

        public void GetOldRes(int battleId)
        {
            MainManager.battle.enemydata[battleId].poisonres = oldRes[0];
            MainManager.battle.enemydata[battleId].freezeres = oldRes[1];
            MainManager.battle.enemydata[battleId].numbres = oldRes[2];
            MainManager.battle.enemydata[battleId].sleepres = oldRes[3];
        }

        public static Entity_Ext GetEntity_Ext(EntityControl entity)
        {
            if (entity.GetComponent<Entity_Ext>() == null)
            {
                return entity.gameObject.AddComponent<Entity_Ext>();
            }
            return entity.GetComponent<Entity_Ext>();
        }

        public void CreateItem(int itemID)
        {
            item = new GameObject("item").AddComponent<SpriteRenderer>();
            item.sprite = MainManager.itemsprites[0, itemID];
            item.material = MainManager.spritemat;
            item.material.renderQueue = 50000;
            item.gameObject.layer = 14;
            item.transform.parent = transform;
            item.transform.position = entity.spritetransform.position + new Vector3(0.6f, 0.5f, -0.1f);
            itemId = itemID;
        }

        bool CheckResIcons(BattleControl battle)
        {
            bool active = false;
            if (!battle.action)
            {
                if (battle.currentaction == Pick.SelectEnemy)
                {
                    bool isSelectAll = battle.itemarea == AttackArea.AllEnemies || battle.itemarea == AttackArea.All;
                    bool optionIsInRange = battle.option >= 0 && battle.option < battle.avaliabletargets.Length;
                    bool isSelectSingle = battle.itemarea == AttackArea.SingleEnemy;
                    EntityControl selectedEntity = battle.avaliabletargets[battle.option].battleentity;

                    if ((isSelectAll || (optionIsInRange && isSelectSingle &&
                        selectedEntity != null && selectedEntity.battleid == entity.battleid)) && !entity.hpbar.gameObject.activeSelf)
                    {
                        active = true;
                    }
                }
            }
            iconHolder.gameObject.SetActive(active);
            return active;
        }

        public void CreateResIcons()
        {
            if (entity == null)
                entity = GetComponent<EntityControl>();
            var component = entity.hpbar.Find("back").GetComponent<SpriteRenderer>();
            var dropOffset = new Vector3(0.03f, -0.04f);

            iconHolder = new GameObject("iconHolder").transform;
            iconHolder.parent = entity.transform;
            iconHolder.transform.localPosition = new Vector3(0, 0);
            iconHolder.gameObject.SetActive(false);

            /*var box = MainManager.Create9Box(Vector3.zero, new Vector3(1.4f,1), 0, 0, Color.white, false);
            box.parent = iconHolder;
            box.transform.localPosition = new Vector3(-0.25f, -0.29f);*/

            CreateIcon(iconHolder, new Vector3(-0.65f, -0.1f), new Vector3(0.35f, 0.35f, 1f), MainManager.guisprites[44],
                Vector2.one, "poisonIcon", component, dropOffset, 0, new Color(0.92f, 0.82f, 0.92f));

            CreateIcon(iconHolder, new Vector3(-0.25f, -0.1f), new Vector3(0.35f, 0.35f, 1f), MainManager.guisprites[43],
                Vector2.one, "freezeIcon", component, dropOffset, 1, Color.white);

            CreateIcon(iconHolder, new Vector3(-0.65f, -0.5f), new Vector3(0.3f, 0.3f, 1f), MainManager.guisprites[45],
                new Vector2(1.2f, 1.2f), "numbIcon", component, dropOffset, 2, new Color(0.75f, 0.39f, 0.05f));

            CreateIcon(iconHolder, new Vector3(-0.25f, -0.5f), new Vector3(0.28f, 0.28f, 1f), MainManager.guisprites[46],
                new Vector2(1.2f, 1.2f), "sleepIcon", component, dropOffset, 3, new Color(0.68f, 0.85f, 0.90f));

            CreateIcon(iconHolder, new Vector3(0.15f, -0.1f), new Vector3(0.38f, 0.38f, 1f), MainManager.guisprites[(int)NewGui.Dizzy],
                new Vector2(1f, 1f), "dizzyIcon", component, dropOffset, 4, Color.white);//new Color(0.90f, 0.63f, 0.58f));

            iconHolder.transform.localScale = Vector3.one * 1.2f;
        }

        void CreateIcon(Transform iconParent, Vector3 pos, Vector3 size, Sprite sprite, Vector3 fontSize, string name, SpriteRenderer component, Vector3 dropOffset, int index, Color color)
        {
            var icon = MainManager.NewUIObject(name, iconParent, pos, size, sprite, 10);
            DynamicFont stat = DynamicFont.SetUp(string.Empty, false, true, 5f, 2, component.sortingOrder + 20, fontSize, icon.transform, Vector3.zero, color);
            stat.dropshadow = true;
            stat.dropoffset = dropOffset;
            string[] resNames = new string[] { "poisonres", "freezeres", "numbres", "sleepres" };
            resStats[index] = new Stat(stat, index < 4 ? resNames[index] : null, this);
        }

        public void CheckInkDebuff(ref MainManager.BattleData data)
        {
            int addedValue = inkDebuffed ? 25 : -25;

            if (data.poisonres < 110)
            {
                data.poisonres = Mathf.Clamp(data.poisonres + addedValue, -999, 999);
            }

            if (data.freezeres < 110)
            {
                data.freezeres = Mathf.Clamp(data.freezeres + addedValue, -999, 999);
            }

            if (data.numbres < 110)
            {
                data.numbres = Mathf.Clamp(data.numbres + addedValue, -999, 999);
            }

            if (data.sleepres < 110)
            {
                data.sleepres = Mathf.Clamp(data.sleepres + addedValue, -999, 999);
            }

            if (extraData.DizzyRes < 110)
            {
                extraData.DizzyRes = Mathf.Clamp(extraData.DizzyRes + addedValue, -999, 999);
            }

            for (int i = 0; i < oldRes.Length; i++)
            {
                if (oldRes[i] < 110)
                {
                    oldRes[i] = Mathf.Clamp(oldRes[i] + addedValue, -999, 999);
                }
            }

            inkDebuffed = !inkDebuffed;
        }

        class Stat
        {
            FieldInfo statRef;
            DynamicFont dynamicFont;
            int lastRes = -1;
            Entity_Ext parent;
            public Stat(DynamicFont stat, string fieldName, Entity_Ext parent)
            {
                this.parent = parent;
                dynamicFont = stat;
                if (fieldName != null)
                    statRef = AccessTools.Field(typeof(MainManager.BattleData), fieldName);
            }


            public void CheckStat(int battleId)
            {
                if (battleId >= 0 && battleId < MainManager.battle.enemydata.Length)
                {
                    int currentRes;

                    if (statRef != null)
                        currentRes = (int)statRef.GetValue(MainManager.battle.enemydata[battleId]);
                    else
                        currentRes = parent.extraData.DizzyRes;

                    if (currentRes != lastRes && dynamicFont.letters != null)
                    {
                        lastRes = currentRes;
                        dynamicFont.text = Mathf.Clamp(currentRes, 0, 999).ToString();
                        ChangeAlignement();
                    }
                }
            }

            void ChangeAlignement()
            {
                foreach (var letter in dynamicFont.letters)
                {
                    if (letter != null)
                    {
                        letter.anchor = TextAnchor.MiddleCenter;
                        letter.alignment = TextAlignment.Center;
                    }
                }
            }
        }

        public void UpdateModelSprite()
        {
            if (modelSprites != null && entity != null && entity.sprite != null)
            {
                Color spriteColor = entity.sprite.material.color;

                foreach (var sprite in modelSprites)
                {
                    sprite.material.color = spriteColor;
                }
            }
        }

        public void ResetCarousel()
        {
            cantSwap = false;
            canBypassSwapRestrictions = MainManager.BadgeIsEquipped((int)Medal.Carousel, MainManager.instance.playerdata[id].trueid);
        }
    }
}
