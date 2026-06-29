using System.Collections;
using UnityEngine;

namespace BFPlus.Extensions
{
    public class JumpAntAmbushObj : MonoBehaviour
    {
        float duration = 60f;
        float time = 0;

        EntityControl[] seedlings = new EntityControl[2];
        Vector3[] basePositions = new Vector3[3];
        EntityControl jumpAnt;
        Vector3[] offsets = new Vector3[2] { new Vector3(-1f, 0, -0.1f), new Vector3(1f, 0, -0.1f) };
        int fromTarget = 0;
        bool doingAnim = false;
        void Start()
        {
            seedlings[0] = MainManager.map.entities[27];
            seedlings[1] = MainManager.map.entities[28];

            for (int i = 0; i < seedlings.Length; i++)
            {
                seedlings[i].npcdata.IsDummy();
                basePositions[i] = seedlings[i].transform.position;
            }

            jumpAnt = MainManager.map.entities[29];

            jumpAnt.transform.position = seedlings[0].transform.position + offsets[0];
            basePositions[2] = jumpAnt.transform.position;
            jumpAnt.animstate = (int)MainManager.Animations.Hurt;
        }

        void Update()
        {
            if (!doingAnim)
            {
                if (time < duration)
                {
                    int target = fromTarget == 0 ? 1 : 0;
                    time += MainManager.TieFramerate(1f);
                    jumpAnt.transform.position = MainManager.SmoothLerp(seedlings[fromTarget].transform.position + offsets[fromTarget], seedlings[target].transform.position + offsets[target], time / duration);
                    jumpAnt.spin = new Vector3(0, Mathf.Lerp(20, 0, time / duration), 0);
                }
                else
                {
                    doingAnim = true;
                    time = 0;
                    fromTarget = fromTarget == 0 ? 1 : 0;
                    StartCoroutine(DoAnim(seedlings[fromTarget]));
                }
            }
        }

        IEnumerator DoAnim(EntityControl entity)
        {
            jumpAnt.animstate = (int)MainManager.Animations.Hurt;
            entity.overrideflip = true;
            Vector3 targetPos = jumpAnt.transform.position;
            entity.animstate = (int)MainManager.Animations.Chase;
            entity.PlaySound("Spin10");
            entity.MoveTowards(targetPos + new Vector3(fromTarget == 0 ? 0.35f : -0.35f, 0f, -0.1f), 2.5f, entity.animstate, entity.animstate);
            while (entity.forcemove)
            {
                yield return null;
            }
            jumpAnt.PlaySound("Damage0");
            MainManager.HitPart(jumpAnt.transform.position + Vector3.up);
            //entity.Jump();

            doingAnim = false;
            yield return BattleControl_Ext.LerpPosition(10, entity.transform.position, basePositions[fromTarget], entity.transform);
            entity.flip = fromTarget == 1;
            entity.animstate = 0;
        }

        void OnDisable()
        {
            StopAllCoroutines();
            doingAnim = false;
            if (jumpAnt != null)
            {
                jumpAnt.oldstate = -1;
                jumpAnt.laststate = "";
                jumpAnt.animstate = -1;
                jumpAnt.transform.position = basePositions[2];
            }

            for (int i = 0; i < basePositions.Length - 1; i++)
            {
                if (seedlings[i] != null)
                {
                    seedlings[i].transform.position = basePositions[i];
                }
            }
            time = 0;
            fromTarget = 0;
        }
    }
}
