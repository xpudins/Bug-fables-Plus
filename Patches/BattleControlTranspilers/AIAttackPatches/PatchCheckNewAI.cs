using BFPlus.Extensions;
using BFPlus.Extensions.EnemyAI;
using BFPlus.Patches.DoActionPatches;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MainManager;

namespace BFPlus.Patches.BattleControlTranspilers.AIAttackPatches
{
    public class PatchCheckNewAI : PatchBaseAIAttack
    {
        public PatchCheckNewAI()
        {
            priority = 37141;
        }

        protected override void ApplyPatch(ILCursor cursor, ILContext context)
        {
            ILLabel label = cursor.DefineLabel();

            cursor.GotoNext(MoveType.After, i => i.MatchStloc2());

            int cursorIndex = cursor.Index;

            ILLabel labelJump = null;
            cursor.GotoNext(i => i.MatchBr(out labelJump));
            cursor.Goto(cursorIndex);

            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchCheckNewAI), "CheckNewAI"));
            cursor.Emit(OpCodes.Brfalse, label);

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, AccessTools.Method(typeof(PatchCheckNewAI), "DoNewAiParty"));
            Utils.InsertYieldReturn(cursor);
            cursor.Emit(OpCodes.Br, labelJump);
            cursor.MarkLabel(label);
        }

        static bool CheckNewAI()
        {
            List<int> newAiParty = new List<int>() { (int)NewAnimID.JumpAnt, (int)NewAnimID.Hoaxe };
            return newAiParty.Contains(MainManager.battle.aiparty.animid);
        }

        static IEnumerator DoNewAiParty()
        {
            int helperDamageBoost = MainManager.BadgeHowManyEquipped(90);
            switch (MainManager.battle.aiparty.animid)
            {
                case (int)NewAnimID.Hoaxe:
                    yield return DoHoaxeAI(helperDamageBoost);
                    break;

                case (int)NewAnimID.JumpAnt:
                    yield return DoJumpAntAI(helperDamageBoost);
                    break;
            }
        }

        static IEnumerator DoHoaxeAI(int damageBoost)
        {
            BattleControl battle = MainManager.battle;
            EntityControl hoaxe = battle.aiparty;
            int hoaxeDamage = 4 + damageBoost;

            int targetId = UnityEngine.Random.Range(0, battle.enemydata.Length);

            hoaxe.animstate = 113;
            yield return EventControl.halfsec;

            MainManager.PlaySound("FirePillar");
            DialogueAnim pillar = (UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Objects/FirePillar 1"), battle.enemydata[targetId].battleentity.transform.position, Quaternion.identity) as GameObject).AddComponent<DialogueAnim>();
            pillar.transform.parent = battle.battlemap.transform;
            pillar.transform.localScale = new Vector3(0f, 1f, 0f);
            pillar.targetscale = new Vector3(0.35f, 1f, 0.35f);
            pillar.shrink = false;
            pillar.shrinkspeed = 0.015f;
            yield return new WaitForSeconds(0.65f);
            pillar.shrinkspeed = 0.2f;
            yield return new WaitForSeconds(0.2f);
            MainManager.ShakeScreen(0.25f, 0.75f);
            battle.DoDamage(null, ref battle.enemydata[targetId], hoaxeDamage, BattleControl.AttackProperty.Fire, false);

            yield return new WaitForSeconds(1.15f);
            pillar.targetscale = new Vector3(0f, 1f, 0f);
            ParticleSystem[] componentsInChildren = pillar.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                componentsInChildren[i].Stop();
            }
            pillar.shrinkspeed = 0.05f;
            UnityEngine.Object.Destroy(pillar.gameObject, 3f);
            hoaxe.animstate = 115;

            yield return EventControl.halfsec;
            hoaxe.animstate = (int)MainManager.Animations.Flustered;

            if (!MainManager.instance.flags[962])
            {
                //put in common dialogue file later lul
                string line = "Soooo, do either of you have any idea why HE is here?|next,1|I... I really have no idea. Could it be because..?|next,2|We'd rather not think too much  about the implications here.";
                MainManager.instance.StartCoroutine(MainManager.SetText(line, true, Vector3.zero, MainManager.instance.playerdata[0].battleentity.transform, null));
                while (MainManager.instance.message)
                {
                    yield return null;
                }

                MainManager.instance.flags[962] = true;
            }
        }

        static IEnumerator DoJumpAntAI(int damageBoost)
        {
            BattleControl battle = MainManager.battle;
            EntityControl jumpAnt = battle.aiparty;
            Vector3 basePos = jumpAnt.transform.position;

            //change damage depending on which jump ant ally it is
            int jumpAntDamage = 1;
            bool canStylish = false;

            if (MainManager.instance.flags[967])
            {
                jumpAntDamage++;
                canStylish = true;
            }

            if (MainManager.instance.flags[974])
            {
                jumpAntDamage += 2;
            }

            int targetId;
            jumpAntDamage += damageBoost;

            targetId = MainManager.battle.GetNextGrounded(false, false);

            if (targetId != -1 && UnityEngine.Random.Range(0, 2) == 0)
            {
                yield return DoHammer(jumpAnt, jumpAntDamage, targetId, canStylish);
            }
            else
            {
                int[] targets = battle.GetTargetList(new List<BattleControl.BattlePosition>() { BattleControl.BattlePosition.Ground, BattleControl.BattlePosition.Flying });

                if (targets.Length > 0)
                {
                    targetId = targets[UnityEngine.Random.Range(0, targets.Length)];
                    yield return DoJumpAttack(jumpAnt, targetId, jumpAntDamage, canStylish);
                }
            }

            jumpAnt.MoveTowards(basePos, 2f, 1, 0);
            while (jumpAnt.forcemove)
            {
                yield return null;
            }
            jumpAnt.transform.position = basePos;
            jumpAnt.flip = true;
        }

        static IEnumerator DoJumpAttack(EntityControl entity, int target, int damage, bool canStylish)
        {
            damage = Mathf.CeilToInt((float)damage / 2);
            Vector3 basePos = entity.transform.position;
            entity.animstate = (int)MainManager.Animations.Walk;
            BattleControl battle = MainManager.battle;
            yield return EventControl.tenthsec;

            entity.MoveTowards(Vector3.zero, 1.5f);
            yield return new WaitUntil(() => !entity.forcemove);

            entity.animstate = (int)MainManager.Animations.Angry;
            yield return new WaitForSeconds(0.08f);

            entity.PlaySound("Jump", 1, 0.9f);
            entity.LockRigid(true);
            entity.animstate = (int)MainManager.Animations.Jump;
            float a = 0;

            float b = 30;
            Vector3 startPos = entity.transform.position;
            Vector3 targetPos = battle.enemydata[target].battleentity.transform.position + new Vector3(0, 1, -0.1f) + Vector3.up * battle.enemydata[target].battleentity.height;
            do
            {
                entity.transform.position = MainManager.BeizierCurve3(startPos, targetPos, 5, a / b);
                a += MainManager.TieFramerate(1f);
                yield return null;
            } while (a < b);

            entity.animstate = 100;
            yield return null;
            battle.DoDamage(null, ref battle.enemydata[target], damage, BattleControl.AttackProperty.Pierce, false);
            yield return EventControl.tenthsec;

            entity.PlaySound("Jump", 1, 0.9f);

            entity.animstate = (int)MainManager.Animations.Jump;

            if (canStylish)
            {
                Vector3 mid = entity.transform.position + Vector3.up * 5;
                startPos = entity.transform.position;

                bool stylished = false;
                a = 0;
                b = 30;
                do
                {
                    entity.transform.position = MainManager.BeizierCurve3(startPos, targetPos, mid, a / b);
                    a += MainManager.TieFramerate(1f);
                    if (a >= b / 2 && !stylished && canStylish)
                    {
                        JumpAntAI.DoShrinkStompStylish(entity, true);
                        stylished = true;
                    }

                    yield return null;
                } while (a < b);

                entity.animstate = 100;
                yield return null;
                battle.DoDamage(null, ref battle.enemydata[target], damage, BattleControl.AttackProperty.Pierce, false);
                yield return EventControl.tenthsec;
            }
            yield return JumpAntAI.ReturnFromJumpAttack(entity, canStylish, true);
        }

        static IEnumerator DoHammer(EntityControl entity, int damage, int target, bool canStylish)
        {
            Vector3 basePos = entity.transform.position;
            entity.animstate = (int)MainManager.Animations.Walk;
            yield return EventControl.tenthsec;

            entity.MoveTowards(battle.enemydata[target].battleentity.transform.position + new Vector3(-1.5f, 0, -0.1f), 1.5f);
            yield return new WaitUntil(() => !entity.forcemove);

            entity.animstate = 104;
            yield return EventControl.sec;

            entity.animstate = 106;
            yield return EventControl.tenthsec;

            battle.DoDamage(null, ref battle.enemydata[target], damage, BattleControl.AttackProperty.Pierce, false);
            MainManager.ShakeScreen(0.1f, 0.5f);
            yield return EventControl.tenthsec;

            if (UnityEngine.Random.Range(0, 10) < 5 && canStylish)
            {
                yield return JumpAntAI.DoHammerStylishFlip(entity, 1, 25, 7, entity.transform.position - Vector3.right * 1.5f, true);
            }
            else
            {
                yield return EventControl.halfsec;
            }

            entity.animstate = (int)MainManager.Animations.Idle;
        }

    }
}
