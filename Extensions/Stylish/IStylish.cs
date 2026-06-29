using System.Collections;
using UnityEngine;

namespace BFPlus.Extensions.Stylish
{
    public interface IStylish
    {
        IEnumerator DoStylish(int actionid, int stylishID, float stylishGain);
    }

    public class StylishUtils
    {
        public static bool CheckStylish(ref bool failedStylish, EntityControl entity, float time, float antiSpamFrames)
        {
            if (time < antiSpamFrames)
            {
                if (time > 3f)
                {
                    if (MainManager.GetKey(4, false))
                    {
                        failedStylish = true;
                        return false;
                    }
                }
            }
            else if (!failedStylish)
            {
                if (MainManager.BadgeIsEquipped((int)Medal.TimingTutor))
                    entity.Emoticon(MainManager.Emoticons.Exclamation);
                if (MainManager.GetKey(4, false))
                {
                    entity.Emoticon(MainManager.Emoticons.None);
                    return true;
                }
            }
            return false;
        }


        public static void ShowStylish(float pitch, EntityControl entity, float stylishIncrease = 0.1f, bool increaseBar = true, Vector3? offset = null)
        {
            if(stylishIncrease > 0 && !float.IsNaN(stylishIncrease) && !float.IsInfinity(stylishIncrease))
            {
                MainManager.PlaySound("AtkSuccess", pitch, 1);
                MainManager.battle.StartCoroutine(BattleControl_Ext.Instance.ShowStylishMessage(entity, offset));

                if (increaseBar)
                    MainManager.battle.StartCoroutine(BattleControl_Ext.Instance.IncreaseStylishBar(stylishIncrease, entity));
            }
        }
    }
}
