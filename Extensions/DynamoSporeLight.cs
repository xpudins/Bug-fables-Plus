using BFPlus.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DynamoSporeLight : MonoBehaviour
{
    public enum LightState
    {
        Off,
        Small,
        Mid,
        High,
        Full
    }

    public enum Mode
    {
        Stay,
        ChargeUp,
        ChargeDown,
        Charging,
        Blinking,
        Decharging
    }
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    public LightState state = LightState.Off;
    public LightState lastState = LightState.Off;
    public Mode mode = Mode.Charging;
    public bool overrideLight = false;
    float chargeCooldown = 0;
    public float chargeFrame = 30f;
    Sprite lastSprite;
    SpriteRenderer lightSprite;
    EntityControl entity;
    int oldAnimState = 0;
    Vector3 baseLightPosition = new Vector3(0, 0, -0.0001f);
    static Dictionary<LightState, Dictionary<string, Sprite>> lightSprites = new Dictionary<LightState, Dictionary<string, Sprite>>();
    Color lastLightColor;

    void Start()
    {
        entity = GetComponent<EntityControl>();
        lightSprite = new GameObject("LightLevel").AddComponent<SpriteRenderer>();
        lightSprite.transform.parent = entity.spritetransform;
        //lightSprite.sortingOrder = entity.sprite.sortingOrder + 1;
        
        if(lightSprites.Count == 0)
        {
            Sprite[] lights = MainManager_Ext.assetBundle.LoadAssetWithSubAssets<Sprite>("DynamoSporeLight");

            LightState[] states = Enum.GetValues(typeof(LightState)).Cast<LightState>().ToArray();
            foreach (LightState state in states)
            {
                Dictionary<string, Sprite> tempDict = new Dictionary<string, Sprite>();

                Sprite[] sprites = lights.Where(a => a.name.Contains(state.ToString())).ToArray();
                string stateString = state.ToString();
                foreach (var s in sprites)
                {
                    s.name = s.name.Replace("dynamoLight" + stateString, "zombees");
                    tempDict.Add(s.name, s);
                }

                lightSprites.Add(state, tempDict);
            }
        }

        lightSprite.sprite = lightSprites[LightState.Off]["zombees_39"];
        lightSprite.sortingOrder = -1;
        lightSprite.transform.localPosition = baseLightPosition;
        lightSprite.transform.localScale = Vector3.one;
    }

    void LateUpdate()
    {
        Color targetColor = Color.white;

        if (entity.icecube != null)
        {
            mode = Mode.Stay;
        }
        else
        {
            if (oldAnimState != entity.animstate)
            {
                oldAnimState = entity.animstate;
                GetChargeFrame();
            }

            if (HasMaxCharge())
            {
                if (entity.sprite != null && entity.sprite.sprite != null)
                {
                    targetColor = MainManager.RainbowColor(1, 5.9f * 1, 0.5f, 1f, 1f);

                    if (!overrideLight && (entity.animstate == (int)MainManager.Animations.Idle || entity.animstate == (int)MainManager.Animations.Sleep))
                    {
                        mode = Mode.Stay;
                        state = LightState.Full;
                    }
                }
            }
            else
            {
                targetColor = Color.white;
            }

            if (mode == Mode.Charging)
            {
                if (chargeCooldown <= 0)
                {
                    chargeCooldown = chargeFrame;
                    state++;

                    if ((int)state >= 4)
                    {
                        state = LightState.Full;
                        mode = Mode.Decharging;
                    }
                }
            }
            else if (mode == Mode.ChargeUp || mode == Mode.ChargeDown)
            {
                int dif = mode == Mode.ChargeUp ? 1 : -1;
                int max = mode == Mode.ChargeUp ? (int)LightState.Full : (int)LightState.Off;
                CheckCharge(mode == Mode.ChargeUp ? (int)state + dif <= max : (int)state + dif >= max, dif);
            }
            else if (mode == Mode.Blinking)
            {
                if (chargeCooldown <= 0)
                {
                    chargeCooldown = chargeFrame;
                    lightSprite.enabled = !lightSprite.enabled;
                }
            }
            else if (mode == Mode.Decharging)
            {
                if (chargeCooldown <= 0)
                {
                    chargeCooldown = chargeFrame;
                    state--;

                    if ((int)state <= 0)
                    {
                        state = LightState.Off;
                        mode = Mode.Charging;
                    }
                }
            }
            chargeCooldown -= 1f;
        }

        if (entity.sprite != null && entity.sprite.sprite != null && (lastSprite != entity.sprite.sprite || lastState != state))
        {
            lastSprite = entity.sprite.sprite;
            lightSprite.sprite = lightSprites[state][lastSprite.name];
        }

        if(targetColor != lastLightColor)
        {
            lastLightColor = targetColor;
            lightSprite.color = targetColor;
        }

        if (lastState != state)
            lastState = state;
    }

    void GetChargeFrame()
    {
        if (!overrideLight)
        {
            switch (entity.animstate)
            {
                case (int)MainManager.Animations.Idle:
                    mode = mode == Mode.Decharging ? Mode.Decharging : Mode.Charging;
                    chargeFrame = 30f;
                    break;
                case (int)MainManager.Animations.Walk:
                    chargeFrame = 22.5f;
                    break;
                case (int)MainManager.Animations.Hurt:
                    chargeFrame = 5f;
                    break;
                case (int)MainManager.Animations.Sleep:
                    chargeFrame = 60f;
                    break;
            }
            chargeCooldown = chargeFrame;
        }
    }

    void CheckCharge(bool condition, int n)
    {
        if (chargeCooldown <= 0)
        {
            chargeCooldown = chargeFrame;
            if (condition)
                state += n;
            else
                mode = Mode.Stay;
        }
    }

    bool HasMaxCharge()
    {
        if (MainManager.instance.inbattle && MainManager.battle != null && MainManager.battle.enemydata != null && entity.battleid != -1 && entity.battleid < MainManager.battle.enemydata.Length)
        {
            return MainManager.battle.enemydata[entity.battleid].charge == 3;
        }
        return false;
    }

    public IEnumerator DoChargeUp(float frame)
    {
        chargeFrame = frame;
        mode = Mode.ChargeUp;
        state = LightState.Off;
        yield return new WaitUntil(() => state == LightState.Full);
        mode = Mode.Stay;
    }

    public void ResetData()
    {
        chargeFrame = 30f;
        overrideLight = false;
    }
}
