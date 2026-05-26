using BFPlus.Patches.DoActionPatches;
using BFPlus.Extensions;
using BFPlus.Extensions.BattleStuff.StatusStuff;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.BattleControlTranspilers.CheckDeadPatches
{
    public class PatchPhoenixCheck : PatchBaseCheckDead
    {
        public PatchPhoenixCheck()
        {
            priority = 155710;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchLdfld(AccessTools.Field(typeof(EntityControl), "dead")), i => i.MatchBrtrue(out _));

            var jumpLabel = cursor.Prev.Operand;
            var label = cursor.DefineLabel();
            var idRef = cursor.Body.Instructions[cursor.Index - 5];

            cursor.Emit(idRef.OpCode, idRef.Operand);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchPhoenixCheck), "ClearEffectsOnDeath"));

            cursor.Emit(idRef.OpCode, idRef.Operand);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "DoInkBlotPlayer"));

            cursor.Emit(idRef.OpCode, idRef.Operand);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "CheckPhoenix"));
            cursor.Emit(OpCodes.Brfalse, label);

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(idRef.OpCode, idRef.Operand);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(BattleControl_Ext), "DoPhoenix"));
            Utils.InsertYieldReturn(cursor);
            cursor.Emit(OpCodes.Br, jumpLabel);
            cursor.MarkLabel(label);

            cursor.Emit(idRef.OpCode, idRef.Operand);
            cursor.Emit(OpCodes.Ldc_I4_1);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(Dizzy), "CheckDizzyKO"));
        }

        static void ClearEffectsOnDeath(int playerId)
        {
            var ext = Entity_Ext.GetEntity_Ext(MainManager.instance.playerdata[playerId].battleentity);
            ext.cantSwap = false;
            ext.canBypassSwapRestrictions = false;

            if(!BattleControl_Ext.CheckPhoenix(playerId))
                BattleControl_Ext.Instance.ResetTinyHugeEffect(MainManager.instance.playerdata[playerId].battleentity);
        }

    }
}
