using System.Collections;
using System.Linq;
using UnityEngine;

namespace BFPlus.Extensions.Events.JumpAntEvents
{
    public class JumpAntIntermission2 : NewEvent
    {
        protected override IEnumerator DoEvent(NPCControl caller, EventControl instance)
        {
            while (MainManager.instance.message)
                yield return null;

            EntityControl[] party = MainManager.GetPartyEntities(true);
            EntityControl[] enemies = MainManager.GetEntities(new int[] { 37, 38, 39 });
            EntityControl jumpAnt = MainManager.GetEntity(40);
            EntityControl chompy = MainManager.map.chompy;

            jumpAnt.overrideanim = false;
            Vector3[] posArray = new Vector3[]
            {
                new Vector3(52.76f, 5, 7.6f),
                new Vector3(54.1f, 5, 8f),
                new Vector3(55.3f, 5, 7.6f),
                new Vector3(55.5f, 5, 7.67f)
            };

            for (int i = 0; i < party.Length; i++)
            {
                party[i].transform.position = new Vector3(54.6f, 5f, 5.52f);
            }

            if (chompy != null)
            {
                chompy.transform.position = new Vector3(54.6f, 5f, 5.52f);
            }

            MainManager.events.MoveParty(posArray);
            while (!MainManager.PartyIsNotMoving() || (chompy != null && chompy.forcemove))
            {
                yield return null;
            }
            posArray = new Vector3[]
            {
                new Vector3(52.75f, 5, 11f),
                new Vector3(51.5f, 5, 11.5f),
                new Vector3(50f, 5, 11f),
                new Vector3(49f, 5, 12f)
            };

            jumpAnt.alwaysactive = true;
            foreach (var enemy in enemies)
            {
                enemy.alwaysactive = true;
                enemy.overrideflip = false;
            }

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[2], true, Vector3.zero, party[1].transform, null));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            MainManager.SetCamera(jumpAnt.transform.position + new Vector3(1, 0.5f, -1), 0.01f);

            yield return EventControl.sec;

            AnimNPC anim = jumpAnt.gameObject.GetComponent<AnimNPC>();
            anim?.StopAllCoroutines();
            jumpAnt.StopAllCoroutines();
            jumpAnt.basestate = 0;
            UnityEngine.Object.Destroy(anim);

            jumpAnt.Emoticon(MainManager.Emoticons.Exclamation, 60);
            jumpAnt.animstate = (int)MainManager.Animations.Jump;
            MainManager.PlaySound("Jump");
            jumpAnt.Jump();
            foreach (var enemy in enemies)
            {
                anim = enemy.gameObject.GetComponent<AnimNPC>();

                anim?.StopAllCoroutines();
                UnityEngine.Object.Destroy(anim);

                enemy.Emoticon(MainManager.Emoticons.Exclamation, 60);
                Physics.IgnoreCollision(enemy.ccol, jumpAnt.ccol, true);

                foreach (var e in enemies)
                {
                    Physics.IgnoreCollision(e.ccol, enemy.ccol, true);
                }
            }
            yield return EventControl.tenthsec;
            while (!jumpAnt.onground)
            {
                yield return null;
            }
            jumpAnt.animstate = 0;

            yield return EventControl.halfsec;

            MainManager.events.MoveParty(posArray);
            if (chompy != null)
            {
                chompy.transform.position = posArray[3];
                chompy.forcejump = true;
            }


            posArray = new Vector3[]
            {
                new Vector3(55.8f, 5, 10.90f),
                new Vector3(57.48f, 5, 10f),
                new Vector3(57.7f, 5, 12f),

            };

            for (int i = 0; i < enemies.Length; i++)
            {
                enemies[i].MoveTowards(posArray[i], 1.5f);
            }
            jumpAnt.MoveTowards(new Vector3(50.86f, 5f, 9.7f), 1, 1, 0, false, new Vector3(60, 0, 0));


            while (!MainManager.PartyIsNotMoving() || (chompy != null && chompy.forcemove))
            {
                yield return null;
            }

            yield return EventControl.tenthsec;
            foreach (var e in party)
            {
                e.FaceTowards(enemies[0].transform.position);
                e.animstate = (int)MainManager.Animations.BattleIdle;
            }

            chompy?.FaceTowards(enemies[0].transform.position);

            while (enemies.Any(e => e.forcemove) || jumpAnt.forcemove)
            {
                yield return null;
            }

            foreach (var e in enemies)
            {
                if (e.animid == (int)MainManager.AnimIDs.Weevil - 1)
                    e.animstate = 23;
                else
                    e.animstate = 100;

                e.FaceTowards(party[0].transform.position);
            }
            jumpAnt.FaceTowards(enemies[0].transform.position);
            jumpAnt.animstate = (int)MainManager.Animations.Angry;
            yield return EventControl.halfsec;
            yield return EventControl.sec;

            MainManager.instance.StartCoroutine(BattleControl.StartBattle(
                new int[] { (int)NewEnemies.PirahnaChomp, (int)NewEnemies.PirahnaChomp, (int)MainManager.Enemies.Weevil },
                (int)MainManager.BattleMaps.GoldenBattle5, -1, NewMusic.EventBattle.ToString(), null, false)
            );

            yield return EventControl.sec;
            while (MainManager.battle != null)
            {
                yield return null;
            }

            yield return EventControl.halfsec;

            MainManager.SetCamera(new Vector3(53f, 5f, 11), 1);

            yield return EventControl.halfsec;
            MainManager.FadeOut();

            foreach (var e in enemies)
            {
                e.animstate = (int)MainManager.Animations.Hurt;
                e.destroytype = NPCControl.DeathType.SpinSmoke;
                instance.StartCoroutine(e.Death());
            }
            yield return EventControl.halfsec;

            foreach (var p in party)
            {
                Physics.IgnoreCollision(p.ccol, jumpAnt.ccol, true);
            }

            MainManager.SetCamera(jumpAnt.transform.position + new Vector3(1, 0.5f, -1), MainManager.defaultcamangle, new Vector3(1, 2.25f, -8.25f), 0.01f);

            jumpAnt.MoveTowards(new Vector3(56f, 5, 11f), 1, 1, 0, true, party[0].transform.position);
            yield return new WaitUntil(() => !jumpAnt.forcemove);

            foreach (var p in party)
            {
                p.animstate = 0;
            }

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[3], true, Vector3.zero, party[1].transform, null));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            jumpAnt.FaceBehind();

            MainManager.ChangeMusic(NewMusic.JumpAntOverworld.ToString());
            jumpAnt.animstate = (int)MainManager.Animations.Upset;
            yield return EventControl.halfsec;
            jumpAnt.animstate = 0;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[4], true, Vector3.zero, party[2].transform, null));
            while (MainManager.instance.message)
            {
                yield return null;
            }
            jumpAnt.Emoticon(MainManager.Emoticons.Exclamation, 30);
            yield return EventControl.halfsec;

            yield return WaitJump(jumpAnt, 2);

            jumpAnt.animstate = (int)MainManager.Animations.Angry;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[5], true, Vector3.zero, party[0].transform, null));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            //fireDrive start
            jumpAnt.animstate = 110;
            yield return EventControl.sec;
            party[0].FaceAhead();

            jumpAnt.animstate = 112;
            yield return EventControl.tenthsec;
            MainManager.PlaySound("HugeHit4", 1.2f, 1);
            yield return jumpAnt.StartCoroutine(jumpAnt.SlowSpinStop(new Vector3(0, 30, 0), 80));

            jumpAnt.spin = Vector3.zero;
            jumpAnt.animstate = 115;
            MainManager.PlaySound("AtkSuccess", 1.2f, 1);
            yield return EventControl.halfsec;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[6], true, Vector3.zero, party[1].transform, null));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            jumpAnt.animstate = (int)MainManager.Animations.Sit;
            yield return EventControl.sec;

            yield return GiveJumpAntReward(jumpAnt, instance, 20, 68);

            MainManager.FadeMusic(0.01f);
            MainManager.instance.camoffset += new Vector3(0, -1, -2);
            jumpAnt.MoveTowards(new Vector3(53f, 5f, 4f), 1, 1, 0, true, party[0].transform.position);
            yield return new WaitUntil(() => !jumpAnt.forcemove);

            jumpAnt.LockRigid(true);
            jumpAnt.animstate = (int)MainManager.Animations.Jump;
            MainManager.PlaySound("Jump");
            instance.StartCoroutine(MainManager.ArcMovement(jumpAnt.gameObject, jumpAnt.transform.position, new Vector3(50f, 0, 4.3f), Vector3.zero, 5, 30, false));

            MainManager.ChangeMusic();
            for (int i = 1; i < party.Length; i++)
            {
                party[i].Emoticon(MainManager.Emoticons.DotsLong, 100);
            }
            yield return new WaitUntil(() => party[1].emoticoncooldown <= 0);

            party[0].FaceBehind();
            MainManager.instance.camoffset += new Vector3(0, 1, 2);

            jumpAnt.gameObject.SetActive(false);

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[7], true, Vector3.zero, party[1].transform, null));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            party[2].Emoticon(MainManager.Emoticons.DotsLong, 100);
            yield return new WaitUntil(() => party[2].emoticoncooldown <= 0);

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[8], true, Vector3.zero, party[2].transform, null));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            MainManager.UpdateJounal();
            MainManager.ResetCamera();
            MainManager.instance.flags[967] = true;
            MainManager.instance.flags[968] = false;
            if (MainManager.instance.flags[299])
            {
                MainManager.instance.flags[972] = true;
            }
            yield return EventControl.halfsec;
        }
    }
}
