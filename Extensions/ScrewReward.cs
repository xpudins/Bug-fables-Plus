using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MainManager;

namespace BFPlus.Extensions
{
    public class ScrewReward : MonoBehaviour
    {
        bool spitItem = false;
        public int flag;
        public Vector3 itemPos;
        Vector3 bouncepos;
        public int itemId;
        public int itemType = 2;
        NPCControl screwSwitch;

        void Start()
        {
            if (!MainManager.instance.flags[flag] && MainManager.BadgeIsEquipped(2))
            {
                 MainManager.map.hiddenitem = new int?(100);
            }
        }

        void Update()
        {
            if(!spitItem && screwSwitch != null && !MainManager.instance.flags[flag] && screwSwitch.actioncooldown >= screwSwitch.vectordata[0].z)
            {
                MainManager.DeathSmoke(screwSwitch.transform.position);
                spitItem = true;
                PlaySound("Pop", -1, 0.8f, 1);
                NPCControl npccontrol = EntityControl.CreateItem(itemPos, itemType, itemId, bouncepos, -1);
                npccontrol.insideid = MainManager.instance.insideid;
                npccontrol.activationflag = flag;
            }
        }

        public static void CreateScrewReward(NPCControl parent, int itemId, int itemType, Vector3 itemPos, Vector3 bouncePos, int flag)
        {
            ScrewReward comp = parent.gameObject.AddComponent<ScrewReward>();
            comp.itemId = itemId;
            comp.itemType = itemType;
            comp.itemPos = itemPos;
            comp.bouncepos = bouncePos;
            comp.flag = flag;
            comp.screwSwitch = parent;
        }
    }
}
