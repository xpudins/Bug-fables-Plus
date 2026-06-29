using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace BFPlus.Patches.EntityControlTranspilers
{
    public class PatchPlayerDashAnim : PatchBaseEntityControlFollow
    {
        public PatchPlayerDashAnim()
        {
            priority = 209;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchCall(AccessTools.Method(typeof(EntityControl), "DoFollow")));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchPlayerDashAnim), "CheckDashingAnim"));
        }

        static void CheckDashingAnim(EntityControl entity)
        {
            if (MainManager.player.dashing && !MainManager.instance.inevent && !(entity.animid == 2 && !MainManager.instance.flags[16]))
            {
                SetDashAnim(entity, 126);
            }
        }

        public static void SetDashAnim(EntityControl entity, int dashAnim)
        {
            entity.overrridejump = true;
            entity.overrideanim = true;
            entity.backsprite = false;
            entity.animstate = dashAnim;
        }
    }

    public class PatchFollowerDashAnim : PatchBaseEntityControlFollow
    {
        public PatchFollowerDashAnim()
        {
            priority = 296;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(MoveType.After, i => i.MatchCall(AccessTools.Method(typeof(EntityControl), "DoFollow")));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchFollowerDashAnim), "CheckDashingAnim"));
        }

        static void CheckDashingAnim(EntityControl entity)
        {
            if (entity.animid == (int)NewAnimID.JumpAnt || entity.animid == (int)MainManager.AnimIDs.ChompyChan - 1)
            {
                if (MainManager.player.dashing && !MainManager.instance.inevent)
                {
                    PatchPlayerDashAnim.SetDashAnim(entity, entity.animid == (int)NewAnimID.JumpAnt ? 125 : 104);
                }
                else
                {
                    entity.overrridejump = false;
                    entity.overrideanim = false;
                }
            }
        }
    }
}
