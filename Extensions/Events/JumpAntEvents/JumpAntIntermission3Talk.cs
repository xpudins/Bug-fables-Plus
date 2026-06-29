using System.Collections;
using UnityEngine;

namespace BFPlus.Extensions.Events.JumpAntEvents
{
    public class JumpAntIntermission3Talk : NewEvent
    {
        protected override IEnumerator DoEvent(NPCControl caller, EventControl instance)
        {
            while (MainManager.instance.message)
                yield return null;

            EntityControl[] party = MainManager.GetPartyEntities(true);
            EntityControl jumpAnt = MainManager.GetEntity(23);
            EntityControl chompy = MainManager.map.chompy;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[5], true, Vector3.zero, party[0].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }
            yield return EventControl.quartersec;

            jumpAnt.animstate = (int)MainManager.Animations.Sit;
            yield return EventControl.quartersec;
            party[0].animstate = 0;
            yield return EventControl.sec;

            jumpAnt.animstate = (int)MainManager.Animations.PickAction;
            yield return EventControl.sec;

            jumpAnt.Emoticon(MainManager.Emoticons.Exclamation, 60);
            jumpAnt.animstate = 118;
            yield return new WaitForSeconds(1.1f);

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[6], true, Vector3.zero, party[1].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            Vector3 startPos = jumpAnt.transform.position;

            yield return WaitJump(jumpAnt);

            Vector3 targetPos = new Vector3(-0.55f, 13.2f, 23.54f);
            MainManager.SetCamera(targetPos + new Vector3(-1, 4, -7), MainManager.defaultcamangle, new Vector3(0, 0, 0), 0.01f);

            jumpAnt.MoveTowards(targetPos);
            yield return new WaitUntil(() => !jumpAnt.forcemove);

            foreach (var p in party)
            {
                p.FaceTowards(jumpAnt.transform.position, false, true);
                p.backsprite = true;
            }

            yield return EventControl.quartersec;

            bool startFlip = jumpAnt.flip;
            for (int i = 0; i < 3; i++)
            {
                MainManager.PlaySound("Alarm2");
                jumpAnt.flip = !jumpAnt.flip;
                yield return EventControl.halfsec;
            }

            jumpAnt.animstate = 114;
            yield return EventControl.halfsec;

            MainManager.PlaySound("Shock");
            MainManager.PlaySound("Damage0");
            jumpAnt.animstate = (int)MainManager.Animations.Flustered;
            jumpAnt.LockRigid(true);
            jumpAnt.flip = startFlip;

            MainManager.ResetCamera();
            yield return BattleControl_Ext.LerpPosition(30, jumpAnt.transform.position, jumpAnt.transform.position + Vector3.up * 10, jumpAnt.transform);

            yield return EventControl.halfsec;
            MainManager.PlaySound("Fall");

            jumpAnt.animstate = (int)MainManager.Animations.Hurt;
            yield return BattleControl_Ext.LerpPosition(20, jumpAnt.transform.position, startPos, jumpAnt.transform);

            for (int i = 0; i < party.Length; i++)
            {
                if (i != 2)
                    party[i].animstate = (int)MainManager.Animations.Surprized;
                party[i].FaceTowards(jumpAnt.transform.position, true);
                party[i].backsprite = false;

            }

            jumpAnt.LockRigid(false);
            MainManager.PlaySound("Death3");
            jumpAnt.animstate = (int)MainManager.Animations.KO;
            MainManager.ShakeScreen(0.5f);

            for (int i = 0; i < 2; i++)
            {
                party[i].Jump(1);
            }

            yield return EventControl.quartersec;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[7], true, Vector3.zero, party[0].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            jumpAnt.animstate = 116;
            yield return new WaitForSeconds(1.6f);
            MainManager.PlaySound("TextBack", 1.2f, 1);
            jumpAnt.animstate = (int)MainManager.Animations.Happy;
            yield return EventControl.sec;
            jumpAnt.animstate = 0;
            party[1].animstate = 0;

            MainManager.FadeMusic(0.05f);

            jumpAnt.MoveTowards(new Vector3(-1.5f, 13.2f, 30.72f));
            yield return new WaitUntil(() => !jumpAnt.forcemove);
            jumpAnt.gameObject.SetActive(false);

            yield return EventControl.halfsec;

            MainManager.ResetCamera();
            MainManager.ChangeMusic();
            MainManager.instance.flags[970] = true;

            yield return EventControl.halfsec;
        }
    }
}

