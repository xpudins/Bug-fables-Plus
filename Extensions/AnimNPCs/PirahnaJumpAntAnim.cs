using System.Collections;
using UnityEngine;

namespace BFPlus.Extensions.AnimNPCs
{
    class PirahnaJumpAntAnim : AnimNPC
    {
        void Start()
        {
            Setup();
        }
        protected override IEnumerator Anim()
        {
            doingAnim = true;
            entity.overrideanim = true;

            entity.PlaySound("Chew", 0.4f);
            entity.animstate = 100;
            yield return new WaitForSeconds(0.33f);

            entity.animstate = 102;
            entity.PlaySound("Bite", 0.4f);
            yield return EventControl.halfsec;

            entity.animstate = (int)MainManager.Animations.Idle;
            entity.overrideanim = false;
            doingAnim = false;
        }
    }
}

