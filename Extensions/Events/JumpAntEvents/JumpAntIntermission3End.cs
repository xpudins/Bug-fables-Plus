using System.Collections;
using UnityEngine;
using static MainManager;

namespace BFPlus.Extensions.Events.JumpAntEvents
{
    public class JumpAntIntermission3End : NewEvent
    {
        protected override IEnumerator DoEvent(NPCControl caller, EventControl instance)
        {
            while (MainManager.instance.message)
                yield return null;

            EntityControl[] party = MainManager.GetPartyEntities(true);
            EntityControl jumpAnt = MainManager.GetEntity(14);
            EntityControl voltshroom = MainManager.GetEntity(15);
            EntityControl chompy = MainManager.map.chompy;

            var glowTriggers = GameObject.FindObjectsOfType<GlowTrigger>();

            foreach (var glowTrigger in glowTriggers)
            {
                if (glowTrigger.name == "Cube")
                {
                    glowTrigger.enabled = false;
                }
            }

            Vector3[] posArrays = new Vector3[]
            {
                new Vector3(14.50f, -6f, 5.14f),
                new Vector3(13.36f, -6f, 4.93f),
                new Vector3(12f, -6, 4.90f),
                new Vector3(12f, -6, 5.5f),
            };

            instance.MoveParty(posArrays, true);
            MainManager.SetCamera(jumpAnt.transform.position, new Vector3(15, 0, 0), new Vector3(-0.5f, 2, -8), 0.05f);

            while (!MainManager.PartyIsNotMoving() || (chompy != null && chompy.forcemove))
            {
                yield return null;
            }

            foreach (var p in party)
                p.FaceTowards(jumpAnt.transform.position);

            chompy?.FaceTowards(jumpAnt.transform.position);

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[3], true, Vector3.zero, party[2].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            yield return WaitJump(jumpAnt, 2);

            jumpAnt.animstate = 121;
            yield return EventControl.halfsec;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[4], true, Vector3.zero, party[2].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }
            yield return EventControl.tenthsec;
            foreach (var p in party)
            {
                p.animstate = (int)MainManager.Animations.PickAction;
            }

            MainManager.SetCamera(new Vector3(15, 5, -5), new Vector3(-15, 0, 0), new Vector3(2, 0, -6), 0.05f);
            yield return new WaitForSeconds(2);

            foreach (var p in party)
            {
                p.animstate = (int)MainManager.Animations.Idle;
            }

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[5], true, Vector3.zero, party[1].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            MainManager.SetCamera(jumpAnt.transform.position, new Vector3(15, 0, 0), new Vector3(-0.5f, 2, -8), 0.05f);
            Vector3 startPos = jumpAnt.transform.position;
            yield return EventControl.halfsec;

            jumpAnt.animstate = (int)MainManager.Animations.Upset;
            yield return EventControl.halfsec;
            Vector3 startScale = jumpAnt.startscale;

            jumpAnt.PlaySound("Jump", 1, 0.9f);
            yield return WaitJump(jumpAnt, 1, 10, true);

            for (int i = 0; i < party.Length; i++)
            {
                if (i == 0)
                    party[i].animstate = (int)MainManager.Animations.Surprized;
                else if (i == 1)
                    party[i].animstate = 105;
                else
                    party[i].animstate = (int)MainManager.Animations.BattleIdle;
            }

            MainManager.PlaySound("Charge3", 1.4f, 1);
            jumpAnt.animstate = (int)MainManager.Animations.Angry;
            yield return jumpAnt.ChangeScale(new Vector3(startScale.x, 0.5f, startScale.z), 130f, false);

            jumpAnt.startscale = startScale;
            jumpAnt.LockRigid(true);
            jumpAnt.animstate = (int)MainManager.Animations.Jump;
            MainManager.PlaySound("Boing0", 1.1f, 1);

            yield return BattleControl_Ext.LerpPosition(20, jumpAnt.transform.position, jumpAnt.transform.position + Vector3.up * 10, jumpAnt.transform);
            MainManager.PlaySound("Death3");
            MainManager.ShakeScreen(1);
            MainManager.FadeMusic(1);

            jumpAnt.animstate = (int)MainManager.Animations.Hurt;
            jumpAnt.overrideheight = true;
            jumpAnt.rotater.gameObject.AddComponent<SpinAround>().itself = new Vector3(0, 0, -10);
            jumpAnt.spritetransform.localPosition = new Vector3(0, -1, 0);
            yield return EventControl.sec;
            MainManager.PlaySound("Fall");
            yield return BattleControl_Ext.LerpPosition(40, jumpAnt.transform.position, startPos, jumpAnt.transform);

            MainManager.PlaySound("Death3");

            jumpAnt.overrideheight = false;
            UnityEngine.Object.Destroy(jumpAnt.rotater.GetComponent<SpinAround>());
            jumpAnt.spritetransform.localPosition = Vector3.zero;

            jumpAnt.animstate = (int)MainManager.Animations.KO;
            jumpAnt.spin = Vector3.zero;
            jumpAnt.rotater.localEulerAngles = Vector3.zero;
            jumpAnt.LockRigid(false);
            yield return EventControl.halfsec;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[6], true, Vector3.zero, party[2].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            party[0].Emoticon(MainManager.Emoticons.Exclamation);
            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[7], true, Vector3.zero, party[0].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            MainManager.PlaySound("Fall");
            voltshroom.animstate = (int)MainManager.Animations.Hurt;
            voltshroom.LockRigid(true);

            voltshroom.transform.position = jumpAnt.transform.position + Vector3.up * 10;
            startPos = voltshroom.transform.position;
            yield return BattleControl_Ext.LerpPosition(30, startPos, jumpAnt.transform.position + Vector3.up, voltshroom.transform);

            MainManager.instance.StartCoroutine(BattleControl.StartBattle(
                new int[] { (int)NewEnemies.BatteryShroom, (int)MainManager.Enemies.Abomihoney, (int)MainManager.Enemies.ShockWorm },
                (int)MainManager.BattleMaps.FactoryP, -1, NewMusic.EventBattle.ToString(), null, false)
            );

            yield return EventControl.sec;
            while (MainManager.battle != null)
            {
                yield return null;
            }

            yield return EventControl.halfsec;

            MainManager.SetCamera(jumpAnt.transform.position, new Vector3(15, 0, 0), new Vector3(-0.5f, 2, -8), 1);

            foreach (var p in party)
                p.animstate = (int)MainManager.Animations.BattleIdle;

            jumpAnt.animstate = (int)MainManager.Animations.Angry;
            voltshroom.animstate = (int)MainManager.Animations.Hurt;
            voltshroom.transform.position = new Vector3(15.27f, -6, 7f);
            voltshroom.SetAnimForce();
            yield return EventControl.halfsec;
            MainManager.FadeOut();

            instance.StartCoroutine(voltshroom.Death());

            yield return EventControl.sec;
            yield return EventControl.halfsec;

            MainManager.ChangeMusic(NewMusic.JumpAntOverworld.ToString(), 0.05f);
            jumpAnt.animstate = (int)MainManager.Animations.Idle;
            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[8], true, Vector3.zero, party[2].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            jumpAnt.Emoticon(Emoticons.QuestionMark, 60);
            yield return EventControl.sec;

            party[2].animstate = 102;
            yield return new WaitForSeconds(0.75f);
            party[2].animstate = 119;
            yield return EventControl.quartersec;

            for (int i = 0; i < 2; i++)
                party[i].animstate = (int)MainManager.Animations.Happy;

            jumpAnt.animstate = (int)MainManager.Animations.Flustered;

            MainManager.PlaySound("Shield");
            jumpAnt.CreateShield();
            jumpAnt.shieldenabled = true;
            yield return new WaitForSeconds(2);

            party[2].animstate = 0;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[9], true, Vector3.zero, party[1].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            yield return EventControl.halfsec;
            MainManager.PlaySound("TextBack", 1.2f, 1);
            jumpAnt.animstate = (int)MainManager.Animations.Happy;
            yield return EventControl.sec;

            yield return GiveJumpAntReward(jumpAnt, instance, 20, 69);

            Vector3[] path = new Vector3[] { new Vector3(15.99f, -6, 1.89f), new Vector3(15.99f, -5, -1.13f), new Vector3(0, 0, -1.16f) };

            foreach (var p in path)
            {
                jumpAnt.forcejump = true;
                jumpAnt.MoveTowards(p, 2);
                yield return new WaitUntil(() => !jumpAnt.forcemove);
                jumpAnt.Jump();
                party[1].animstate = (int)MainManager.Animations.WeakBattleIdle;
            }

            jumpAnt.gameObject.SetActive(false);

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[10], true, Vector3.zero, party[0].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            yield return EventControl.quartersec;

            foreach (var glowTrigger in glowTriggers)
            {
                if (glowTrigger.name == "Cube")
                {
                    glowTrigger.enabled = true;
                }
            }
            MainManager.ResetCamera();
            MainManager.ChangeMusic();
            MainManager.instance.flags[971] = true;
            if (MainManager.instance.flags[345])
            {
                MainManager.instance.flags[975] = true;
            }
            MainManager.UpdateJounal();
            yield return EventControl.halfsec;
        }
    }
}


