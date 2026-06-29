using BFPlus.Extensions.MiscStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using static BFPlus.Extensions.MiscStuff.MaterialCache;

namespace BFPlus.Patches
{
    [HarmonyPatch(typeof(Fader), "Start")]
    public class PatchFaderStart
    {
        static void Postfix(Fader __instance)
        {
            MaterialCache.SetupCache(__instance.renders);
        }
    }

    [HarmonyPatch(typeof(Fader), "LateUpdate")]
    public class PatchFaderLateUpdate
    {
        static bool Prefix(Fader __instance)
        {
            NewLateUpdate(__instance);
            return false;
        }

        static void NewLateUpdate(Fader __instance)
        {
            if (__instance.initialcolors == null || __instance.initialcolors.Count == 0)
            {
                __instance.initialcolors = new List<Color[]>();
                for (int i = 0; i < __instance.renders.Length; i++)
                {
                    MatCache cache = MaterialCache.Get(__instance.renders[i]);

                    List<Color> list = new List<Color>();
                    for (int j = 0; j < cache.mats.Length; j++)
                    {
                        if (cache.mats[j].HasProperty("_Color"))
                        {
                            list.Add(cache.mats[j].color);
                        }
                    }
                    __instance.initialcolors.Add(list.ToArray());
                }
            }
            if ((!__instance.forcestayonpause || MainManager.FreePlayer(false)) && Time.frameCount % 3 == 0)
            {
                if (!Fader.grasslands && !__instance.alwaysfade)
                {
                    for (int k = 0; k < __instance.renders.Length; k++)
                    {
                        if (!__instance.renders[k].gameObject.CompareTag("NoFader"))
                        {
                            if (!__instance.CheckY())
                            {
                                __instance.renders[k].shadowCastingMode = ShadowCastingMode.On;
                            }
                            else if (__instance.childtied && k > 0)
                            {
                                __instance.renders[k].shadowCastingMode = __instance.renders[0].shadowCastingMode;
                            }
                            else
                            {
                                MainManager.DisableRender(__instance.renders[k], __instance.zdistance, __instance.pivotoffset);
                            }
                        }
                    }
                    return;
                }
                Vector3 vector = MainManager.MainCamera.WorldToViewportPoint(__instance.transform.position + __instance.pivotoffset);
                for (int i = 0; i < __instance.renders.Length; i++)
                {
                    MatCache cache = MaterialCache.Get(__instance.renders[i]);

                    if (!__instance.renders[i].gameObject.CompareTag("NoFader"))
                    {
                        for (int j = 0; j < cache.mats.Length; j++)
                        {
                            if (__instance.fadedistance > -1f && cache.mat.shader != MainManager.fakelight && cache.mat.shader != MainManager.emptymat.shader)
                            {
                                if (cache.mats[j].shader == MainManager.outlinemain.shader)
                                {
                                    cache.mats[j].renderQueue = 3000;
                                }
                                else
                                {
                                    __instance.UpdateShader(i, j);
                                }
                            }
                        }
                        if (__instance.insideid > -2 && __instance.insideid != MainManager.instance.insideid)
                        {
                            __instance.renders[i].enabled = false;
                        }
                        else if (__instance.childtied && i > 0)
                        {
                            __instance.renders[i].shadowCastingMode = __instance.renders[0].shadowCastingMode;
                            __instance.renders[i].enabled = __instance.renders[0].enabled;
                        }
                        else if (MainManager.player != null && !__instance.CheckY())
                        {
                            __instance.renders[i].shadowCastingMode = ShadowCastingMode.On;
                        }
                        else if (__instance.zdistance > -1f)
                        {
                            if (__instance.checkx < 0.1f)
                            {
                                MainManager.DisableRender(__instance.renders[i], __instance.zdistance, __instance.pivotoffset);
                            }
                            else if (MainManager.GetDistance(MainManager.MainCamera.transform.position.x, __instance.transform.position.x + __instance.pivotoffset.x) > __instance.checkx)
                            {
                                __instance.renders[i].enabled = true;
                            }
                            else
                            {
                                MainManager.DisableRender(__instance.renders[i], __instance.zdistance, __instance.pivotoffset);
                            }
                        }
                    }
                }
                if (__instance.ignoreY)
                {
                    __instance.inrange = MainManager.player != null && MainManager.GetSqrDistance(new Vector3(__instance.transform.position.x, 0f, __instance.transform.position.z) + new Vector3(__instance.pivotoffset.x, 0f, __instance.pivotoffset.z), new Vector3(MainManager.MainCamera.transform.position.x, 0f, MainManager.MainCamera.transform.position.z), true) < __instance.fadedistance;
                    return;
                }
                __instance.inrange = MainManager.player != null && MainManager.GetSqrDistance(__instance.transform.position + __instance.pivotoffset, MainManager.MainCamera.transform.position, true) < __instance.fadedistance && MainManager.MainCamera.WorldToViewportPoint(MainManager.player.transform.position).z > vector.z && __instance.CheckY();
            }
        }
    }

    [HarmonyPatch(typeof(Fader), "UpdateShader")]
    public class PatchFaderUpdateShader
    {
        static bool Prefix(Fader __instance, int i, int j)
        {
            NewUpdateShader(__instance, i, j);
            return false;     
        }

        static void NewUpdateShader(Fader __instance, int i, int j)
        {
            if (__instance.hascolor[i][j])
            {
                MatCache cache = MaterialCache.Get(__instance.renders[i]);

                if (cache.mats[j].shader == MainManager.Main3D.shader || cache.mats[j].shader == MainManager.Fade3D.shader)
                {
                    if (cache.mats[j].color.a >= 0.9f)
                    {
                        cache.mats[j].shader = MainManager.Main3D.shader;
                    }
                    else
                    {
                        cache.mats[j].shader = MainManager.Fade3D.shader;
                    }
                }
                else if (cache.mats[j].shader == MainManager.fadePlane.shader || cache.mats[j].shader == MainManager.mainPlane.shader)
                {
                    if (!__instance.faderender.Contains(__instance.renders[i].transform) && !__instance.dontclone)
                    {
                        __instance.faderender.Add(__instance.renders[i].transform);
                        Renderer component = UnityEngine.Object.Instantiate<GameObject>(__instance.renders[i].gameObject).GetComponent<Renderer>();
                        __instance.faderender.Add(component.transform);
                        component.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                        component.receiveShadows = false;
                        component.transform.position = __instance.renders[i].transform.position;
                        component.transform.eulerAngles = __instance.renders[i].transform.eulerAngles;
                        component.material.color = Color.white;
                        component.gameObject.isStatic = true;
                        component.gameObject.tag = "NoFader";
                        component.transform.parent = __instance.renders[i].transform;
                        component.transform.localScale = Vector3.one;
                        if (component.GetComponent<Fader>() != null)
                        {
                            UnityEngine.Object.Destroy(component.GetComponent<Fader>());
                        }
                    }
                    if (cache.mats[j].color.a >= 0.9f)
                    {
                        cache.mats[j].shader = MainManager.mainPlane.shader;
                    }
                    else
                    {
                        cache.mats[j].shader = MainManager.fadePlane.shader;
                    }
                }
                if (__instance.initialcolors != null && __instance.initialcolors.Count > 0 && __instance.initialcolors[i].Length != 0 && j <= __instance.initialcolors[i].Length - 1)
                {
                    Color color = __instance.initialcolors[i][j];
                    if (cache.mats[j].shader == MainManager.Fade3D.shader)
                    {
                        color = MainManager.map.skycolor;
                    }
                    color = new Color(color.r, color.g, color.b, cache.mats[j].color.a);
                    if (__instance.inrange)
                    {
                        cache.mats[j].color = Color.Lerp(color, new Color(color.r, color.g, color.b, 0.3f), MainManager.TieFramerate(__instance.fadespeed));
                        return;
                    }
                    cache.mats[j].color = Color.Lerp(color, new Color(color.r, color.g, color.b, 1f), MainManager.TieFramerate(__instance.fadespeed));
                }
            }
        }
    }
}
