using BFPlus.Extensions.EnemyAI;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace BFPlus.Extensions.Events.JumpAntEvents
{
    public class JumpAntIntermission7 : NewEvent
    {
        protected override IEnumerator DoEvent(NPCControl caller, EventControl instance)
        {
            while (MainManager.instance.message)
                yield return null;

            EntityControl[] party = MainManager.GetPartyEntities(true);
            EntityControl jumpAnt = MainManager.GetEntity(73);
            EntityControl chompy = MainManager.map.chompy;
            EntityControl artis = MainManager.GetEntity(16);

            foreach (var p in party)
                p.FaceTowards(artis.transform.position);

            instance.SetPartyPos(new Vector3[] { new Vector3(-7.46f, 7.87f, 22.24f), new Vector3(-6.26f, 7.87f, 21.94f),
                                                new Vector3(-8.9f, 7.8f, 21.9f), new Vector3(-8.9f, 7.8f, 21.5f)
            }, new bool[] { true, false, true, true });

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[147], true, Vector3.zero, artis.transform, artis.npcdata));
            while (MainManager.instance.message)
            {
                yield return null;
            }
            Vector3 baseCamPos = new Vector3(-13.5f, 4.66f, 15f);

            MainManager.SetCamera(baseCamPos, 0.05f);
            jumpAnt.transform.position = new Vector3(-13.69f, 4.66f, 14.56f);
            jumpAnt.LockRigid(false);

            Vector3 lookAfter = new Vector3(-13f, 4.66f, 15.26f);
            yield return EventControl.quartersec;
            instance.StartCoroutine(WaitMove(jumpAnt, new Vector3(-11f, 4.66f, 14.79f), lookAfter: lookAfter));

            MainManager.ChangeMusic(NewMusic.JumpAntOverworld.ToString(), 0.05f);
            MainManager.instance.overridefollower = false;

            Vector3[] path = { new Vector3(-10.54f, 7.87f, 20.31f), new Vector3(-13.44f, 4.66f, 14.92f) };
            for (int i = 0; i < path.Length; i++)
            {
                party[0].MoveTowards(path[i]);
                yield return new WaitUntil(() => !party[0].forcemove);
            }
            MainManager.instance.overridefollower = true;
            Vector3[] posArray = new Vector3[]
            {
                new Vector3(-15.65f, 4.66f, 14.78f),
                new Vector3(-16.92f, 4.66f, 14.77f),
                new Vector3(-18.21f, 4.66f, 14.77f),
                new Vector3(-19.45f, 4.66f, 14.78f)
            };

            for (int i = 0; i < party.Length; i++)
            {
                instance.StartCoroutine(WaitMove(party[i], posArray[i], lookAfter: lookAfter));
            }

            if (chompy != null)
            {
                instance.StartCoroutine(WaitMove(chompy, posArray[posArray.Length - 1], lookAfter: lookAfter));
            }
            yield return null;
            while (!MainManager.PartyIsNotMoving() || (chompy != null && chompy.forcemove))
            {
                yield return null;
            }

            jumpAnt.flip = false;
            foreach (var p in party)
                p.flip = true;

            if (chompy != null)
                chompy.flip = true;

            jumpAnt.animstate = 118; //raisehand
            yield return EventControl.sec;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[148], true, Vector3.zero, party[0].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            yield return EventControl.tenthsec;

            jumpAnt.animstate = (int)MainManager.Animations.Upset;
            yield return EventControl.sec;

            jumpAnt.animstate = (int)MainManager.Animations.Idle;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[149], true, Vector3.zero, party[1].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            yield return EventControl.tenthsec;

            MainManager.PlaySound("TextBack", 1.2f, 1);
            jumpAnt.animstate = (int)MainManager.Animations.Happy;
            yield return EventControl.sec;

            //caveling comes out
            EntityControl caveling = MainManager.GetEntity(74);
            Vector3 cavelingPos = jumpAnt.transform.position + Vector3.right * 2;
            yield return GetOutPartner(caveling, jumpAnt, cavelingPos);

            yield return EventControl.quartersec;

            foreach (var p in party)
            {
                p.Emoticon(MainManager.Emoticons.Exclamation);
                p.animstate = (int)MainManager.Animations.BattleIdle;
            }

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[150], true, Vector3.zero, party[1].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            jumpAnt.animstate = (int)MainManager.Animations.Angry;

            float a = 0;
            float b = 30;

            do
            {
                caveling.height = Mathf.Lerp(0, 1, a / b);
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < b);

            yield return EventControl.halfsec;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[151], true, Vector3.zero, party[1].transform, caller));
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
            MainManager.instance.insideid = 0;
            MainManager.map.RefreshInsides(true, null);
            MainManager.GetEntity(2).gameObject.SetActive(true);
            MainManager.map.insides[0].GetComponent<Animator>().Play("Open");
            MainManager.ResetCamera(true);

            yield return EventControl.halfsec;

            foreach (var p in party)
            {
                p.FaceTowards(jumpAnt.transform.position);
                p.animstate = (int)MainManager.Animations.BattleIdle;
            }

            jumpAnt.animstate = (int)MainManager.Animations.KO;
            caveling.transform.position = new Vector3(0, -30);
            MainManager.SetCamera(baseCamPos, 0.05f);
            yield return EventControl.tenthsec;

            MainManager.FadeOut();
            yield return EventControl.halfsec;

            jumpAnt.animstate = 116;
            yield return new WaitForSeconds(1.66f);

            jumpAnt.animstate = 0;
            yield return EventControl.halfsec;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[152], true, Vector3.zero, party[0].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            foreach (var p in party)
            {
                p.animstate = (int)MainManager.Animations.Idle;
            }

            yield return EventControl.quartersec;

            jumpAnt.Emoticon(MainManager.Emoticons.DotsLong, 100);
            yield return new WaitUntil(() => jumpAnt.emoticoncooldown <= 0);


            MainManager.PlaySound("PingUp", 1.2f, 1);
            jumpAnt.overrridejump = true;
            jumpAnt.animstate = (int)MainManager.Animations.Flustered;
            jumpAnt.Jump();
            MainManager.ChangeMusic(NewMusic.JumpAntOverworld.ToString(), 0.05f);
            yield return EventControl.sec;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[153], true, Vector3.zero, party[1].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            jumpAnt.overrridejump = false;

            yield return WaitJump(jumpAnt, 2);

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[154], true, Vector3.zero, party[1].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            yield return EventControl.quartersec;

            jumpAnt.animstate = (int)MainManager.Animations.PickAction;
            yield return EventControl.halfsec;

            jumpAnt.animstate = 118; //raisehand
            yield return EventControl.sec;
            jumpAnt.animstate = 0;

            EntityControl amber = MainManager.GetEntity(75);
            Vector3 outsidePos = new Vector3(-13.55f, 4.66f, 8.27f);
            amber.transform.position = outsidePos;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[155], true, Vector3.zero, amber.transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }
            amber.LockRigid(false);
            amber.alwaysactive = true;

            yield return WaitMove(amber, new Vector3(-13f, 4.66f, 14.78f));

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[156], true, Vector3.zero, amber.transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            yield return EventControl.tenthsec;

            yield return WaitJump(jumpAnt, 2);

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[157], true, Vector3.zero, amber.transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            jumpAnt.animstate = 122;
            instance.StartCoroutine(AmberExit(amber, outsidePos));
            yield return EventControl.sec;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[158], true, Vector3.zero, party[2].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }
            Vector3 basePos = jumpAnt.transform.position;

            yield return EventControl.quartersec;
            MainManager.PlaySound("TextBack", 1.2f, 1);
            jumpAnt.animstate = (int)MainManager.Animations.Happy;
            yield return EventControl.sec;

            Vector3 itemOffset = new Vector3(0, 2.5f, -0.1f);
            MainManager.PlaySound("ItemHold");
            party[0].animstate = (int)MainManager.Animations.ItemGet;
            SpriteRenderer permit = MainManager.NewSpriteObject(itemOffset, party[0].transform, MainManager.itemsprites[0, (int)MainManager.Items.ExplorerPermit]);
            yield return EventControl.sec;

            party[0].animstate = (int)MainManager.Animations.TossItem;
            MainManager.PlaySound("Toss", 0.9f, 1);

            Vector3 targetPos = jumpAnt.transform.position + itemOffset + Vector3.up;
            yield return MainManager.ArcMovement(permit.gameObject, permit.transform.position, targetPos, Vector3.zero, 5, 30, false);
            party[0].animstate = (int)MainManager.Animations.Idle;

            permit.transform.position = jumpAnt.transform.position + itemOffset + Vector3.up;
            jumpAnt.animstate = (int)MainManager.Animations.ItemGet;
            yield return EventControl.sec;
            UnityEngine.Object.Destroy(permit.gameObject);

            jumpAnt.overrridejump = true;
            jumpAnt.animstate = 124;
            MainManager.PlaySound("Charge8", 1.2f, 1);
            jumpAnt.Jump();
            yield return EventControl.tenthsec;
            yield return new WaitUntil(() => jumpAnt.onground);

            jumpAnt.animstate = 115;
            MainManager.PlaySound("AtkSuccess", 1.2f, 1);
            yield return EventControl.sec;

            targetPos = new Vector3(-17.67f, 4.76f, 18.70f);
            MainManager.SetCamera(targetPos, 0.05f);
            yield return WaitMove(jumpAnt, targetPos);

            jumpAnt.LockRigid(true);
            yield return JumpAntAI.DoGroundPound(jumpAnt, jumpAnt.transform.position + Vector3.down);
            MainManager.PlaySound("DigPop", 1.2f, 1);
            jumpAnt.anim.enabled = true;

            MainManager.ShakeScreen(0.2f, 0.5f);

            Transform panel = MainManager.map.transform.Find("associationheadquarters/CarpetPanel(Clone)");
            Vector3 panelAngle = panel.localEulerAngles;

            instance.StartCoroutine(PanelFlip(panel, 3, new Vector3(20, 0), 30));

            var mushroom = EntityControl.CreateItem(panel.transform.position, 0, (int)NewItem.HeartShroom, Vector3.zero, -1);
            yield return null;

            mushroom.entity.LockRigid(true);
            MainManager.PlaySound("Damage2");

            targetPos = new Vector3(-17.32f, 4.76f, 17f);
            instance.StartCoroutine(MainManager.ArcMovement(mushroom.gameObject, mushroom.transform.position, targetPos,
                Vector3.zero, 3, 30, false));

            yield return EventControl.tenthsec;

            yield return WaitJump(jumpAnt);

            jumpAnt.animstate = 0;
            yield return EventControl.halfsec;

            Physics.IgnoreCollision(mushroom.entity.ccol, jumpAnt.ccol);
            yield return WaitMove(jumpAnt, mushroom.transform.position);

            jumpAnt.animstate = (int)MainManager.Animations.ItemGet;
            mushroom.transform.position = jumpAnt.transform.position + itemOffset;
            MainManager.PlaySound("ItemHold");
            yield return EventControl.sec;

            mushroom.gameObject.SetActive(false);
            MainManager.SetCamera(baseCamPos, 0.05f);
            yield return WaitMove(jumpAnt, basePos, lookAfter: party[0].transform.position);

            mushroom.transform.position = jumpAnt.transform.position + itemOffset;
            mushroom.gameObject.SetActive(true);
            jumpAnt.animstate = (int)MainManager.Animations.TossItem;

            MainManager.PlaySound("Toss", 0.9f, 1);
            targetPos = party[0].transform.position + Vector3.up * 2;
            yield return MainManager.ArcMovement(mushroom.gameObject, mushroom.transform.position, targetPos, Vector3.zero, 5, 30, true);
            jumpAnt.animstate = (int)MainManager.Animations.Idle;

            instance.GiveItem(1, (int)NewItem.HeartShroom, -4);
            while (MainManager.instance.message)
            {
                yield return null;
            }

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[159], true, Vector3.zero, party[1].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            jumpAnt.animstate = (int)MainManager.Animations.Upset;
            yield return EventControl.sec;

            jumpAnt.animstate = (int)MainManager.Animations.Idle;

            EntityControl[] partners = MainManager.GetEntities(new int[] { 74, 76, 77, 78, 79, 80, 81, 82 });
            targetPos = jumpAnt.transform.position + new Vector3(-1, 0, 1);

            for (int i = 0; i < partners.Length; i++)
            {
                Vector3 partnerPos = i == 0 ? cavelingPos : targetPos + new Vector3(i * 0.5f, 0, 0.1f * i);
                instance.StartCoroutine(GetOutPartner(partners[i], jumpAnt, partnerPos));
                yield return EventControl.quartersec;
            }

            MainManager.PlaySound("TextBack", 1.2f, 1);
            jumpAnt.animstate = (int)MainManager.Animations.Happy;
            yield return EventControl.sec;

            jumpAnt.animstate = 118;
            yield return EventControl.sec;

            foreach (var partner in partners)
            {
                foreach (var p in partners)
                    Physics.IgnoreCollision(partner.ccol, p.ccol);

                foreach (var p in party)
                    Physics.IgnoreCollision(partner.ccol, p.ccol);
            }

            path = new Vector3[] {
                jumpAnt.transform.position +Vector3.left*2,
                outsidePos,
                new Vector3(-2.11f, 0, 6.38f),
                new Vector3(15.96f, 0, 0f)
            };

            instance.StartCoroutine(DoExitPath(jumpAnt, path));
            yield return EventControl.quartersec;
            foreach (var p in partners)
            {
                instance.StartCoroutine(DoExitPath(p, path));
                yield return EventControl.quartersec;
            }
            yield return new WaitUntil(() => partners.All(p => p == null));

            MainManager.CompleteQuest((int)NewQuest.ExplorerExam);
            MainManager.AddPrizeMedal((int)NewPrizeFlag.JumpAnt);

            MainManager.ResetCamera();
            MainManager.ChangeMusic(MainManager.Musics.Inside0.ToString());
            MainManager.instance.flags[980] = false;
            MainManager.instance.flags[981] = true;
            yield return EventControl.halfsec;
        }

        IEnumerator PanelFlip(Transform panel, float height, Vector3 spin, float frameTime)
        {
            Vector3 baseAngle = panel.localEulerAngles;
            yield return MainManager.ArcMovement(panel.gameObject, panel.position, panel.position, spin, height, frameTime, false);
            panel.localEulerAngles = baseAngle;
        }

        IEnumerator AmberExit(EntityControl amber, Vector3 pos)
        {
            yield return WaitMove(amber, pos);
            UnityEngine.Object.Destroy(amber.gameObject);
        }

        IEnumerator DoExitPath(EntityControl entity, Vector3[] path)
        {
            foreach (var p in path)
            {
                yield return WaitMove(entity, p);
            }
            UnityEngine.Object.Destroy(entity.gameObject);
        }

        IEnumerator GetOutPartner(EntityControl entity, EntityControl jumpAnt, Vector3 targetPos)
        {
            entity.LockRigid(true);
            entity.startscale = Vector3.zero;
            entity.transform.position = jumpAnt.transform.position;
            yield return null;

            MainManager.PlaySound("Switch", 1.2f);
            yield return JumpAntFight.MovePartner(entity, targetPos, new Vector3(0, 20), Vector3.zero, Vector3.one, 30);
            entity.transform.localEulerAngles = Vector3.zero;
            entity.flip = false;
            entity.startscale = Vector3.one;
            entity.transform.position = new Vector3(entity.transform.position.x, 4.66f, entity.transform.position.z);
        }
    }
}




