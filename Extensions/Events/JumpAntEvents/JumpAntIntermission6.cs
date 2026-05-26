using System.Collections;
using UnityEngine;

namespace BFPlus.Extensions.Events.JumpAntEvents
{
    public class JumpAntIntermission6 : NewEvent
    {
        protected override IEnumerator DoEvent(NPCControl caller, EventControl instance)
        {
            while (MainManager.instance.message)
                yield return null;

            EntityControl[] party = MainManager.GetPartyEntities(true);
            EntityControl jumpAnt = MainManager.GetEntity(31);

            MainManager.ChangeMusic(NewMusic.JumpAntOverworld.ToString(), 0.05f);

            int dialogueId = 14;
            bool wakeUp = true;
            Vector3 targetPos = jumpAnt.transform.position + Vector3.up * 0.75f;

            int[] amounthits = new int[] { 2, 3, 3 };
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < amounthits[i]; j++)
                {
                    yield return ThrowBeemerang(party[0], targetPos);

                    MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[dialogueId], true, Vector3.zero, null, caller));
                    while (MainManager.instance.message)
                    {
                        yield return null;
                    }

                    if (MainManager.instance.option != 0)
                    {
                        wakeUp = false;
                        break;
                    }
                }

                if (!wakeUp)
                    break;

                dialogueId++;
            }

            if (wakeUp)
            {
                yield return ThrowBeemerang(party[0], targetPos);

                MainManager.SetCamera(null, jumpAnt.transform.position + new Vector3(0, -0.5f, 1), 0.05f, MainManager.defaultcamoffset + new Vector3(-2, 0, 0), MainManager.defaultcamangle);

                for (int i = 0; i < 6; i++)
                {
                    jumpAnt.animstate = i % 2 == 0 ? 126 : 102;
                    yield return EventControl.quartersec;
                }

                jumpAnt.flip = true;
                yield return WaitJump(jumpAnt, 1, 10);

                jumpAnt.animstate = 0;
                yield return EventControl.quartersec;

                jumpAnt.animstate = 118;
                yield return EventControl.sec;

                MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[17], true, Vector3.zero, party[0].transform, caller));
                while (MainManager.instance.message)
                {
                    yield return null;
                }

                MainManager.PlaySound("Roar", 1f, 1);
                MainManager.FadeMusic(0.1f);
                yield return EventControl.halfsec;
                foreach (var p in party)
                {
                    p.animstate = (int)MainManager.Animations.BattleIdle;
                    p.flip = true;
                }
                jumpAnt.animstate = (int)MainManager.Animations.Angry;
                EntityControl belostoss = EntityControl.CreateNewEntity("belostoss", (int)MainManager.AnimIDs.ToeBiter - 1, new Vector3(12.20f, 4f, 9f));
                belostoss.transform.parent = MainManager.map.transform;
                yield return null;

                var paths = new Vector3[] { new Vector3(-8.8f, 4, 7.96f), new Vector3(-12f, 4, 2f), };

                foreach (var path in paths)
                {
                    belostoss.forcejump = true;
                    belostoss.MoveTowards(path, 2);
                    yield return new WaitUntil(() => !belostoss.forcemove);
                }

                targetPos = new Vector3(-10.39f, 0, -0.91f);
                belostoss.PlaySound("Jump");
                belostoss.LockRigid(true);
                belostoss.animstate = (int)MainManager.Animations.ItemGet;
                yield return MainManager.ArcMovement(belostoss.gameObject, belostoss.transform.position, targetPos, Vector3.zero, 5, 30, false);

                MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[18], true, Vector3.zero, party[1].transform, caller));
                while (MainManager.instance.message)
                {
                    yield return null;
                }

                jumpAnt.animstate = 104;
                yield return EventControl.sec;

                Sprite hammerSprite = MainManager_Ext.assetBundle.LoadAssetWithSubAssets<Sprite>("JumpAnt")[55];
                SpriteRenderer hammer = MainManager.NewSpriteObject(jumpAnt.transform.position + new Vector3(0, 1, -0.1f), MainManager.map.transform, hammerSprite);
                targetPos = belostoss.transform.position + new Vector3(0, 1, -0.1f);

                jumpAnt.animstate = (int)MainManager.Animations.TossItem;
                MainManager.PlaySound("Toss10", 0.9f, 1);
                yield return MainManager.ArcMovement(hammer.gameObject, hammer.transform.position, targetPos, new Vector3(0, 0, 20), 10, 30, true);

                MainManager.PlaySound("Damage0");
                MainManager.HitPart(targetPos);
                belostoss.animstate = (int)MainManager.Animations.Hurt;
                yield return EventControl.tenthsec;

                MainManager.instance.StartCoroutine(BattleControl.StartBattle(
                new int[] { (int)MainManager.Enemies.ToeBiter, (int)NewEnemies.LonglegsSpider, (int)NewEnemies.Spineling, (int)NewEnemies.WormSwarm },
                (int)MainManager.BattleMaps.BarrenLands, -1, NewMusic.EventBattle.ToString(), null, false));

                yield return EventControl.sec;
                while (MainManager.battle != null)
                {
                    yield return null;
                }
                yield return EventControl.halfsec;

                Vector3 camPos = jumpAnt.transform.position + new Vector3(1, -0.5f, 0);
                MainManager.SetCamera(null, camPos, 1f, MainManager.defaultcamoffset + new Vector3(-1, 0, 0), MainManager.defaultcamangle);

                belostoss.destroytype = NPCControl.DeathType.SpinSmoke;
                belostoss.animstate = (int)MainManager.Animations.Hurt;
                jumpAnt.animstate = (int)MainManager.Animations.Idle;
                foreach (var p in party)
                    p.FaceTowards(jumpAnt.transform.position);
                yield return EventControl.halfsec;
                MainManager.FadeOut();

                instance.StartCoroutine(belostoss.Death());
                yield return EventControl.sec;

                MainManager.ChangeMusic(NewMusic.JumpAntOverworld.ToString(), 0.05f);

                MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[19], true, Vector3.zero, party[0].transform, caller));
                while (MainManager.instance.message)
                {
                    yield return null;
                }

                EntityControl venus = MainManager.GetEntity(1);
                yield return EventControl.quartersec;

                MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[20], true, Vector3.zero, venus.transform, caller));
                while (MainManager.instance.message)
                {
                    yield return null;
                }
                yield return EventControl.quartersec;

                MainManager.PlaySound("Lost", 0.9f, 1);
                jumpAnt.Emoticon(MainManager.Emoticons.QuestionMark, 60);
                yield return EventControl.sec;

                foreach (var p in party)
                    Physics.IgnoreCollision(jumpAnt.ccol, p.ccol);
                Physics.IgnoreCollision(jumpAnt.ccol, venus.ccol);

                jumpAnt.MoveTowards(venus.transform.position + new Vector3(-1f, 0, -0.1f));
                yield return new WaitUntil(() => !jumpAnt.forcemove);
                jumpAnt.animstate = 0;

                MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[21], true, Vector3.zero, venus.transform, caller));
                while (MainManager.instance.message)
                {
                    yield return null;
                }

                jumpAnt.animstate = 127;
                venus.Emoticon(MainManager.Emoticons.Exclamation);
                party[1].animstate = 105;
                party[0].animstate = (int)MainManager.Animations.Surprized;
                yield return EventControl.halfsec;

                MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[22], true, Vector3.zero, venus.transform, caller));
                while (MainManager.instance.message)
                {
                    yield return null;
                }

                yield return jumpAnt.ShakeSprite(0.1f, 60);
                jumpAnt.animstate = 128;
                yield return EventControl.tenthsec;

                venus.spin = new Vector3(0f, -20f);
                MainManager.PlaySound("Rumble3");
                MainManager.ShakeScreen(0.7f);
                yield return new WaitForSeconds(0.7f);
                MainManager.PlayParticle("DirtExplode", venus.transform.position);
                venus.spin = new Vector3(0f, -30f);
                float a = 0f;
                float b = 40f;
                venus.spinextra[0] = new Vector3(0f, -20f);
                MainManager.PlaySound("Charge6");
                MainManager.SetCamera(null, camPos + Vector3.up * 1, 0.05f, MainManager.defaultcamoffset + new Vector3(-1, 0, 0), MainManager.defaultcamangle);
                do
                {
                    venus.height = MainManager.BeizierCurve(venus.transform.position, venus.transform.position + Vector3.up * 3f, 5f, a / b).y;
                    a += MainManager.TieFramerate(1f);
                    yield return null;
                }
                while (a < b);
                venus.bobrange = venus.startbf;
                venus.bobspeed = venus.startbs;
                venus.spin = Vector3.zero;
                yield return EventControl.halfsec;

                jumpAnt.animstate = 115;
                MainManager.PlaySound("AtkSuccess", 1.2f, 1);
                yield return EventControl.sec;

                MainManager.SetCamera(null, camPos, 0.05f, MainManager.defaultcamoffset + new Vector3(-1, 0, 0), MainManager.defaultcamangle);
                MainManager.PlaySound("ChargeDown");
                venus.animstate = 101;
                venus.spin = new Vector3(0f, -20f);
                a = 0f;
                b = 30;
                float startHeight = venus.height;
                do
                {
                    venus.height = Mathf.Lerp(startHeight, 0f, a / b);
                    a += MainManager.TieFramerate(1f);
                    yield return null;
                }
                while (a < b);
                venus.height = 0f;
                venus.animstate = 0;
                venus.spin = Vector3.zero;
                venus.FlipAngle(true);

                MainManager.PlaySound("Wam");

                MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[23], true, Vector3.zero, venus.transform, caller));
                while (MainManager.instance.message)
                {
                    yield return null;
                }

                jumpAnt.LockRigid(true);
                targetPos = new Vector3(-16f, 0, -0.5f);
                jumpAnt.PlaySound("Jump");
                jumpAnt.animstate = (int)MainManager.Animations.Flustered;
                yield return MainManager.ArcMovement(jumpAnt.gameObject, jumpAnt.transform.position, targetPos, Vector3.zero, 5, 30, false);

                jumpAnt.LockRigid(false);
                MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[24], true, Vector3.zero, venus.transform, caller));
                while (MainManager.instance.message)
                {
                    yield return null;
                }
                yield return EventControl.halfsec;

                jumpAnt.animstate = (int)MainManager.Animations.Sit;
                yield return EventControl.sec;

                jumpAnt.animstate = 118;
                yield return EventControl.sec;

                foreach (var p in party)
                    p.FaceTowards(jumpAnt.transform.position);

                party[0].animstate = (int)MainManager.Animations.Sad;
                party[1].animstate = 0;

                MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[25], true, Vector3.zero, party[0].transform, caller));
                while (MainManager.instance.message)
                {
                    yield return null;
                }

                party[2].animstate = 0;
                foreach (var p in party)
                    p.Emoticon(MainManager.Emoticons.DotsLong, 100);
                venus.Emoticon(MainManager.Emoticons.DotsLong, 100);
                yield return new WaitUntil(() => party[0].emoticoncooldown <= 0);

                MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[26], true, Vector3.zero, venus.transform, caller));
                while (MainManager.instance.message)
                {
                    yield return null;
                }
                yield return EventControl.quartersec;

                MainManager.PlaySound("TextBack", 1.2f, 1);
                jumpAnt.animstate = (int)MainManager.Animations.Happy;
                yield return EventControl.sec;

                jumpAnt.animstate = 118;
                yield return EventControl.sec;

                foreach (var p in party)
                    p.FaceTowards(venus.transform.position);

                jumpAnt.MoveTowards(venus.transform.position + new Vector3(-2f, 0, -0.1f));
                yield return new WaitUntil(() => !jumpAnt.forcemove);
                jumpAnt.animstate = 0;

                MainManager.PlaySound("ItemHold");
                jumpAnt.animstate = (int)MainManager.Animations.ItemGet;
                var berries = MainManager.NewSpriteObject(new Vector3(0, 3, -0.1f), jumpAnt.transform, MainManager.itemsprites[0, (int)MainManager.Items.MoneyBig]);
                yield return EventControl.sec;
                UnityEngine.Object.Destroy(berries.gameObject);

                yield return JumpAntPDash(jumpAnt, party, new Vector3(-15.79f, 0, -6.4654f), 2);

                MainManager.FadeMusic(0.05f);

                MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[27], true, Vector3.zero, party[2].transform, caller));
                while (MainManager.instance.message)
                {
                    yield return null;
                }
                MainManager.ResetCamera();
                MainManager.ChangeMusic();

                MainManager.instance.flags[978] = true;
                MainManager.instance.flags[979] = false;
                MainManager.UpdateJounal();
            }
            else
            {
                MainManager.ChangeMusic();
                MainManager.ResetCamera();
            }

        }

        IEnumerator ThrowBeemerang(EntityControl vi, Vector3 targetPos)
        {
            vi.animstate = 103;
            yield return new WaitForSeconds(0.17f);
            vi.animstate = 104;
            yield return new WaitForSeconds(0.5f);
            vi.animstate = 105;
            yield return new WaitForSeconds(0.05f);
            MainManager.PlaySound("Woosh", 8, 1.1f, 1f, true);
            GameObject beemerang = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("Prefabs/Objects/BeerangBattle") as GameObject);
            beemerang.transform.position = vi.transform.position + Vector3.up;
            Vector3 start = beemerang.transform.position + new Vector3(0.5f, 0.5f);
            Vector3 target = targetPos;
            Vector3 mid = Vector3.Lerp(start, target, 0.5f) + new Vector3(0f, 0f, -5f);
            float a = 0f;
            do
            {
                a += MainManager.framestep;
                beemerang.transform.position = MainManager.BeizierCurve3(start, target, mid, Mathf.Clamp01(a / 20f));
                beemerang.transform.localEulerAngles = new Vector3(80f, 0f, beemerang.transform.localEulerAngles.z - MainManager.framestep * 20f);
                if (a >= 20f)
                {
                    MainManager.PlaySound("Damage0");
                    MainManager.HitPart(targetPos + Vector3.up);
                    break;
                }
                yield return null;
            }
            while (a < 40f);
            UnityEngine.Object.Destroy(beemerang);
            MainManager.StopSound(8, 0.1f);
            yield return EventControl.quartersec;
            vi.animstate = (int)MainManager.Animations.BattleIdle;
        }


    }
}




