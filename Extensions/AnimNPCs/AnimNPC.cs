using System;
using System.Collections;
using UnityEngine;

namespace BFPlus.Extensions
{
    public class AnimNPC : MonoBehaviour
    {
        public bool doingAnim = false;
        protected EntityControl entity;
        protected Vector3 target;

        protected float waitTimes;
        public float baseWaitTimes = 60;
        protected float animSpeed = 20f;
        public bool doAnimWhilePaused = false;

        protected void Setup()
        {
            entity = GetComponent<EntityControl>();

            waitTimes = baseWaitTimes;
        }

        void Update()
        {
            if (((!MainManager.instance.pause && !MainManager.instance.minipause && !MainManager.instance.message) || doAnimWhilePaused) && entity != null && !doingAnim && MainManager.battle == null)
            {
                if (waitTimes <= 0)
                {
                    waitTimes = baseWaitTimes;
                    //Console.WriteLine($"starting anim of {base.name}");
                    StartCoroutine(Anim());
                }

                if (waitTimes > 0)
                {
                    waitTimes -= MainManager.framestep;
                }
            }
        }

        protected virtual IEnumerator Anim()
        {
            yield return null;
        }

        public static AnimNPC CreateAnimNPC(Type type, EntityControl entity, bool doAnimWhilePaused, float waitTimes)
        {
            AnimNPC npc = (AnimNPC)entity.gameObject.AddComponent(type);
            npc.doAnimWhilePaused = doAnimWhilePaused;
            npc.baseWaitTimes = waitTimes;
            npc.waitTimes = npc.baseWaitTimes;
            return npc;
        }

        void OnDisable()
        {
            doingAnim = false;
            if (entity != null)
            {
                entity.oldstate = -1;
                entity.laststate = "";
                entity.animstate = -1;
            }
        }
    }
}
