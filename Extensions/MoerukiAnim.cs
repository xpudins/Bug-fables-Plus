using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DynamoSporeLight;

namespace BFPlus.Extensions
{
    public class MoerukiAnim : MonoBehaviour
    {
        static readonly int ColorId = Shader.PropertyToID("_Color");
        string[] names = { "Fire1", "Fire2", "Fire3", "Fire1" };
        SpriteRenderer[] fires = new SpriteRenderer[4];
        Sprite[] lastSprite = new Sprite[4];
        MaterialPropertyBlock mpb;
        EntityControl entity;
        public Color[] colors =
        {
            new Color(0.490f, 0.027f, 0.027f, 1),
            new Color(0.765f, 0.149f, 0.031f, 1),
            new Color(0.933f, 0.510f, 0.000f, 1),
            new Color(1.000f, 0.871f, 0.216f, 1),
            new Color(0.980f, 0.941f, 0.835f, 1)
        };

        const float minValue = -5;
        const float maxValue = 10;
        float colorValue;
        public float targetValue = 0;
        public float lerpSpeed = 5;
        public float shiftRange = 2;
        public float shiftSpeed = 1.5f;
        public float finalValue;
        float driftTimer;
        public bool noShift = false;
        float lerpMultiplicator = 1;
        static Dictionary<int, Dictionary<string, Sprite>> fireSprites = new Dictionary<int, Dictionary<string, Sprite>>();
        void Start()
        {
            mpb = new MaterialPropertyBlock();
            entity = GetComponent<EntityControl>();
            for (int i = 0; i < names.Length; i++)
            {
                fires[i] = new GameObject(names[i]).AddComponent<SpriteRenderer>();
                fires[i].transform.parent = entity.spritetransform;

                if (i == names.Length - 1)
                    fires[i].transform.localPosition = new Vector3(0, 0, 0.003f);
                else
                    fires[i].transform.localPosition = new Vector3(0, 0, -0.003f + 0.001f * i);
                fires[i].transform.localEulerAngles = Vector3.zero;
                fires[i].material = MainManager.spritemat;

                if(fireSprites.Count < names.Length)
                {
                    Dictionary<string, Sprite> tempDict = new Dictionary<string, Sprite>();
                    Sprite[] sprites = MainManager_Ext.assetBundle.LoadAssetWithSubAssets<Sprite>("Moemuri_" + names[i]);
                    foreach (var s in sprites)
                    {
                        tempDict.Add(s.name, s);
                    }

                    fireSprites.Add(i, tempDict);
                }
            }
        }

        void LateUpdate()
        {
            if (entity != null)
            {
                lerpMultiplicator = 1;
                float offset = 0;
                switch ((MainManager.Animations)entity.animstate)
                {
                    case MainManager.Animations.Sleep:
                        offset = -3;
                        break;
                    case MainManager.Animations.Hurt:
                        offset = 3;
                        lerpMultiplicator *= 3;
                        break;

                    case MainManager.Animations.Block:
                        offset = 5;
                        break;
                }

                if (MainManager.instance.inbattle && MainManager.battle != null && MainManager.enemydata != null &&
                    entity.battleid < MainManager.battle.enemydata.Length && entity.battleid > 0)
                {
                    offset += GetOffsetConditions(entity.battleid);
                    offset += MainManager.battle.enemydata[entity.battleid].charge;
                }

                float value = targetValue + offset;

                if (colorValue != value)
                {
                    colorValue = Mathf.SmoothStep(colorValue, value, lerpSpeed * lerpMultiplicator * Time.deltaTime);
                    if (Mathf.Abs(colorValue - value) < 0.1f)
                    {
                        colorValue = value;
                    }
                }

                bool atTarget = Mathf.Abs(colorValue - value) < 0.1f;

                finalValue = colorValue;

                if (atTarget)
                {
                    if (!noShift)
                    {
                        driftTimer += Time.deltaTime * shiftSpeed;
                    }
                    float shiftOffset = Mathf.PingPong(driftTimer, shiftRange * 2f) - shiftRange;
                    finalValue = value + shiftOffset;
                }
                else
                {
                    driftTimer = shiftRange;
                }

                finalValue = Mathf.Clamp(finalValue, minValue, maxValue);


                if (entity.sprite != null && entity.sprite.sprite != null)
                {
                    for (int i = 0; i < lastSprite.Length; i++)
                    {
                        if (lastSprite[i] != entity.sprite.sprite)
                        {
                            if (fireSprites[i].ContainsKey(entity.sprite.sprite.name))
                                fires[i].sprite = fireSprites[i][entity.sprite.sprite.name];
                            else
                                fires[i].sprite = null;

                            lastSprite[i] = entity.sprite.sprite;
                            float colorOffset = i == lastSprite.Length - 1 ? 0 : i * 1.5f;

                            mpb.Clear();
                            fires[i].GetPropertyBlock(mpb);
                            mpb.SetColor(ColorId, GetLerpedColor(finalValue - colorOffset));
                            fires[i].SetPropertyBlock(mpb);
                        }
                    }
                }
            }
        }

        int GetOffsetConditions(int enemyId)
        {
            int offset = 0;

            if (MainManager.HasCondition(MainManager.BattleCondition.AttackUp, MainManager.battle.enemydata[enemyId]) > -1)
                offset += 5;
            if (MainManager.HasCondition(MainManager.BattleCondition.AttackDown, MainManager.battle.enemydata[enemyId]) > -1)
                offset += -5;

            return offset;
        }

        Color GetLerpedColor(float v)
        {
            float t = Mathf.InverseLerp(minValue, maxValue, v);
            float scaled = t * (colors.Length - 1);

            int index = Mathf.FloorToInt(scaled);
            int nextIndex = Mathf.Clamp(index + 1, 0, colors.Length - 1);

            float lerpT = scaled - index;

            return Color.Lerp(colors[index], colors[nextIndex], lerpT);
        }
    }
}
