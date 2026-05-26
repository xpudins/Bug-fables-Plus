using HarmonyLib;

namespace BFPlus.Patches.CardGameTranspilers
{
    [HarmonyPatch(typeof(CardGame), "SetArena")]
    public class PatchCardGameSetArena
    {
        static void Postfix(CardGame __instance, int mapid)
        {
            __instance.entities[3].animstate = PatchOpponentCardAnimstate.CheckOpponentAnimstate((int)MainManager.Animations.BattleIdle);

            if (mapid == (int)MainManager.BattleMaps.Theater)
            {
                __instance.map.transform.position = new UnityEngine.Vector3(-2, 0, 0);
            }
        }
    }
}
