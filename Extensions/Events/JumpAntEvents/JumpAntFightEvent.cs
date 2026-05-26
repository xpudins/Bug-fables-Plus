using System.Collections;

namespace BFPlus.Extensions.Events.JumpAntEvents
{
    public class JumpAntFightEvent : NewEvent
    {
        protected override IEnumerator DoEvent(NPCControl caller, EventControl instance)
        {
            while (MainManager.instance.message)
            {
                yield return null;
            }

            yield return EventControl.tenthsec;
            MainManager.instance.StartCoroutine(BattleControl.StartBattle(
                new int[] { (int)NewEnemies.JumpAnt, (int)NewEnemies.Caveling },
                (int)MainManager.BattleMaps.AssociationHQ, -1, NewMusic.JumpAntTheme.ToString(), null, false)
            );

            yield return EventControl.sec;
            while (MainManager.battle != null)
            {
                yield return null;
            }
            MainManager.ResetCamera();

            yield return EventControl.halfsec;
            MainManager.FadeOut();
            MainManager.ResetCamera();
            MainManager.ChangeMusic();
        }
    }
}
