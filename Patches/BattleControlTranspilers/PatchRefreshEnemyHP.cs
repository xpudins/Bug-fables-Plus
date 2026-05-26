using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFPlus.Patches.BattleControlTranspilers
{
    public class PatchHPStringAlloc : PatchBaseRefreshEnemyHP
    {
        public PatchHPStringAlloc()
        {
            priority = 183;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchStfld(AccessTools.Field(typeof(DynamicFont), "text")));
            cursor.GotoPrev(MoveType.After, i => i.MatchBrfalse(out _));

            ILLabel label = cursor.DefineLabel();

            cursor.Emit(OpCodes.Ldloc_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchHPStringAlloc), nameof(CheckHPUpdate)));
            cursor.Emit(OpCodes.Brfalse, label);

            cursor.GotoNext(MoveType.After, i => i.MatchStfld(AccessTools.Field(typeof(DynamicFont), "text")));
            cursor.MarkLabel(label);
        }

        static bool CheckHPUpdate(int enemyIndex)
        {
            if (BattleControl_Ext.Instance.cachedEnemies[enemyIndex].transform != MainManager.battle.enemydata[enemyIndex].battleentity.transform)
            {
                BattleControl_Ext.Instance.cachedEnemies[enemyIndex].transform = MainManager.battle.enemydata[enemyIndex].battleentity.transform;
                BattleControl_Ext.Instance.cachedEnemies[enemyIndex].hp = null;
                BattleControl_Ext.Instance.cachedEnemies[enemyIndex].def = null;
            }

            if(BattleControl_Ext.Instance.cachedEnemies[enemyIndex].hp != MainManager.battle.enemydata[enemyIndex].hp)
            {
                BattleControl_Ext.Instance.cachedEnemies[enemyIndex].hp = MainManager.battle.enemydata[enemyIndex].hp;
                return true;
            }
            return false;
        } 
    }

    public class PatchDEFStringAlloc : PatchBaseRefreshEnemyHP
    {
        public PatchDEFStringAlloc()
        {
            priority = 261;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdfld(AccessTools.Field(typeof(EntityControl), "defstat")));
            ILLabel label = null;
            cursor.GotoNext(MoveType.After, i => i.MatchBrfalse(out label));

            cursor.Emit(OpCodes.Ldloc_0);
            cursor.Emit(OpCodes.Ldloc_2);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchDEFStringAlloc), nameof(CheckDEFUpdate)));
            cursor.Emit(OpCodes.Brfalse, label);
        }

        static bool CheckDEFUpdate(int enemyIndex, int def)
        {
            if (BattleControl_Ext.Instance.cachedEnemies[enemyIndex].def != def)
            {
                BattleControl_Ext.Instance.cachedEnemies[enemyIndex].def = def;
                return true;
            }
            return false;
        }
    }
}
