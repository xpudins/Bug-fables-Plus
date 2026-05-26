using System.Collections;
using UnityEngine;

namespace BFPlus.Extensions.Events.JumpAntEvents
{
    public class JumpAntCardBattle : NewEvent
    {
        protected override IEnumerator DoEvent(NPCControl caller, EventControl instance)
        {
            while (MainManager.instance.message)
            {
                yield return null;
            }
            yield return EventControl.tenthsec;

            int[] jumpAntDeck = new int[]
            {
                (int)NewCards.Mars, (int)NewCards.JumpAnt, (int)NewCards.Caveling,
                (int)NewCards.Frostfly, (int)NewCards.MechaJaw, (int)NewCards.Moeruki,
                (int)NewCards.SplotchSpider, (int)NewCards.Mothmite, (int)NewCards.Voltshroom,
                (int)NewCards.PirahnaChomp, (int)NewCards.MarsBud, (int)NewCards.WormSwarm,
                (int)NewCards.Patton, (int)NewCards.MarsBud, (int)NewCards.FireAnt
            };

            EntityControl jumpAnt = caller.entity;
            EntityControl[] party = MainManager.GetPartyEntities(true);

            MainManager.SetCamera(jumpAnt.transform.position);

            if (!MainManager.instance.flags[982])
            {
                MainManager.PlaySound("PingUp", 1.2f, 1);
                jumpAnt.animstate = (int)MainManager.Animations.Flustered;
                yield return EventControl.sec;
            }

            jumpAnt.animstate = (int)MainManager.Animations.Angry;
            party[2].animstate = 102;
            yield return EventControl.sec;

            CardGame.StartGame((int)NewAnimID.JumpAnt + 1, (int)MainManager.BattleMaps.Theater, jumpAntDeck);
            yield return new WaitForSeconds(5f);
            while (MainManager.instance.cardgame != null)
            {
                yield return null;
            }
            MainManager.SetCamera(jumpAnt.transform.position);
            if (MainManager.battleresult)
            {
                jumpAnt.animstate = (int)MainManager.Animations.Woobly;

                MainManager.FadeOut(0.05f);
                yield return EventControl.halfsec;

                MainManager.ChangeMusic(NewMusic.JumpAntOverworld.ToString(), 0.05f);
                yield return EventControl.halfsec;

                yield return WaitJump(jumpAnt, 2);

                MainManager.PlaySound("TextBack", 1.2f, 1);
                jumpAnt.animstate = (int)MainManager.Animations.Happy;
                yield return EventControl.sec;

                foreach (var p in party)
                    p.animstate = (int)MainManager.Animations.Happy;

                party[2].animstate = (int)MainManager.Animations.BattleIdle;
                yield return EventControl.halfsec;

                if (!MainManager.instance.flags[982])
                {
                    Vector3 itemOffset = new Vector3(0, 2.5f, -0.1f);
                    MainManager.PlaySound("ItemHold");
                    jumpAnt.animstate = (int)MainManager.Animations.ItemGet;
                    SpriteRenderer stylishShroom = MainManager.NewSpriteObject(itemOffset, jumpAnt.transform, MainManager.itemsprites[1, (int)Medal.StylishShroom]);
                    yield return EventControl.sec;
                    jumpAnt.animstate = (int)MainManager.Animations.Idle;
                    UnityEngine.Object.Destroy(stylishShroom.gameObject);

                    instance.GiveItem(2, (int)Medal.StylishShroom);
                    while (MainManager.instance.message)
                    {
                        yield return null;
                    }

                    yield return WaitJump(jumpAnt, 2);
                    MainManager.CompleteQuest((int)NewQuest.AnAdventureWithJumpAnt);
                    MainManager.instance.flags[982] = true;
                }
            }
            else
            {
                party[2].animstate = (int)MainManager.Animations.WeakBattleIdle;
                MainManager.FadeOut(0.05f);
                yield return EventControl.sec;
            }

            jumpAnt.animstate = 117;
            MainManager.ResetCamera();
            MainManager.ChangeMusic();
            yield return EventControl.halfsec;
        }
    }
}
