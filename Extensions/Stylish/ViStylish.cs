using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static MainManager;
using static BFPlus.Extensions.BattleControl_Ext;
namespace BFPlus.Extensions.Stylish
{
    public class ViStylish : IStylish
    {
        IEnumerator DoStylish_SimpleSpin(float gain, bool delayed)
        {
            EntityControl vi = !delayed ? Instance.entityAttacking :
                instance.playerdata[Instance.inPlayerDelayedProjs].battleentity;
            StylishUtils.ShowStylish(1.3f, vi, gain);
            vi.spin = new Vector3(0, 20, 0);
            yield return EventControl.quartersec;
            vi.spin = Vector3.zero;
        }
        
        IEnumerator DoBasicAttackStylish(float gain, int stylishAnim)
        {
            yield return null;
            EntityControl vi = Instance.entityAttacking;
            vi.overrideflip = false;
            vi.flip = true;
            vi.spin = new Vector3(0, 20, 0);
            vi.animstate = stylishAnim;

            StylishUtils.ShowStylish(1.2f, vi, gain);
            yield return EventControl.halfsec;
            vi.spin = Vector3.zero;
            yield return EventControl.quartersec;
        }
        IEnumerator DoTornadoTossHitStylish(float gain)
        {
            EntityControl vi = Instance.entityAttacking;
            StylishUtils.ShowStylish(1.3f, vi, gain);
            vi.flip = !vi.flip;
            vi.animstate = 109;
            yield return EventControl.quartersec;
        }
        IEnumerator DoHeavyThrowStylish(float gain)
        {
            EntityControl vi = Instance.entityAttacking;
            StylishUtils.ShowStylish(1.2f, vi, gain);
            vi.animstate = 111;
            yield return EventControl.halfsec;
        }
        IEnumerator DoNeedleTossStylish(float gain)
        {
            EntityControl vi = Instance.entityAttacking;
            StylishUtils.ShowStylish(1.2f, vi, gain);
            vi.animstate = 107;
            vi.spin = new Vector3(0, 20, 0);
            yield return EventControl.quartersec;
            vi.spin = Vector3.zero;
        }
        IEnumerator DoNeedlePincerStylish(float gain)
        {
            BattleControl.SetDefaultCamera();
            EntityControl vi = Instance.entityAttacking;
            StylishUtils.ShowStylish(1.2f, vi, gain);
            vi.animstate = 111;
            vi.spin = new Vector3(0, 30, 0);

            Vector3[] partyPos = MainManager.battle.partypos;
            int viIndex = Array.FindIndex(MainManager.battle.partypointer, x => x == MainManager.battle.currentturn);
            Vector3 endPos = partyPos[viIndex];
            Vector3 startPos = vi.transform.position;
            float a = 0;
            float b = 35f;
            do
            {
                vi.transform.position = Vector3.Lerp(startPos, endPos, a / b);
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < b);

            yield return EventControl.quartersec;
            vi.spin = Vector3.zero;
        }
        IEnumerator DoStashStylish(float gain)
        {
            EntityControl vi = Instance.entityAttacking;
            StylishUtils.ShowStylish(1.2f, vi, gain);
            vi.animstate = (int)MainManager.Animations.Happy;
            yield return EventControl.halfsec;
        }
        IEnumerator DoNeedleSurgeStylish_Final(float gain)
        {
            yield return null;
            EntityControl vi = Instance.entityAttacking;
            vi.spin = new Vector3(0, 20, 0);
            vi.animstate = 111;
            EntityControl actualVi = null;
            if (actionID == (int)NewSkill.NeedleSurge && instance.playerdata[vi.battleid].trueid != 0)
            {
                for (int i = 0; i < instance.playerdata.Length; i++)
                {
                    if (instance.playerdata[i].trueid != 0)
                    {
                        continue;
                    }
                    actualVi = instance.playerdata[i].battleentity;
                    actualVi.spin = new Vector3(0, -20, 0);
                    actualVi.animstate = 111;
                    battle.StartCoroutine(Instance.IncreaseStylishBar(gain, actualVi));
                    break;
                }
            }

            StylishUtils.ShowStylish(1.2f, vi, gain);
            yield return EventControl.halfsec;
            vi.spin = Vector3.zero;
            if (actualVi != null)
                actualVi.spin = Vector3.zero;
            yield return EventControl.quartersec;
        }

        public IEnumerator DoStylish(int actionid, int stylishID, float stylishGain)
        {
            Instance.entityAttacking?.Emoticon(Emoticons.None);
            switch (actionid)
            {
                case -1: // basic attack
                    yield return DoBasicAttackStylish(stylishGain, 111);
                    break;

                case -2: // falling needles from needle toss
                    yield return DoStylish_SimpleSpin(stylishGain, true);
                    break;

                case (int)Skills.BeeRangMultiHit:
                    switch (stylishID)
                    {
                        case 0: // final hit
                            yield return DoBasicAttackStylish(stylishGain, 111);
                            break;
                        case 1: // tornado hits
                            yield return DoTornadoTossHitStylish(stylishGain);
                            break;
                    }
                    break;
                case (int)Skills.HurricaneBeemerang:
                    yield return DoBasicAttackStylish(stylishGain, 111);
                    break;

                case (int)Skills.NeedleToss:
                    yield return DoNeedleTossStylish(stylishGain);
                    break;
                case (int)Skills.NeedlePincer:
                    switch (stylishID)
                    {
                        case 0: // preceding hits
                            yield return DoStylish_SimpleSpin(stylishGain, false);
                            break;
                        case 1: // final hit
                            yield return DoNeedlePincerStylish(stylishGain);
                            break;
                    }
                    break;
                case (int)NewSkill.NeedleSurge:
                    switch (stylishID)
                    {
                        case 0: // when user pulls out needles
                            yield return DoStylish_SimpleSpin(stylishGain, false);
                            break;
                        case 1: // post-surge
                            yield return DoNeedleSurgeStylish_Final(stylishGain);
                            break;
                    }
                    break;

                case (int)Skills.SecretStash:
                case (int)Skills.SharingStash:
                    yield return DoStashStylish(stylishGain);
                    break;

                case (int)Skills.HeavyThrow:
                    yield return DoHeavyThrowStylish(stylishGain);
                    break;

                case (int)NewSkill.Steal:
                    switch (stylishID)
                    {
                        case 1: // final hit
                            yield return DoBasicAttackStylish(stylishGain, 109);
                            break;
                        case 0: // preceding hits
                            yield return DoTornadoTossHitStylish(stylishGain);
                            break;
                    }
                    break;
            }
        }
    }
}
