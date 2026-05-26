using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.AdvanceTurnEntityPatches
{
    public class PatchGradualTPEnemy : PatchBaseAdvanceTurnEntity
    {
        public PatchGradualTPEnemy()
        {
            priority = 526;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdsfld(out _), i => i.MatchLdfld(AccessTools.Field(typeof(MainManager), "tp")));
            cursor.GotoNext(i => i.MatchLdfld(out _));
            cursor.Prev.OpCode = OpCodes.Nop;

            int cursorIndex = cursor.Index;
            ILLabel jumpLabel = null;

            cursor.GotoNext(i => i.MatchBge(out jumpLabel));
            cursor.Goto(cursorIndex);

            ILLabel label = cursor.DefineLabel();
            cursor.Emit(OpCodes.Ldloc_0);
            cursor.Emit(OpCodes.Brtrue, label);

            cursor.Emit(OpCodes.Ldarg_1);
            cursor.Emit(OpCodes.Ldarg_2);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchGradualTPEnemy), "DoEnemyTPRegen"));
            cursor.Emit(OpCodes.Br, jumpLabel);

            cursor.MarkLabel(label);
            cursor.Emit(OpCodes.Ldsfld, AccessTools.Field(typeof(MainManager), "instance"));
        }

        static void DoEnemyTPRegen(ref MainManager.BattleData target, ref bool delay)
        {
            BattleControl_Ext.Instance.RecoverEnemyTp(2, target.battleentity.battleid);
            delay = true;
        }

    }
}
