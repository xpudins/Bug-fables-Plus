using System.Collections;
using System.Linq;
using UnityEngine;

namespace BFPlus.Extensions.Events
{
    public class FoodThievesEvent : NewEvent
    {
        protected override IEnumerator DoEvent(NPCControl caller, EventControl instance)
        {
            while (MainManager.instance.message)
                yield return null;
            EntityControl[] party = MainManager.GetPartyEntities(true);
            caller.entity.flip = true;

            MainManager.DialogueText(MainManager.map.dialogues[44], caller.transform, caller);
            while (MainManager.instance.message)
                yield return null;
            MainManager.PlaySound("longLegsGrow");
            MainManager.PlaySound("FlipNoise3");
            yield return EventControl.quartersec;

            foreach (var p in party)
                p.Emoticon(MainManager.Emoticons.Exclamation);
            yield return EventControl.quartersec;

            foreach (var p in party)
                p.animstate = (int)MainManager.Animations.BattleIdle;
            caller.entity.animstate = (int)MainManager.Animations.BattleIdle;
            caller.entity.flip = false;

            MainManager.ChangeMusic(MainManager.Musics.Tension.ToString());

            yield return EventControl.quartersec;
            MainManager.DialogueText(MainManager.map.dialogues[45], caller.transform, caller);
            while (MainManager.instance.message)
                yield return null;

            Vector3 camPos = caller.transform.position + new Vector3(-1, 0, -1);
            MainManager.SetCamera(camPos);

            EntityControl[] enemies = new EntityControl[4];
            for (int i = 0; i < enemies.Length; i++)
            {
                int animid = i % 2 == 0 ? (int)MainManager.AnimIDs.JumpingSpider - 1 : (int)NewAnimID.Caveling;

                Vector3 basePosition = new Vector3(-13.67f, 0, 1.63f);
                Vector3 offset = new Vector3(-0.2f * i, 0, i % 2 == 0 ? 0 : 0.1f);

                enemies[i] = EntityControl.CreateNewEntity("foodThief" + i, animid, basePosition + offset);
                enemies[i].transform.parent = MainManager.map.transform;
            }

            yield return null;
            foreach (var e in enemies)
            {
                foreach (var i in enemies)
                    Physics.IgnoreCollision(e.ccol, i.ccol);
            }

            Vector3[] posArray = new Vector3[]
            {
                new Vector3(-0.27f, 0, 1.20f),
                new Vector3(-2.24f, 0, 2),
                new Vector3(-2.29f, 0, 0.94f),
                new Vector3(0f, 0, 0f)
            };

            for (int i = 0; i < enemies.Length; i++)
            {
                enemies[i].MoveTowards(posArray[i], 1.5f);
            }

            yield return new WaitUntil(() => enemies.All(e => !e.forcemove));

            MainManager.DialogueText(MainManager.map.dialogues[46], party[0].transform, caller);
            while (MainManager.instance.message)
                yield return null;

            MainManager.instance.StartCoroutine(BattleControl.StartBattle(new int[]
            {
                (int)MainManager.Enemies.JumpingSpider, (int)NewEnemies.Caveling,
                (int)MainManager.Enemies.JumpingSpider, (int)NewEnemies.Caveling,
                (int)MainManager.Enemies.JumpingSpider, (int)NewEnemies.Caveling,
                (int)MainManager.Enemies.JumpingSpider, (int)NewEnemies.Caveling
            }, (int)MainManager.BattleMaps.FarGrasslands, -1, NewMusic.EventBattle.ToString(), null, false));
            yield return EventControl.sec;

            while (MainManager.battle != null)
                yield return null;

            MainManager.SetCameraInstant(camPos);
            yield return EventControl.halfsec;
            MainManager.FadeOut();

            foreach (var e in enemies)
            {
                e.animstate = (int)MainManager.Animations.Hurt;
                e.destroytype = NPCControl.DeathType.SpinSmoke;
                instance.StartCoroutine(e.Death());
            }
            yield return EventControl.halfsec;
            MainManager.FadeOut(0.05f);

            yield return EventControl.halfsec;
            MainManager.SetCamera(caller.transform.position);
            caller.entity.flip = true;
            yield return EventControl.quartersec;

            MainManager.DialogueText(MainManager.map.dialogues[47], party[2].transform, caller);
            while (MainManager.instance.message)
                yield return null;

            MainManager.CompleteQuest((int)NewQuest.FoodThieves);
            MainManager.instance.flags[984] = false;
            MainManager.instance.flags[985] = true;
            MainManager.ResetCamera();
            MainManager.ChangeMusic();
            yield return null;
        }
    }
}
