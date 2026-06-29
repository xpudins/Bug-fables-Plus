using System.Collections;
using UnityEngine;

namespace BFPlus.Extensions.AnimNPCs
{
    class GenTrainingAnim : AnimNPC
    {
        void Start()
        {
            target = new Vector3(9.4f, 2f, 7f);
            animSpeed = 20f;
            baseWaitTimes = 60f;
            Setup();
        }
        protected override IEnumerator Anim()
        {
            doingAnim = true;
            entity.overrideanim = true;

            Vector3 startPos = entity.transform.position + new Vector3(0.75f, 1f, -0.1f);

            int dartType = UnityEngine.Random.Range(0, 2);
            SpriteRenderer item = MainManager.NewSpriteObject(startPos, entity.transform, MainManager.itemsprites[0, (dartType == 0) ? 88 : 40]);
            item.transform.localEulerAngles = new Vector3(0f, 0f, 135f);
            entity.animstate = 100;
            entity.PlaySound("Toss12", 0.2f);

            float a = 0;
            do
            {
                item.transform.position = Vector3.Lerp(startPos, target, a / animSpeed);
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < animSpeed);

            entity.animstate = (int)MainManager.Animations.Idle;
            MainManager.DeathSmoke(target);
            Destroy(item.gameObject);
            entity.overrideanim = false;
            doingAnim = false;
        }
    }
}
