using System.Collections;

namespace BFPlus.Extensions.AnimNPCs
{
    class StratosTrainingAnim : AnimNPC
    {
        void Start()
        {
            baseWaitTimes = 45f;
            Setup();
        }
        protected override IEnumerator Anim()
        {
            doingAnim = true;
            entity.overrideanim = true;

            entity.PlaySound("Woosh6", 0.4f);
            entity.animstate = 102;
            yield return EventControl.sec;

            entity.animstate = 103;
            entity.PlaySound("Thud4", 0.4f, 1.1f);
            yield return EventControl.quartersec;
            entity.animstate = 104;
            yield return EventControl.sec;

            entity.PlaySound("Slash2", 0.4f);
            entity.animstate = 105;
            yield return EventControl.sec;

            entity.animstate = (int)MainManager.Animations.BattleIdle;
            entity.overrideanim = false;
            doingAnim = false;
        }
    }
}
