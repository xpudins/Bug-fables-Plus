using System.Collections;
using System.Linq;
using UnityEngine;

namespace BFPlus.Extensions.Events.JumpAntEvents
{
    public class JumpAntIntermission1 : NewEvent
    {
        protected override IEnumerator DoEvent(NPCControl caller, EventControl instance)
        {
            while (MainManager.instance.message)
                yield return null;

            EntityControl[] party = MainManager.GetPartyEntities(true);
            EntityControl[] seedlings = MainManager.GetEntities(new int[] { 27, 28 });
            EntityControl jumpAnt = MainManager.GetEntity(29);
            EntityControl chompy = MainManager.map.chompy;
            Vector3[] posArray = new Vector3[]
            {
                new Vector3(-38.5f, 0, -4f),
                new Vector3(-37f, 0f,-3.5f),
                new Vector3(-36f, 0, -4f),
                new Vector3(-36.94f, 0, -2.94f)
            };
            jumpAnt.alwaysactive = true;
            foreach (var seed in seedlings)
            {
                seed.alwaysactive = true;
                seed.overrideflip = false;
            }

            MainManager.SetCamera(jumpAnt.transform.position + new Vector3(0, 1f, -1), 0.01f);

            MainManager.events.MoveParty(posArray);
            if (chompy != null)
            {
                chompy.transform.position = posArray[3];
                chompy.forcejump = true;
            }

            while (!MainManager.PartyIsNotMoving() || (chompy != null && chompy.forcemove))
            {
                yield return null;
            }

            yield return EventControl.tenthsec;
            foreach (var e in party)
            {
                e.flip = false;
                e.animstate = (int)MainManager.Animations.Surprized;
            }

            chompy?.FaceTowards(jumpAnt.transform.position);
            yield return EventControl.halfsec;

            yield return EventControl.sec;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[10], true, Vector3.zero, party[0].transform, null));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            MainManager.map.transform.Find("jumpAntAmbush").gameObject.SetActive(false);

            foreach (var seedling in seedlings)
            {
                Physics.IgnoreCollision(seedling.ccol, jumpAnt.ccol, true);
            }
            Physics.IgnoreCollision(seedlings[0].ccol, seedlings[1].ccol, true);

            MainManager.SetCamera(new Vector3(-39.5f, 1f, -6), 0.01f);

            posArray = new Vector3[]
            {
                new Vector3(-42f, 0, -4f),
                new Vector3(-43.5f, 0f,-3.5f),
            };

            for (int i = 0; i < seedlings.Length; i++)
            {
                seedlings[i].MoveTowards(posArray[i], 1.5f);
            }

            yield return new WaitUntil(() => seedlings.All(s => !s.forcemove));

            foreach (var seedling in seedlings)
            {
                seedling.overrideflip = false;
                seedling.FaceAhead();
                seedling.animstate = (int)MainManager.Animations.Chase;
            }
            yield return EventControl.tenthsec;

            jumpAnt.spin = Vector3.zero;
            MainManager.PlaySound("Jump");
            jumpAnt.Jump();
            yield return EventControl.halfsec;

            jumpAnt.MoveTowards(new Vector3(-37.2f, 0, -5.6f), 2, 1, (int)MainManager.Animations.Angry, false, new Vector3(-50, 0, 0));
            yield return new WaitUntil(() => !jumpAnt.forcemove);

            jumpAnt.FaceBehind();

            for (int i = 0; i < 2; i++)
            {
                foreach (var seedling in seedlings)
                {
                    MainManager.PlaySound("Jump");
                    seedling.Jump();
                    yield return EventControl.tenthsec;
                }
                yield return new WaitUntil(() => seedlings.All(s => s.onground));
            }

            foreach (var e in party)
            {
                e.animstate = (int)MainManager.Animations.BattleIdle;
            }

            yield return EventControl.halfsec;
            MainManager.instance.StartCoroutine(BattleControl.StartBattle(
                new int[] { (int)NewEnemies.RedSeedling, (int)NewEnemies.BlueSeedling },
                (int)MainManager.BattleMaps.Grasslands1, -1, NewMusic.EventBattle.ToString(), null, false)
            );

            yield return EventControl.sec;
            while (MainManager.battle != null)
            {
                yield return null;
            }
            MainManager.SetCamera(new Vector3(-39.5f, 1f, -6), 1);

            yield return EventControl.halfsec;
            MainManager.FadeOut();

            foreach (var seedling in seedlings)
            {
                seedling.animstate = (int)MainManager.Animations.Hurt;
                seedling.destroytype = NPCControl.DeathType.SpinSmoke;
                instance.StartCoroutine(seedling.Death());
            }
            yield return EventControl.halfsec;

            MainManager.SetCamera(new Vector3(-39.5f, 0f, -4), 0.1f);

            jumpAnt.MoveTowards(new Vector3(-41.56f, 0, -3.78f));
            yield return new WaitUntil(() => !jumpAnt.forcemove);

            for (int i = 0; i < 4; i++)
            {
                jumpAnt.flip = !jumpAnt.flip;
                yield return EventControl.halfsec;
            }
            jumpAnt.FaceAhead();

            foreach (var p in party)
                p.animstate = 0;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[3], true, Vector3.zero, party[0].transform, null));
            while (MainManager.instance.message)
            {
                yield return null;
            }
            MainManager.ChangeMusic(NewMusic.JumpAntOverworld.ToString());

            jumpAnt.FaceAhead();
            yield return EventControl.tenthsec;
            MainManager.PlaySound("TextBack", 1.2f, 1);
            jumpAnt.animstate = (int)MainManager.Animations.Happy;
            yield return EventControl.sec;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[4], true, Vector3.zero, party[1].transform, null));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            jumpAnt.animstate = (int)MainManager.Animations.PickAction;
            yield return EventControl.halfsec;

            jumpAnt.Emoticon(MainManager.Emoticons.Exclamation, 60);
            yield return EventControl.quartersec;

            jumpAnt.animstate = (int)MainManager.Animations.ItemGet;
            jumpAnt.spin = new Vector3(0, 20);
            MainManager.PlaySound("Switch", 0.8f, 1);

            yield return EventControl.halfsec;

            jumpAnt.spin = Vector3.zero;

            jumpAnt.animid = (int)MainManager.AnimIDs.SeedlingKing - 1;
            jumpAnt.UpdateSprite();
            jumpAnt.startscale = MainManager.endata[jumpAnt.animid + 1].startscale;
            jumpAnt.spritetransform.localScale = jumpAnt.startscale;


            yield return null;
            jumpAnt.animstate = 100;

            yield return EventControl.tenthsec;

            foreach (var p in party)
            {
                if (p.animid == 0)
                    p.animstate = (int)MainManager.Animations.Surprized;
                else if (p.animid == 1)
                    p.animstate = 105;
                else
                    p.animstate = (int)MainManager.Animations.BattleIdle;
            }
            yield return EventControl.halfsec;

            jumpAnt.spin = new Vector3(0, 20);
            MainManager.PlaySound("Switch", 0.8f, 1);

            yield return EventControl.halfsec;
            jumpAnt.spin = Vector3.zero;
            jumpAnt.animid = (int)NewAnimID.JumpAnt;
            jumpAnt.startscale = MainManager.endata[jumpAnt.animid + 1].startscale;
            jumpAnt.spritetransform.localScale = jumpAnt.startscale;

            jumpAnt.UpdateSprite();

            yield return null;
            jumpAnt.animstate = (int)MainManager.Animations.Jump;
            MainManager.PlaySound("Jump");
            yield return MainManager.ArcMovement(jumpAnt.gameObject, jumpAnt.transform.position, jumpAnt.transform.position + new Vector3(-2, 0, 0), Vector3.zero, 3, 20, false);

            jumpAnt.animstate = (int)MainManager.Animations.Flustered;
            jumpAnt.FaceAhead();
            yield return EventControl.tenthsec;

            foreach (var p in party)
            {
                p.animstate = (int)MainManager.Animations.PickAction;
            }

            yield return EventControl.halfsec;

            jumpAnt.animstate = (int)MainManager.Animations.Jump;

            for (int i = 0; i < 2; i++)
            {
                MainManager.PlaySound("Jump");
                yield return MainManager.ArcMovement(jumpAnt.gameObject, jumpAnt.transform.position, jumpAnt.transform.position + new Vector3(i * 1, 0, 0), Vector3.zero, 3, 20, false);
                yield return null;

                jumpAnt.animid = i == 0 ? (int)NewAnimID.RedSeedling : (int)NewAnimID.BlueSeedling;
                jumpAnt.startscale = MainManager.endata[jumpAnt.animid + 1].startscale;
                jumpAnt.spritetransform.localScale = jumpAnt.startscale;

                jumpAnt.UpdateSprite();
                yield return null;
                jumpAnt.animstate = (int)MainManager.Animations.Chase;
                yield return EventControl.halfsec;

                if (i == 0)
                {
                    foreach (var p in party)
                    {
                        if (p.animid == 0)
                            p.animstate = (int)MainManager.Animations.Surprized;
                        else if (p.animid == 1)
                            p.animstate = 105;
                        else
                            p.animstate = (int)MainManager.Animations.BattleIdle;
                    }
                }
            }

            jumpAnt.spin = new Vector3(0, 20);
            yield return EventControl.halfsec;

            MainManager.PlaySound("Switch", 0.8f, 1);
            jumpAnt.animid = (int)NewAnimID.JumpAnt;
            jumpAnt.startscale = MainManager.endata[jumpAnt.animid + 1].startscale;
            jumpAnt.animstate = (int)MainManager.Animations.Hurt;
            jumpAnt.spritetransform.localScale = jumpAnt.startscale;
            yield return EventControl.halfsec;

            yield return jumpAnt.SlowSpinStop(new Vector3(0, 20), 20);
            jumpAnt.spin = Vector3.zero;
            MainManager.PlaySound("Death3");
            jumpAnt.animstate = (int)MainManager.Animations.KO;
            yield return EventControl.halfsec;

            jumpAnt.animstate = 116;
            yield return new WaitForSeconds(1.83f);
            MainManager.PlaySound("AtkSuccess", 1.2f, 1);

            yield return EventControl.halfsec;
            jumpAnt.animstate = 0;
            jumpAnt.FaceAhead();

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[5], true, Vector3.zero, party[0].transform, null));
            while (MainManager.instance.message)
            {
                yield return null;
            }
            //nodding
            jumpAnt.animstate = (int)MainManager.Animations.Upset;

            yield return EventControl.halfsec;

            jumpAnt.animstate = 0;

            int dialogueId = MainManager.instance.flags[143] ? 9 : 6;
            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[dialogueId], true, Vector3.zero, party[1].transform, null));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            yield return WaitJump(jumpAnt, 2);

            yield return GiveJumpAntReward(jumpAnt, instance, 20, 67);

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[7], true, Vector3.zero, party[0].transform, null));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            jumpAnt.animstate = 118;
            yield return EventControl.sec;

            //jump ant runs to right

            jumpAnt.MoveTowards(new Vector3(-60, 0, 0), 2);

            MainManager.FadeMusic(0.01f);

            yield return new WaitUntil(() => !jumpAnt.forcemove);

            jumpAnt.gameObject.SetActive(false);
            MainManager.SetCamera(new Vector3(-38f, 0f, -4), 0.1f);

            party[0].FaceTowards(party[1].transform.position);

            yield return EventControl.halfsec;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[8], true, Vector3.zero, party[2].transform, null));
            while (MainManager.instance.message)
            {
                yield return null;
            }
            MainManager.instance.boardquests[1].Add((int)NewQuest.AnAdventureWithJumpAnt);
            MainManager.UpdateJounal();
            MainManager.ResetCamera();
            MainManager.ChangeMusic();
            MainManager.instance.flags[966] = true;
            MainManager.instance.flags[969] = false;
            if (MainManager.instance.flags[88])
            {
                MainManager.instance.flags[968] = true;
            }

            yield return EventControl.halfsec;
        }
    }
}
