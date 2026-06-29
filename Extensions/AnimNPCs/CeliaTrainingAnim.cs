using System.Collections;
using UnityEngine;

namespace BFPlus.Extensions.AnimNPCs
{
    class CeliaTrainingAnim : AnimNPC
    {
        void Start()
        {
            target = new Vector3(15.2f, 2f, -12f);
            animSpeed = 30f;
            baseWaitTimes = 30f;
            Setup();
        }
        protected override IEnumerator Anim()
        {
            doingAnim = true;
            entity.overrideanim = true;

            entity.animstate = 105;
            entity.spin = new Vector3(0f, 30f);
            yield return EventControl.halfsec;

            entity.spin = Vector3.zero;
            entity.PlaySound("Toss8", 1f, 1f);
            entity.PlaySound("Toss2", 1f, 0.8f);
            Transform shield = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/Objects/Shield")).transform;
            shield.GetComponent<SpriteRenderer>().materials = new Material[] { MainManager.spritematlit };
            shield.transform.parent = entity.transform;

            entity.animstate = 102;
            ParticleSystem.MainModule p = shield.GetComponentInChildren<ParticleSystem>().main;
            Vector3 startPos = entity.transform.position + new Vector3(0f, 1.25f, 0f);
            float a = 0f;
            do
            {
                shield.position = Vector3.Lerp(startPos, target, a / animSpeed);
                shield.localEulerAngles += Vector3.forward * 10f * MainManager.framestep;
                p.startRotationMultiplier = shield.localEulerAngles.z;

                a += MainManager.TieFramerate(1f);
                yield return null;

            } while (a < animSpeed);

            MainManager.PlaySoundAt("WoodHit", 1f, target);
            yield return EventControl.tenthsec;

            a = 0f;
            do
            {
                shield.position = Vector3.Lerp(target, startPos, a / animSpeed) + Vector3.up * 1.5f;
                shield.localEulerAngles += Vector3.forward * (-10f * MainManager.framestep);
                p.startRotationMultiplier = shield.localEulerAngles.z;
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < animSpeed);

            MainManager.StopSound(3);
            entity.PlaySound("Ding2", 1f, 1f);
            Destroy(shield.gameObject);
            entity.animstate = 104;

            yield return entity.SlowSpinStop(Vector3.up * -50f, 30f);
            entity.animstate = (int)MainManager.Animations.Idle;
            entity.overrideanim = false;
            doingAnim = false;
        }
    }
}
