using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BFPlus.Extensions.Events.JumpAntEvents
{
    public class JumpAntIntermission5Event : NewEvent
    {
        protected override IEnumerator DoEvent(NPCControl caller, EventControl instance)
        {
            while (MainManager.instance.message)
                yield return null;

            EntityControl[] party = MainManager.GetPartyEntities(true);
            EntityControl jumpAnt = MainManager.GetEntity(34);
            EntityControl[] leafBugs = MainManager.GetEntities(new int[] { 35, 36, 37 });

            EntityControl[] bushes = MainManager.GetEntities(new int[] { 13, 15 });


            foreach (var leafbug in leafBugs)
            {
                foreach (var l in leafBugs)
                {
                    Physics.IgnoreCollision(leafbug.ccol, l.ccol);
                }
            }

            foreach (var b in bushes)
                b.gameObject.SetActive(false);

            for (int i = 0; i < party.Length; i++)
            {
                party[i].Emoticon(MainManager.Emoticons.Exclamation, 60);
            }

            yield return EventControl.sec;
            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[5], true, Vector3.zero, party[0].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            MainManager.SetCamera(jumpAnt.transform, jumpAnt.transform.position, 0.035f);
            yield return new WaitForSeconds(2);

            MainManager.ResetCamera();
            yield return EventControl.halfsec;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[6], true, Vector3.zero, party[1].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            MainManager.FadeIn(0.05f);
            yield return EventControl.sec;
            yield return EventControl.halfsec;

            Vector3[] posArray = new Vector3[]
            {
                new Vector3(5f, 0, -9.56f),
                new Vector3(3.5f, 0, -9f),
                new Vector3(2, 0, -9.56f),
                new Vector3(1.5f, 0, -9.4f)
            };
            instance.SetPartyPos(posArray, new bool[] { true, true, true });
            MainManager.ResetCamera(true);
            MainManager.FadeOut(0.05f);
            yield return EventControl.sec;

            MainManager.ChangeMusic(NewMusic.JumpAntOverworld.ToString(), 0.05f);
            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[7], true, Vector3.zero, party[1].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }
            yield return EventControl.halfsec;

            jumpAnt.Emoticon(MainManager.Emoticons.Exclamation, 30);
            jumpAnt.animstate = 0;
            jumpAnt.flip = false;
            yield return EventControl.quartersec;

            jumpAnt.animstate = 118;
            yield return new WaitForSeconds(2);

            jumpAnt.animstate = 121;
            jumpAnt.flip = true;
            yield return EventControl.halfsec;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[8], true, Vector3.zero, party[0].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }
            yield return EventControl.quartersec;

            MainManager.SetCamera(leafBugs[1].transform.position + new Vector3(-1, -1, -3), 0.05f);
            yield return EventControl.sec;

            List<Coroutine> coroutines = new List<Coroutine>();

            for (int i = 0; i < leafBugs.Length; i++)
            {
                leafBugs[i].flip = i % 2 == 0;
                coroutines.Add(MainManager.events.StartCoroutine(EventControl_Ext.FlipAround(leafBugs[i])));
            }
            yield return new WaitForSeconds(2);

            foreach (var coroutine in coroutines)
                MainManager.events.StopCoroutine(coroutine);
            coroutines.Clear();

            leafBugs[1].Emoticon(MainManager.Emoticons.QuestionMark, 30);
            leafBugs[2].Emoticon(MainManager.Emoticons.DotsLong, 100);

            MainManager.PlaySound("Lost");

            MiniBubble minibubble = MiniBubble.SetUp($"      |Position,-0.3||icon,{(int)NewGui.SplotchSpider},2|", leafBugs[0], new Vector3(-2.5f, 2f, 10f), 0, 1f);
            while (minibubble != null)
            {
                yield return null;
            }


            for (int i = 0; i < leafBugs.Length - 1; i++)
            {
                instance.StartCoroutine(MoveLeafbugToLift(leafBugs[i], i));
                yield return EventControl.quartersec;
            }
            yield return MoveLeafbugToLift(leafBugs[leafBugs.Length - 1], leafBugs.Length - 1);

            MainManager.PlaySound("ElevatorStart");
            MainManager.ResetCamera();
            yield return EventControl.halfsec;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[9], true, Vector3.zero, party[2].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }
            yield return EventControl.quartersec;

            Vector3 startPos = jumpAnt.transform.position;

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

            party[1].animstate = 105;
            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[10], true, Vector3.zero, party[1].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

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

            MainManager.PlaySound("Fall");
            EntityControl spider = MainManager.GetEntity(38);

            spider.transform.localEulerAngles = new Vector3(0, 0, 0);
            spider.transform.position = jumpAnt.transform.position + Vector3.up * 10;
            spider.animstate = (int)MainManager.Animations.Hurt;
            spider.LockRigid(true);

            startPos = spider.transform.position;
            yield return BattleControl_Ext.LerpPosition(30, startPos, jumpAnt.transform.position + Vector3.up, spider.transform);

            MainManager.instance.StartCoroutine(BattleControl.StartBattle(
                new int[] { (int)NewEnemies.SplotchSpider, (int)MainManager.Enemies.SkullCaterpillar, (int)NewEnemies.Dewling, (int)MainManager.Enemies.JumpingSpider },
                (int)MainManager.BattleMaps.Swamplands, -1, NewMusic.EventBattle.ToString(), null, false)
            );

            yield return EventControl.sec;
            while (MainManager.battle != null)
            {
                yield return null;
            }
            yield return EventControl.halfsec;

            MainManager.SetCamera(party[0].transform.position, 1);

            foreach (var p in party)
                p.animstate = (int)MainManager.Animations.WeakBattleIdle;

            jumpAnt.flip = false;
            jumpAnt.animstate = (int)MainManager.Animations.Block;
            spider.animstate = (int)MainManager.Animations.Hurt;
            spider.destroytype = NPCControl.DeathType.SpinSmoke;
            yield return EventControl.halfsec;
            MainManager.FadeOut();

            instance.StartCoroutine(spider.Death());

            yield return EventControl.sec;

            MainManager.ChangeMusic(NewMusic.JumpAntOverworld.ToString(), 0.05f);
            yield return EventControl.halfsec;

            jumpAnt.animstate = (int)MainManager.Animations.Idle;
            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[11], true, Vector3.zero, party[2].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            yield return EventControl.halfsec;

            jumpAnt.animstate = (int)MainManager.Animations.Upset;
            yield return EventControl.sec;

            jumpAnt.animstate = 118;
            yield return EventControl.sec;

            yield return GiveJumpAntReward(jumpAnt, instance, 20, 71);

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[12], true, Vector3.zero, party[0].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }
            yield return EventControl.quartersec;

            MainManager.PlaySound("TextBack", 1.2f, 1);
            jumpAnt.animstate = (int)MainManager.Animations.Happy;
            yield return EventControl.sec;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[13], true, Vector3.zero, party[1].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }
            yield return EventControl.halfsec;

            yield return WaitJump(jumpAnt);

            foreach (var p in party)
                Physics.IgnoreCollision(jumpAnt.ccol, p.ccol);

            jumpAnt.animstate = 125;
            jumpAnt.PlaySound("BeetleDash", 1.2f, 1);
            jumpAnt.MoveTowards(new Vector3(-14f, 0, -4.29f), 3, 125, 0);
            yield return EventControl.tenthsec;

            foreach (var p in party)
                p.flip = false;

            yield return new WaitUntil(() => !jumpAnt.forcemove);
            jumpAnt.gameObject.SetActive(false);

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[14], true, Vector3.zero, party[1].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }
            yield return EventControl.halfsec;

            if (!MainManager.instance.librarystuff[0, (int)NewDiscoveries.LeafbugVillage])
            {
                MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[15], true, Vector3.zero, party[2].transform, caller));
                while (MainManager.instance.message)
                {
                    yield return null;
                }
                yield return EventControl.halfsec;
            }

            foreach (var leafbug in leafBugs)
            {
                leafbug.gameObject.SetActive(false);
            }

            MainManager.ResetCamera();
            MainManager.ChangeMusic();

            MainManager.instance.flags[976] = true;
            MainManager.instance.flags[977] = false;

            if (MainManager.instance.flags[454])
                MainManager.instance.flags[979] = true;

            MainManager.UpdateJounal();
        }

        IEnumerator MoveLeafbugToLift(EntityControl leafbug, int index)
        {
            Vector3 liftSpot = new Vector3(-6.94f, 12.57f, -1.29f);
            switch (index)
            {
                case 1:
                    liftSpot += Vector3.left;
                    break;
                case 2:
                    liftSpot += new Vector3(-0.5f, 0, -0.2f);
                    break;
            }

            Vector3[] targetPositions = new Vector3[]
            {
                new Vector3(6f, 12.47f, -11.67f),
                new Vector3(2f, 12.17f, -10f),
                new Vector3(-3.51f, 12.17f, -7.6f),
                liftSpot
            };

            for (int i = 0; i < targetPositions.Length; i++)
            {
                if (i % 2 == 0)
                {
                    leafbug.LockRigid(false);
                    leafbug.MoveTowards(targetPositions[i]);
                    yield return new WaitUntil(() => !leafbug.forcemove);
                }
                else
                {
                    leafbug.LockRigid(true);
                    leafbug.PlaySound("Jump");
                    yield return MainManager.ArcMovement(leafbug.gameObject, leafbug.transform.position, targetPositions[i], Vector3.zero, 5, 30, false);
                }
            }
        }
    }
}



