using BFPlus.Patches.BattleControlTranspilers.AdvanceTurnEntityPatches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.AI;
using static MainManager;

namespace BFPlus.Extensions.Events
{
    public class SpinoutCarEvent : NewEvent
    {
        protected override IEnumerator DoEvent(NPCControl caller, EventControl instance)
        {
            while (MainManager.instance.message)
                yield return null;
            EntityControl[] party = MainManager.GetPartyEntities(true);
            Transform car = MainManager.map.transform.GetChild(0).Find("SpinoutCar(Clone)").GetChild(0);
            Animator carAnim = car.GetComponent<Animator>();

            carAnim.Play("Opening");

            while(MainManager.instance.flagvar[(int)NewFlagVar.SpinoutCarBattery] < 2)
            {
                MainManager.DialogueText(MainManager.map.dialogues[7], null, null);
                while (MainManager.instance.message)
                    yield return null;

                if (MainManager.listcanceled)
                    break;

                if (MainManager.instance.flagvar[0] == (int)NewItem.BeeBattery)
                {
                    MainManager.instance.items[0].Remove((int)NewItem.BeeBattery);
                    MainManager.instance.flagvar[(int)NewFlagVar.SpinoutCarBattery]++;
                    foreach (Transform child in car)
                    {
                        if (!child.gameObject.activeSelf && child.name.Contains("Battery"))
                        {
                            child.gameObject.SetActive(true);
                            break;
                        }
                    }
                }
                else
                {
                    MainManager.DialogueText(MainManager.map.dialogues[8], null, null);
                    while (MainManager.instance.message)
                        yield return null;
                }
            }

            if (MainManager.instance.flagvar[(int)NewFlagVar.SpinoutCarBattery] >= 2)
            {
                MainManager.instance.flags[991] = true;
                carAnim.Play("Closing");
                Transform parent = car.parent;
                MainManager.PlaySound("Rumble", 9, 1.5f, 1, true);
                yield return MainManager.ShakeObject(parent, Vector3.one*0.1f, 60, true);
                MainManager.StopSound(9);

                MainManager.PlaySound("Boing1", 1.2f, 1);
                Vector3 targetPos = new Vector3(10.73f, 1f, 8.77f);
                Vector3 startPos = parent.position;

                float a = 0;
                float b = 40;

                bool changedAngle = false;
                bool startBoing = false;
                do
                {
                    parent.position = MainManager.BeizierCurve3(startPos, targetPos, 15, a / b);

                    if(a > b / 2 && !changedAngle)
                    {
                        changedAngle = true;
                        parent.localEulerAngles = new Vector3(90, 0, 0);
                    }

                    if(a >= 35f && !startBoing)
                    {
                        startBoing = true;
                        carAnim.Play("Boing");
                    }

                    a += MainManager.TieFramerate(1f);
                    yield return null;
                } while (a<b);

                yield return EventControl.halfsec;

                MainManager.SetCamera(car, null, 0.1f, MainManager.defaultcamoffset + new Vector3(-1, -0.5f, -1));
                targetPos = new Vector3(-17.47f, 1, 11.30f);
                MainManager.PlaySound("UltimaxSpinAttack", 1.2f, 1);
                yield return BattleControl_Ext.LerpPosition(60, parent.position, targetPos, parent);

                MainManager.PlaySound("Explosion3", 1.2f, 1);
                MainManager.PlayParticle("explosion", car.transform.position);
                parent.gameObject.SetActive(false);

                NPCControl spinoutMedal = EntityControl.CreateItem(parent.position, 2, (int)Medal.Spinout, Vector3.zero, -1);
                spinoutMedal.activationflag = 992;
                yield return EventControl.sec;

                MainManager.ResetCamera();
                yield return EventControl.tenthsec;
            }
            carAnim?.Play("Closing");
            yield return null;
        }
    }
}
