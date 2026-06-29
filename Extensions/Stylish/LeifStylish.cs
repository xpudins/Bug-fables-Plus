using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MainManager;
using static BFPlus.Extensions.BattleControl_Ext;
namespace BFPlus.Extensions.Stylish
{

    public class LeifStylish : IStylish
    {
        IEnumerator DoBasicAttackStylish(float gain)
        {
            EntityControl leif = Instance.entityAttacking;
            StylishUtils.ShowStylish(1.2f, leif, gain);
            leif.animstate = 116;
            yield return EventControl.halfsec;
        }
        IEnumerator DoIcefallStylish(float gain)
        {
            EntityControl leif = Instance.entityAttacking;
            StylishUtils.ShowStylish(1.2f, leif, gain);
            leif.animstate = 100;
            yield return EventControl.halfsec;
            leif.animstate = 102;
            leif.spin = new Vector3(0, 20, 0);

            Vector3[] partPos = new Vector3[]
            {
                leif.transform.position + Vector3.forward * 0.5f + Vector3.up*0.5f,
                leif.transform.position + Vector3.back * 0.5f + Vector3.up,
                leif.transform.position + Vector3.forward * 0.5f + Vector3.up*1.5f,
            };

            for (int i = 0; i < 3; i++)
            {
                MainManager.PlayParticle("mothicenormal", partPos[i], 1.5f);
                MainManager.PlaySound("IceMothHit", 1.1f + i * 0.1f, 0.5f);
                yield return EventControl.tenthsec;
            }
            yield return EventControl.quartersec;
            leif.spin = Vector3.zero;
        }
        IEnumerator DoFrigidFirstHitStylish(float gain)
        {
            EntityControl leif = Instance.entityAttacking;
            StylishUtils.ShowStylish(1.2f, leif, gain);
            leif.spin = new Vector3(0, 20, 0);
            yield return EventControl.thirdsec;
            leif.spin = Vector3.zero;
        }
        IEnumerator DoFrigidLastHitStylish(float gain)
        {
            EntityControl leif = Instance.entityAttacking;
            StylishUtils.ShowStylish(1.2f, leif, gain * 0.5f);
            leif.animstate = 111;

            float a = 0;
            float b = 60;
            bool failedStylish = false;
            bool inLeifSpin = false;
            float rotationSpeed = 20f;
            float currentSpeed;

            Vector3 startAngle = leif.spritetransform.eulerAngles;
            leif.overrideonlyflip = true;
            do
            {
                currentSpeed = Mathf.Lerp(rotationSpeed, 0, a / b);
                leif.spritetransform.Rotate(0, currentSpeed * MainManager.TieFramerate(1f), 0);

                if (!failedStylish && !inLeifSpin && a < 50)
                {
                    if (StylishUtils.CheckStylish(ref failedStylish, leif, a, 30f))
                    {
                        inLeifSpin = true;
                        StylishUtils.ShowStylish(1.2f, leif, gain * 0.5f);
                        leif.animstate = (int)MainManager.Animations.ItemGet;
                    }
                }
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < b);

            leif.Emoticon(MainManager.Emoticons.None);
            leif.spritetransform.eulerAngles = startAngle;
            yield return EventControl.halfsec;
            leif.overrideonlyflip = false;
        }

        IEnumerator DoBuffStylish(float gain)
        {
            EntityControl leif = Instance.entityAttacking;
            StylishUtils.ShowStylish(1.2f, leif, gain);
            leif.Jump();
            yield return null;
        }

        IEnumerator DoDebuffStylish(float gain)
        {
            EntityControl leif = Instance.entityAttacking;
            StylishUtils.ShowStylish(1.2f, leif, gain);
            leif.animstate = 103;
            yield return null;
        }

        IEnumerator DoCordycepsLeechStylish(float gain)
        {
            BattleControl.SetDefaultCamera();
            EntityControl leif = Instance.entityAttacking;
            StylishUtils.ShowStylish(1.2f, leif, gain);
            leif.animstate = 101;
            leif.spin = new Vector3(0, 30, 0);

            Vector3[] partyPos = MainManager.battle.partypos;
            int leifIndex = Array.FindIndex(MainManager.battle.partypointer, x => x == MainManager.battle.currentturn);

            Vector3 endPos = partyPos[leifIndex];
            Vector3 startPos = leif.transform.position;
            float a = 0;
            float b = 35f;
            do
            {
                leif.transform.position = Vector3.Lerp(startPos, endPos, a / b);
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < b);

            yield return EventControl.quartersec;
            leif.spin = Vector3.zero;
        }

        public IEnumerator DoStylish(int actionid, int stylishID, float stylishGain)
        {
            Instance.entityAttacking?.Emoticon(Emoticons.None);
            switch (actionid)
            {
                //Basic Attack
                case -1:
                    yield return DoBasicAttackStylish(stylishGain);
                    break;

                case (int)Skills.Icefall:
                    yield return DoIcefallStylish(stylishGain);
                    break;

                case (int)Skills.FrigidCoffin:
                    switch (stylishID)
                    {
                        case 0: // first hit
                            yield return DoFrigidFirstHitStylish(stylishGain);
                            break;
                        case 1: // freeze
                            yield return DoFrigidLastHitStylish(stylishGain);
                            break;
                    }
                    break;

                case (int)Skills.BubbleShieldLite:
                case (int)Skills.BubbleShield:
                case (int)NewSkill.VitiationLite:
                case (int)NewSkill.Vitiation:
                    yield return DoFrigidFirstHitStylish(stylishGain);
                    break;

                // buffs
                case (int)Skills.Empower:
                case (int)Skills.EmpowerPlus:
                case (int)Skills.DefenseUp:
                case (int)Skills.DefenseUpPlus:
                case (int)Skills.AttackUp:
                case (int)Skills.ChargeUpPlus:
                    yield return DoBuffStylish(stylishGain);
                    break;

                // debuffs
                case (int)Skills.AttackDown:
                case (int)Skills.AttackDownPlus:
                case (int)Skills.DefenseBreak1:
                case (int)Skills.DefenseBreakAll:
                case (int)Skills.Cleanse:
                    yield return DoDebuffStylish(stylishGain);
                    break;

                case (int)Skills.IceRain:
                    yield return DoDebuffStylish(stylishGain);
                    break;

                case (int)NewSkill.CordycepsLeech:
                    yield return DoCordycepsLeechStylish(stylishGain);
                    break;
            }
        }
    }

}
