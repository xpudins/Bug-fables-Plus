using System.Collections;
using UnityEngine;

namespace BFPlus.Extensions.Events.JumpAntEvents
{
    public class JumpAntIntermission4 : NewEvent
    {
        protected override IEnumerator DoEvent(NPCControl caller, EventControl instance)
        {
            while (MainManager.instance.message)
                yield return null;

            EntityControl[] party = MainManager.GetPartyEntities(true);
            EntityControl jumpAnt = MainManager.GetEntity(7);

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[18], true, Vector3.zero, party[0].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }
            yield return EventControl.quartersec;

            jumpAnt.animstate = 118;
            yield return EventControl.quartersec;
            yield return EventControl.sec;

            jumpAnt.animstate = 122;

            foreach (var p in party)
                p.Emoticon(MainManager.Emoticons.Exclamation);
            yield return EventControl.sec;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[19], true, Vector3.zero, party[1].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }
            yield return EventControl.quartersec;
            MainManager.PlaySound("TextBack", 1.2f, 1);
            jumpAnt.animstate = (int)MainManager.Animations.Happy;
            yield return EventControl.sec;
            MainManager.FadeMusic(0.05f);

            MainManager.AddFollower(jumpAnt, -1);
            yield return EventControl.halfsec;
            MainManager.ResetCamera();
            MainManager.ChangeMusic();
            MainManager.instance.flags[973] = true;

            yield return EventControl.halfsec;
        }
    }
}


