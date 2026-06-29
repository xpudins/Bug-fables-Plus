using BFPlus.Extensions;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BFPlus.Patches.NPCControlTranspilers
{
    public class PatchChargeAndAttack : PatchBaseChargeAndAttack
    {
        public PatchChargeAndAttack()
        {
            priority = 0;
        }
        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            cursor.GotoNext(i => i.MatchLdloc1());

            var label = cursor.DefineLabel();
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchChargeAndAttack), "CheckNewChargeAttack"));
            cursor.Emit(OpCodes.Brfalse, label);

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchChargeAndAttack), "CheckNewBehaviors"));
            Utils.InsertYieldReturn(cursor);
            cursor.Emit(OpCodes.Ldc_I4_0);
            cursor.Emit(OpCodes.Ret);
            cursor.MarkLabel(label);
        }

        static bool CheckNewChargeAttack(NPCControl npc)
        {
            switch (npc.entity.animid)
            {
                case (int)NewAnimID.Worm:
                case (int)NewAnimID.WormSwarm:
                case (int)NewAnimID.Frostfly:
                case (int)NewAnimID.MechaJaw:
                    return true;
            }
            return false;
        }

        static IEnumerator CheckNewBehaviors(NPCControl npc)
        {
            npc.entity.StopForceMove();
            npc.entity.overrideanim = true;
            switch (npc.entity.animid)
            {
                case (int)NewAnimID.Worm:
                case (int)NewAnimID.WormSwarm:
                    yield return DoWormAttackBehavior(npc);
                    break;
                case (int)NewAnimID.Frostfly:
                    yield return DoFrostFlyBehavior(npc);
                    break;

                case (int)NewAnimID.MechaJaw:
                    yield return DoMechaJawBehavior(npc);
                    break;
            }

            yield return new WaitForSeconds(0.5f);
            while (MainManager.IsPaused())
            {
                yield return null;
            }
            if (!npc.inrange)
            {
                npc.entity.Emoticon(1, 60);
            }
            npc.entity.animstate = 0;
            npc.entity.overrideanim = false;
            npc.attacking = false;
            npc.StopForceBehavior();
        }

        static IEnumerator DoMechaJawBehavior(NPCControl npc)
        {
            float jumpHeight = 3f;
            float a = 0;
            float endTime = 30;

            npc.entity.FlipSpriteAngleAt(MainManager.player.transform.position, new Vector3(0f, 90f));
            Vector3 startPos = npc.transform.position;
            Vector3 direction = MainManager.GetDirection(MainManager.player.transform.position, startPos).normalized;
            Vector3 targetPos = MainManager.LimitRadius(MainManager.player.transform.position + direction * 1.5f,
                npc.entity.startpos.Value, npc.radiuslimit);
            npc.entity.overrideanim = true;
            npc.entity.animstate = 113;
            do
            {
                npc.transform.position = MainManager.BeizierCurve3(startPos, targetPos, jumpHeight, a / endTime);
                npc.attacking = true;

                if (CheckBattleRange(1, npc))
                {
                    yield break;
                }

                npc.entity.DetectDirection(npc.transform.position - direction);
                if (npc.entity.hitwall)
                {
                    yield break;
                }

                yield return null;
                a += MainManager.TieFramerate(1f);
                while (MainManager.IsPaused())
                {
                    yield return null;
                }
            } while (a < endTime);
            npc.entity.animstate = (int)MainManager.Animations.Chase;
        }

        static IEnumerator DoFrostFlyBehavior(NPCControl npc)
        {
            npc.entity.animstate = 107;
            npc.entity.PlaySound("BugWingFast", 1f, 1.2f);

            yield return new WaitForSeconds(0.1f);
            while (MainManager.IsPaused())
            {
                yield return null;
            }

            npc.entity.animstate = 103;
            npc.attacking = true;
            npc.entity.PlaySound("PingShot");
            npc.entity.PlaySound("FastWoosh");

            yield return new WaitForSeconds(0.1f);
            while (MainManager.IsPaused())
            {
                yield return null;
            }
            if (CheckBattleRange(2, npc))
            {
                yield break;
            }
        }

        static bool CheckBattleRange(float range, NPCControl npc)
        {
            if (Vector3.Distance(npc.transform.position, MainManager.player.transform.position) < range && !MainManager.instance.minipause)
            {
                npc.StartBattle();
                npc.entity.overrideanim = false;
                npc.StopForceBehavior();
                return true;
            }
            return false;
        }

        static IEnumerator DoWormAttackBehavior(NPCControl npc)
        {
            bool isSwarm = npc.entity.animid == (int)NewAnimID.WormSwarm;
            List<EntityControl> worms = new List<EntityControl>() { npc.entity };
            if (isSwarm)
            {
                worms.AddRange(npc.entity.subentity);
            }

            npc.entity.animstate = 100;
            foreach (var worm in worms)
                worm.digging = false;

            MainManager.PlaySound("DigPop2");
            npc.attacking = true;

            if (npc.entity.digpart != null && npc.entity.digpart.Length > 0 && npc.entity.digpart[1] != null)
            {
                npc.entity.digpart[1].transform.position = new Vector3(0f, -9999f);
            }
            if (npc.dirtcd <= 0f)
            {
                MainManager.PlayParticle("DirtExplodeLight", npc.transform.position, 1f).transform.localScale = Vector3.one * 0.75f;
            }
            npc.dirtcd = 30f;

            yield return EventControl.quartersec;
            while (MainManager.IsPaused())
            {
                yield return null;
            }
            yield return new WaitForSeconds(0.1f);
            while (MainManager.IsPaused())
            {
                yield return null;
            }

            if (CheckBattleRange(isSwarm ? 1.4f : 1.2f, npc))
            {
                yield break;
            }

            yield return EventControl.tenthsec;

            if (npc.inrange)
            {
                npc.attacking = false;
                foreach (var worm in worms)
                    worm.digging = false;
                npc.entity.digtime = 100f;
            }
        }

    }
}
