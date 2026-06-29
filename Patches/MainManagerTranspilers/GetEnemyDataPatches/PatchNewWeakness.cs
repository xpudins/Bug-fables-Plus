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

namespace BFPlus.Patches.MainManagerTranspilers.GetEnemyDataPatches
{
    public class PatchNewWeakness : PatchBaseMainManagerGetEnemyData
    {
        public PatchNewWeakness()
        {
            priority = 1049;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = null;
            cursor.GotoNext(MoveType.After, i => i.MatchLdstr(""), i=>i.MatchCall(out _), i=>i.MatchBrfalse(out label));

            cursor.Emit(OpCodes.Ldloc_0);
            cursor.Emit(OpCodes.Ldloc_3);
            cursor.Emit(OpCodes.Ldloc, 8);
            cursor.Emit(OpCodes.Ldelem_Ref);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchNewWeakness), nameof(CheckNewWeakness)));
            cursor.Emit(OpCodes.Brtrue, label);
        }

        static bool CheckNewWeakness(MainManager.BattleData data, string weakness)
        {
            if(Enum.TryParse(weakness, out NewProperty result))
            {
                data.weakness.Add((BattleControl.AttackProperty)result);
                return true;
            }
            return false;
        }
    }
}
