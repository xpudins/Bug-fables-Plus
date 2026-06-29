using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BFPlus.Extensions
{
    public class CorkScrew : MonoBehaviour
    {
        public int flag = 990;
        Vector3 bouncepos =  new Vector3(0, 5,0);
        public int itemId = (int)Medal.Corkscrew;
        public int itemType = 2;
        float actionCooldown = 0;
        float targetCooldown = 300;
        float cooldownIncrease = 1;
        float cooldownDecrease = 0.5f;
        Vector3 spin = new Vector3(0, 0, -20);
        AudioSource sound;
        Vector3 startPos;
        float hitRange = 2f;
        Vector3 target = new Vector3(0.5f,1);
        float goUpSpeed = 8f;
        Vector3 dropItemSpot = new Vector3(-22.8f, 10, 0f);
        bool cancelUpdate = false;
        public Transform stock;

        void Start()
        {
            if (!MainManager.instance.flags[flag] && MainManager.BadgeIsEquipped(2))
            {
                MainManager.map.hiddenitem = new int?(100);
            }

            startPos = transform.position;
        }

        void Update()
        {
            if (!cancelUpdate)
            {
                bool hit = MainManager.player.beemerang != null && MainManager.GetDistance(startPos, MainManager.player.beemerang.transform.position) < hitRange;
                if (hit)
                {
                    if (actionCooldown < targetCooldown)
                    {
                        actionCooldown += MainManager.TieFramerate(cooldownIncrease);
                    }
                }
                else if (actionCooldown > 0)
                {
                    actionCooldown -= MainManager.TieFramerate(cooldownDecrease);
                }

                actionCooldown = Mathf.Clamp(actionCooldown, 0f, targetCooldown);

                float spinOffset = Mathf.Clamp01(actionCooldown / targetCooldown);
                transform.localEulerAngles += spin * spinOffset;
                stock.localEulerAngles += spin * spinOffset;

                Vector3 targetPos = new Vector3(Mathf.Lerp(startPos.x, startPos.x + target.x, spinOffset), Mathf.Lerp(startPos.y, startPos.y + target.y, spinOffset), transform.position.z);
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * goUpSpeed);

                if (spinOffset > 0f)
                {
                    if (sound == null || !sound.isPlaying)
                    {
                        sound = MainManager.PlaySound("SpinSwitch0", 0.5f);
                    }
                    sound.loop = true;
                    sound.pitch = 0.5f + spinOffset;
                    sound.volume = MainManager.GetSoundDistance(transform.position) * 0.5f * MainManager.soundvolume;
                }
                else if (sound != null)
                    sound.loop = false;

                if (actionCooldown >= targetCooldown)
                {
                    cancelUpdate = true;
                    MainManager.instance.StartCoroutine(GiveReward());
                }
            }
        }

        IEnumerator GiveReward()
        {
            MainManager.PlaySound("Pop", -1, 0.8f, 1);
            
            if(sound != null) 
                sound.loop = false;
            float a = 0;
            float b = 20;
            Vector3 start = transform.position;
            Vector3 startStock = stock.transform.position;
            do
            {
                if (stock == null || transform == null)
                    yield break;
                stock.position = Vector3.Lerp(startStock, dropItemSpot, a / b);
                transform.position = Vector3.Lerp(start, dropItemSpot, a / b);
                transform.localEulerAngles += spin;
                stock.localEulerAngles += spin;
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a <b);

            if (stock == null || transform == null)
                yield break;

            Transform parent = transform.parent;
            transform.gameObject.GetComponent<MeshRenderer>().enabled = false;
            stock.gameObject.SetActive(false);

            NPCControl npccontrol = EntityControl.CreateItem(dropItemSpot, itemType, itemId, bouncepos, -1);
            npccontrol.insideid = MainManager.instance.insideid;
            npccontrol.activationflag = flag;

            startPos = parent.position;
            Vector3 targetPos = startPos + Vector3.down * 10;
            a = 0;
            b = 180;
            do
            {
                if (parent == null || transform == null)
                    yield break;
                parent.position = Vector3.Lerp(startPos, targetPos, a / b);
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < b);

            Destroy(parent?.gameObject);
        }

        void OnDestroy()
        {
            if (sound != null)
                sound.loop = false;
        }
    }
}
