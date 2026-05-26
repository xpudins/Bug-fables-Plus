using System.Collections;
using UnityEngine;

namespace BFPlus.Extensions.Events.JumpAntEvents
{
    public class JumpAntIntermission4End : NewEvent
    {
        protected override IEnumerator DoEvent(NPCControl caller, EventControl instance)
        {
            while (MainManager.instance.message)
                yield return null;

            EntityControl[] party = MainManager.GetPartyEntities(true);
            EntityControl jumpAnt = MainManager.GetExtraFollower((int)NewAnimID.JumpAnt);
            EntityControl chompy = MainManager.map.chompy;
            jumpAnt.overrideanim = false;
            jumpAnt.overrridejump = false;

            Transform chest = MainManager.map.mainmesh.Find("Chest(Clone)");
            Collider chestCcol = chest.gameObject.GetComponent<Collider>();
            chestCcol.enabled = false;

            Vector3[] posArray = new Vector3[]
             {
                new Vector3(22f, 0, 0),
                new Vector3(20.5f, 0, 0),
                new Vector3(19f, 0, 0),
                new Vector3(18.5f, 0, 1)
             };
            Vector3 targetPos = new Vector3(24f, 0f, 0f);
            MainManager.events.MoveParty(posArray);
            jumpAnt.MoveTowards(targetPos, 1.5f);
            while (!MainManager.PartyIsNotMoving() || (chompy != null && chompy.forcemove) || jumpAnt.forcemove)
            {
                yield return null;
            }

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[12], true, Vector3.zero, party[0].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }
            yield return EventControl.quartersec;

            jumpAnt.FaceTowards(party[0].transform.position);
            yield return WaitJump(jumpAnt, 2);
            jumpAnt.animstate = 0;

            party[0].animstate = (int)MainManager.Animations.Happy;
            MiniBubble minibubble = MiniBubble.SetUp(MainManager.map.dialogues[17], party[0], new Vector3(0f, 1.5f, 10f), 0, 0.5f);
            while (minibubble != null)
            {
                yield return null;
            }

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[13], true, Vector3.zero, party[1].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }
            yield return EventControl.quartersec;
            MainManager.PlaySound("TextBack", 1.2f, 1);
            jumpAnt.animstate = (int)MainManager.Animations.Happy;
            yield return EventControl.sec;
            jumpAnt.animstate = 0;

            MainManager.SetCamera(chest.transform.position + new Vector3(-1, 0, 0), 0.05f);
            jumpAnt.FaceTowards(chest.position);
            party[0].animstate = (int)MainManager.Animations.Happy;
            party[1].animstate = 0;
            yield return EventControl.quartersec;

            jumpAnt.overrideanim = true;

            MainManager.PlaySound("Charge18", 1.2f, 1);
            jumpAnt.StartCoroutine(jumpAnt.ShakeSprite(0.2f, 60));
            jumpAnt.animstate = 104;
            jumpAnt.StartCoroutine(MainManager.Spin(jumpAnt.transform, new Vector3(0, 0, 15), 60, false));
            yield return EventControl.sec;

            jumpAnt.animstate = 107;
            yield return EventControl.tenthsec;

            jumpAnt.LockRigid(true);
            jumpAnt.PlaySound("Jump");
            yield return BattleControl_Ext.LerpPosition(5, jumpAnt.transform.position, jumpAnt.transform.position + Vector3.up * 2, jumpAnt.transform);

            MainManager.PlaySound("Spin", -1, 1.2f, 1, true);
            for (int i = 0; i < 4; i++)
                yield return MainManager.Spin(jumpAnt.transform, new Vector3(0, 0, -360), 15 - i * 3f, false);
            MainManager.StopSound("Spin");
            yield return EventControl.quartersec;

            jumpAnt.animstate = 109;
            jumpAnt.transform.position = new Vector3(jumpAnt.transform.position.x, 1.2f, jumpAnt.transform.position.z);
            yield return EventControl.tenthsec;

            MainManager.ShakeScreen(0.5f, 1);
            MainManager.PlaySound("Thud3", 0.8f, 1);

            foreach (var p in party)
            {
                instance.StartCoroutine(JumpFall(p));
            }
            yield return EventControl.sec;

            jumpAnt.transform.position = new Vector3(jumpAnt.transform.position.x, 0, jumpAnt.transform.position.z);
            jumpAnt.animstate = (int)MainManager.Animations.Idle;

            yield return EventControl.sec;

            foreach (var p in party)
            {
                instance.StartCoroutine(ShakeJump(p));
            }
            jumpAnt.animstate = (int)MainManager.Animations.Flustered;

            MainManager.PlaySound("Rumble3", 1.2f, 1);
            yield return MainManager.ShakeObject(chest, new Vector3(0.1f, 0f, 0.1f), 120, true);

            party[0].animstate = (int)MainManager.Animations.Surprized;
            party[1].animstate = (int)MainManager.Animations.Surprized;
            party[2].animstate = (int)MainManager.Animations.BattleIdle;

            yield return EventControl.halfsec;

            float a = 0;
            float b = 60;
            Transform chestTop = chest.GetChild(0);
            MainManager.PlaySound("BridgeRope");
            do
            {
                chestTop.transform.localEulerAngles = new Vector3(Mathf.Lerp(0, 70, a / b), 0, 0);
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < b);

            MainManager.PlaySound("b33BotLaserExplosions", 1.2f, 1);
            int wardenAmount = 10;
            EntityControl[] wardens = new EntityControl[wardenAmount];
            for (int i = 0; i < wardenAmount; i++)
            {
                wardens[i] = EntityControl.CreateNewEntity("warden" + i, (int)MainManager.AnimIDs.CursedSkull - 1, chest.transform.position);
                wardens[i].transform.position += MainManager.RandomVector(0.5f);
                wardens[i].inice = i % 2 == 0;
                wardens[i].transform.parent = MainManager.map.transform;
                yield return null;
                wardens[i].LockRigid(true);

                for (int j = 0; j < i; j++)
                {
                    Physics.IgnoreCollision(wardens[i].ccol, wardens[j].ccol);
                }

                targetPos = wardens[i].transform.position + Vector3.up * 3 + MainManager.RandomVector(new Vector3(2, 2, 2));
                instance.StartCoroutine(MainManager.ArcMovement(wardens[i].gameObject, targetPos, 5, 30));
            }

            yield return EventControl.halfsec;

            MainManager.instance.StartCoroutine(BattleControl.StartBattle(
                new int[] {
                    (int)MainManager.Enemies.IceWarden, (int)MainManager.Enemies.CursedSkull, (int)MainManager.Enemies.IceWarden,
                    (int)MainManager.Enemies.IceWarden, (int)MainManager.Enemies.CursedSkull, (int)MainManager.Enemies.IceWarden,
                    (int)MainManager.Enemies.IceWarden, (int)MainManager.Enemies.CursedSkull,(int)MainManager.Enemies.CursedSkull,
                    (int)MainManager.Enemies.CursedSkull
                },
                (int)MainManager.BattleMaps.SandCastle, -1, NewMusic.EventBattle.ToString(), null, false)
            );

            yield return EventControl.sec;
            while (MainManager.battle != null)
            {
                yield return null;
            }
            yield return EventControl.halfsec;
            jumpAnt.overrideanim = false;
            MainManager.SetCamera(jumpAnt.transform.position + new Vector3(-0.5f, 0, 0), 1);

            foreach (var warden in wardens)
            {
                warden.animstate = (int)MainManager.Animations.Hurt;
                warden.SetAnimForce();
            }

            foreach (var p in party)
                p.animstate = (int)MainManager.Animations.WeakBattleIdle;

            jumpAnt.animstate = (int)MainManager.Animations.Block;

            yield return EventControl.halfsec;
            MainManager.FadeOut();

            foreach (var warden in wardens)
            {
                warden.destroytype = NPCControl.DeathType.SpinSmoke;
                instance.StartCoroutine(warden.Death());
            }

            yield return EventControl.sec;
            yield return EventControl.halfsec;
            MainManager.ChangeMusic(NewMusic.JumpAntOverworld.ToString(), 0.01f);
            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[14], true, Vector3.zero, party[1].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }
            yield return EventControl.quartersec;

            jumpAnt.animstate = 0;
            yield return EventControl.halfsec;

            jumpAnt.animstate = 124;
            jumpAnt.LockRigid(true);
            Vector3 startPos = jumpAnt.transform.position;

            foreach (var p in party)
                p.animstate = (int)MainManager.Animations.Idle;

            MainManager.PlaySound("Charge8", 1.2f, 1);
            yield return MainManager.ArcMovement(jumpAnt.gameObject, jumpAnt.transform.position, chestTop.transform.position + new Vector3(0, 1, -1), Vector3.zero, 5, 30, false);

            MainManager.PlaySound("Block2", 1.2f, 1);
            instance.StartCoroutine(MainManager.ShakeObject(chest, new Vector3(0.1f, 0f, 0.1f), 60, true));

            yield return MainManager.ArcMovement(jumpAnt.gameObject, jumpAnt.transform.position, startPos, Vector3.zero, 5, 30, false);
            jumpAnt.LockRigid(false);

            jumpAnt.animstate = 0;
            yield return EventControl.sec;
            var mushroom = EntityControl.CreateItem(chest.transform.position, 0, (int)MainManager.Items.Mushroom, Vector3.zero, -1);
            yield return null;
            mushroom.entity.LockRigid(true);
            MainManager.PlaySound("Damage2");
            yield return MainManager.ArcMovement(mushroom.gameObject, mushroom.transform.position, new Vector3(22.5f, 0, -2), Vector3.zero, 5, 30, false);

            mushroom.entity.LockRigid(false);
            jumpAnt.FacePlayer();

            foreach (var p in party)
            {
                p.Emoticon(MainManager.Emoticons.DotsLong, 100);
            }
            yield return new WaitUntil(() => party[0].emoticoncooldown <= 0);

            party[0].animstate = (int)MainManager.Animations.Angry;
            party[1].animstate = (int)MainManager.Animations.Happy;

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[15], true, Vector3.zero, party[0].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }
            yield return EventControl.quartersec;

            jumpAnt.alwaysactive = true;
            jumpAnt.MoveTowards(mushroom.transform.position);
            yield return new WaitUntil(() => !jumpAnt.forcemove);

            MainManager.PlaySound("ItemHold");
            jumpAnt.animstate = (int)MainManager.Animations.ItemGet;
            mushroom.entity.LockRigid(true);

            mushroom.transform.position = jumpAnt.transform.position + new Vector3(0, 2.5f, -0.1f);
            yield return EventControl.halfsec;

            UnityEngine.Object.Destroy(mushroom.gameObject);


            yield return WaitJump(jumpAnt, 2);

            yield return GiveJumpAntReward(jumpAnt, instance, 20, 70);

            jumpAnt.animstate = 118;
            yield return EventControl.sec;

            yield return JumpAntPDash(jumpAnt, party, Vector3.zero);
            MainManager.SetCamera(party[1].transform.position, 0.05f);

            MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[16], true, Vector3.zero, party[2].transform, caller));
            while (MainManager.instance.message)
            {
                yield return null;
            }

            MainManager.instance.extrafollowers.Remove((int)NewAnimID.JumpAnt);
            if (MainManager.map.chompy != null)
            {
                MainManager.map.chompy.following = MainManager.instance.playerdata[MainManager.instance.playerdata.Length - 1].entity;
            }
            MainManager.ResetCamera();
            MainManager.ChangeMusic();

            //hints npc dialogue
            MainManager.instance.flags[975] = false;
            MainManager.instance.flags[974] = true;

            if (MainManager.instance.flags[359])
                MainManager.instance.flags[977] = true;

            MainManager.UpdateJounal();
            chestCcol.enabled = true;
            chest.gameObject.layer = LayerMask.NameToLayer("NoDigGround");
            yield return EventControl.halfsec;
        }

        IEnumerator JumpFall(EntityControl entity)
        {
            entity.overrideanim = true;
            entity.animstate = (int)MainManager.Animations.Hurt;
            entity.Jump(10);
            yield return EventControl.tenthsec;
            yield return new WaitUntil(() => entity.onground);
            entity.animstate = (int)MainManager.Animations.KO;
            if (MainManager.SoundIsPlaying("Death3") == -1)
                MainManager.PlaySound("Death3");
            entity.overrideanim = false;
        }

        IEnumerator ShakeJump(EntityControl entity)
        {
            entity.StartCoroutine(entity.ShakeSprite(new Vector3(0.1f, 0f), 60f));
            yield return EventControl.sec;
            if (MainManager.SoundIsPlaying("Jump") == -1)
                MainManager.PlaySound("Jump");
            entity.Jump();
        }
    }
}


