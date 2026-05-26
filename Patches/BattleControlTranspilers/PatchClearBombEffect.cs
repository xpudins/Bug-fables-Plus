using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using static MainManager;

namespace BFPlus.Patches.BattleControlTranspilers
{
    public class PatchClearBombEffect : PatchBaseClearBombEffect
    {
        public PatchClearBombEffect()
        {
            priority = 0;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdstr("IceBreak"));
            cursor.Prev.OpCode = OpCodes.Nop;

            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchClearBombEffect), "Prefix"));
            cursor.Emit(OpCodes.Ldstr, "IceBreak");
        }

        static void Prefix()
        {
            int heal = 5;
            if (!battle.enemy)
            {
                heal += BadgeHowManyEquipped((int)BadgeTypes.HealPlus) + (2 * BadgeHowManyEquipped((int)BadgeTypes.BombPlus));
                for (int i = 0; i != instance.playerdata.Length; i++)
                {
                    if (instance.playerdata[i].hp > 0)
                    {
                        battle.Heal(ref instance.playerdata[i], heal, false);
                    }
                }
            }
            else
            {
                for (int i = 0; i != battle.enemydata.Length; i++)
                {
                    if (battle.enemydata[i].hp > 0)
                    {
                        battle.Heal(ref battle.enemydata[i], heal, false);
                    }
                }
            }
        }
    }
}
