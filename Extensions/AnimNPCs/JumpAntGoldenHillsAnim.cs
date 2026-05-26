using System.Collections;
using UnityEngine;

namespace BFPlus.Extensions.AnimNPCs
{
    class JumpAntGoldenHillsAnim : AnimNPC
    {
        Transform tiredPart;
        float shake = 0.05f;
        void Start()
        {
            Setup();
            tiredPart = (Instantiate(Resources.Load("Prefabs/Particles/Tired")) as GameObject).transform;
            tiredPart.parent = entity.transform;
            tiredPart.localPosition = new Vector3(0f, 1);
        }
        protected override IEnumerator Anim()
        {
            doingAnim = true;
            yield return EventControl.tenthsec;
            entity.animstate = (int)MainManager.Animations.Block;
            yield return entity.ShakeSprite(new Vector3(shake, 0f), 999999999999);

            doingAnim = false;
        }

        void OnDestroy()
        {
            Destroy(tiredPart.gameObject);
            entity.overrideanim = false;
            doingAnim = false;
        }
    }
}

