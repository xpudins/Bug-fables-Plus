using BFPlus.Extensions.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BattleControl;
using static MainManager;

namespace BFPlus.Extensions.EnemyAI
{
    //Mothmite
    //Faded Mothfly in a weathered old castle.Has developed different cooperative tactics to the Mothflies of the Forsaken Lands.
    //Attacks:
    //Bash[4 DMG]: Flies up and off the screen, plummeting down towards a random target.
    //If other Mothmites are present, they will all rapid-fire rain down on the same target.
    //Drain Bubble [3 DMG, 3 TP DRAIN]: Lobs a slow bubble of gunk upward, which floats down onto a completely random target.
    //If other Mothmites are present, they will all spit bubbles at random targets in quick succession.
    //Gimmicks:
    //When attacking as a team, Mothmites after the first will deal increasing damage.
    //For any given team attack, every hit grants +1 damage to all subsequent hits.
    public class MothmiteAI : AI
    {
        const int BASE_BASH_DAMAGE = 4;
        const int BASE_DRAIN_BUBBLE_DAMAGE = 3;
        const int BASE_DRAIN_BUBBLE_TP = -3;
        const int BASH_STICKY_TURNS = 2;
        const int SPLATTER_DMG = 3;
        const int SPLATTER_AMOUNT = 4;
        int damage;
        BattleControl battle = null;
        int attackDone = 0;
        Coroutine summonRoutine = null;
        bool isAddingMothmite = false;
        int jumpAntId = -1;
        JumpAntFight fightComponent;
        public override IEnumerator DoBattleAI(EntityControl entity, int actionid)
        {
            battle = MainManager.battle;
            yield return null;
            jumpAntId = battle.EnemyInField((int)NewEnemies.JumpAnt);

            int[] mothmites = FindAllMothmites();

            bool hardmode = battle.HardMode();
            fightComponent = BattleControl_Ext.Instance.jumpAntFightComp;

            if (mothmites.Length == 1 && jumpAntId != -1)
            {
                int itemTargetId = fightComponent.GetItemTargetId(jumpAntId);

                if (UnityEngine.Random.Range(0, 10) == 0 && itemTargetId != -1)
                {
                    yield return fightComponent.DoUseItem(entity, itemTargetId, fightComponent);
                    yield break;
                }

                if (UnityEngine.Random.Range(0, 10) == 0 && !battle.IsStopped(battle.enemydata[jumpAntId]))
                {
                    yield return battle.EnemyRelay(entity, jumpAntId);
                    yield break;
                }

                if (UnityEngine.Random.Range(0, 10) < 5 && !battle.IsStopped(battle.enemydata[jumpAntId]))
                {
                    yield return DoSplatterBall(entity, battle.enemydata[jumpAntId].battleentity, actionid);
                    //battle.EndEnemyTurn(jumpAntId);
                    yield break;
                }
            }

            if (battle.enemydata[actionid].position == BattlePosition.Ground && (mothmites.Length > 1 || fightComponent != null))
            {
                damage = BASE_DRAIN_BUBBLE_DAMAGE;
                float delay = hardmode ? 0.35f : 0.45f;
                yield return DoAttackAll(mothmites, DoDrainBubble, delay);
                yield return ChangeMothmitesPos(BattlePosition.Flying, mothmites);
            }
            else
            {
                damage = BASE_BASH_DAMAGE;
                float delay = hardmode ? 0.25f : 0.35f;
                yield return DoAttackAll(mothmites, DoBash, delay, true);

                if (summonRoutine != null)
                {
                    yield return new WaitUntil(() => summonRoutine == null);
                    if (isAddingMothmite)
                        yield return battle.ChangePosition(battle.lastaddedid, BattlePosition.Ground);
                }

                yield return ChangeMothmitesPos(BattlePosition.Ground, mothmites);
            }

            foreach (var moth in mothmites)
            {
                //we end turn for each mothmites, but we skip our own cause its gonna get ended after anyway
                if (moth != actionid)
                {
                    battle.EndEnemyTurn(moth);
                }
            }
        }

        int[] FindAllMothmites()
        {
            List<int> mothmites = new List<int>();
            for (int i = 0; i < battle.enemydata.Length; i++)
            {
                if (battle.enemydata[i].animid == (int)NewEnemies.Mothmite && !battle.IsStopped(battle.enemydata[i]))
                {
                    mothmites.Add(i);
                }
            }
            return mothmites.ToArray();
        }

        IEnumerator ChangeMothmitesPos(BattleControl.BattlePosition pos, int[] mothmites)
        {
            foreach (var moth in mothmites)
            {
                battle.StartCoroutine(battle.ChangePosition(moth, pos));
            }
            yield return new WaitUntil(() => AllPos(pos, mothmites));
        }

        bool AllPos(BattleControl.BattlePosition pos, int[] mothmites)
        {
            foreach (var moth in mothmites)
            {
                if (battle.enemydata[moth].position != pos)
                    return false;
            }
            return true;
        }

        IEnumerator DoAttackAll(int[] mothmites, AttackDelegate attack, float delay, bool getTarget = false)
        {
            if (getTarget)
                battle.GetSingleTarget();

            for (int i = 0; i < mothmites.Length; i++)
            {
                int mothId = mothmites[i];
                battle.StartCoroutine(attack(battle.enemydata[mothId].battleentity, mothId));
                yield return new WaitForSeconds(delay);
            }
            yield return new WaitUntil(() => attackDone == mothmites.Length);
        }

        IEnumerator DoSplatterBall(EntityControl entity, EntityControl friend, int actionid)
        {
            if (battle.enemydata[actionid].position != BattlePosition.Flying)
            {
                yield return battle.ChangePosition(actionid, BattlePosition.Flying);
            }

            Vector3 startPos = entity.transform.position;
            Vector3 friendStartPos = friend.transform.position;

            Vector3 targetPos = new Vector3(0, 0, -0.9f);
            Vector3 friendPos = new Vector3(5, 0, -1);

            entity.MoveTowards(targetPos);
            friend.StartCoroutine(NewEvent.WaitMove(friend, friendPos, 1, 1, 110, false, Vector3.zero));
            yield return new WaitUntil(() => !entity.forcemove && !friend.forcemove);

            yield return battle.ChangePosition(actionid, BattlePosition.Ground);

            entity.flip = true;
            entity.overrideanim = true;
            entity.animstate = 103;
            entity.StartCoroutine(entity.ShakeSprite(0.1f, 60f));
            MainManager.PlaySound("Charge10", -1, 1.2f, 1f);
            yield return new WaitForSeconds(0.75f);

            entity.Jump(10);
            entity.animstate = 104;
            yield return EventControl.tenthsec;

            entity.animstate = 105;
            entity.LockRigid(true);
            MainManager.PlaySound("Jump");
            Vector3 friendTargetPos = friendPos + new Vector3(-0.5f, 0.5f, -0.1f);
            battle.StartCoroutine(MainManager.ArcMovement(entity.gameObject, entity.transform.position, friendTargetPos, Vector3.zero, 3, 30, false));

            friend.flip = false;

            int damage = SPLATTER_DMG;

            for (int i = 0; i < SPLATTER_AMOUNT; i++)
            {
                if (MainManager.GetAlivePlayerAmmount() == 0)
                    break;

                yield return new WaitForSeconds(0.45f);

                friend.animstate = 112;
                yield return new WaitForSeconds(0.06f);
                MainManager.PlaySound("Damage0", 1.2f, 1);
                friend.StartCoroutine(friend.SlowSpinStop(new Vector3(0, 20, 0), 20));

                battle.GetSingleTarget();
                targetPos = battle.playertargetentity.transform.position + new Vector3(0, 0f, -0.1f);
                MainManager.PlaySound("Damage2", 1.2f, 1);
                yield return MainManager.ArcMovement(entity.gameObject, entity.transform.position, targetPos, Vector3.zero, 3, 30, false);

                friend.animstate = 110;

                battle.DoDamage(actionid, battle.playertargetID, damage, null, battle.commandsuccess);
                BattleControl_Ext.Instance.DoConditionPierce(ref MainManager.instance.playerdata[battle.playertargetID],
                    BASH_STICKY_TURNS, MainManager.BattleCondition.Sticky, "StickyGet", "AhoneynationSpit", 1, 1,
                    battle.playertargetentity.transform.position, Vector3.one);

                MainManager.PlaySound("Damage2", 1.2f, 1);
                battle.StartCoroutine(MainManager.ArcMovement(entity.gameObject, entity.transform.position, friendTargetPos, Vector3.zero, 3, 30, false));
                damage++;
            }

            yield return MainManager.ArcMovement(entity.gameObject, entity.transform.position, startPos, Vector3.zero, 3, 30, false);
            entity.animstate = 0;

            entity.MoveTowards(startPos);
            friend.MoveTowards(friendStartPos);
            yield return new WaitUntil(() => !entity.forcemove && !friend.forcemove);
            friend.transform.position = friendStartPos;
            entity.transform.position = startPos;
        }

        IEnumerator DoBash(EntityControl entity, int actionid)
        {
            int[] mothmites = FindAllMothmites();
            if (mothmites.Length == 1 && jumpAntId == -1)
            {
                entity.animstate = 102;
                entity.StartCoroutine(entity.ShakeSprite(0.1f, 30));
                MainManager.PlaySound("StatUp");
                MainManager.SetCondition(MainManager.BattleCondition.AttackUp, ref battle.enemydata[actionid], 999999);
                MainManager.SetCondition(MainManager.BattleCondition.DefenseUp, ref battle.enemydata[actionid], 999999);
                battle.StartCoroutine(battle.StatEffect(battle.enemydata[actionid].battleentity, 1));
                battle.StartCoroutine(battle.StatEffect(battle.enemydata[actionid].battleentity, 0));
                yield return EventControl.halfsec;
            }

            int playerTargetId = battle.playertargetID;
            Vector3 targetPos = MainManager.instance.playerdata[playerTargetId].battleentity.transform.position + Vector3.up + Vector3.back * 0.1f;

            float baseHeight = entity.height;
            entity.height = 0f;
            entity.LockRigid(true);
            entity.transform.position = new Vector3(entity.transform.position.x, baseHeight, entity.transform.position.z);
            Vector3 startPos = entity.transform.position;
            MainManager.PlaySound("Toss13");
            entity.animstate = 100;
            yield return BattleControl_Ext.LerpPosition(35, entity.transform.position, new Vector3(15f, 8f) + MainManager.RandomVector(new Vector3(1, 1, 0)), entity.transform);

            entity.transform.position = new Vector3(10, 8, 0);
            entity.animstate = 101;
            if (mothmites.Length == 1 && battle.enemydata.Length < 4 && jumpAntId == -1)
            {
                EntityControl summon = EntityControl.CreateNewEntity("mothmite", (int)NewAnimID.Mothmite, new Vector3(10, 8, 0));
                summon.gameObject.layer = 9;
                summon.battle = true;
                yield return null;
                summon.LockRigid(true);
                summon.animstate = 101;
                summonRoutine = battle.StartCoroutine(SummonAttack(summon, playerTargetId, targetPos));
            }

            yield return BashAttack(entity, actionid, playerTargetId, targetPos);

            yield return BattleControl_Ext.LerpPosition(20, entity.transform.position, startPos, entity.transform);
            entity.overrideflip = false;
            entity.transform.position = new Vector3(entity.transform.position.x, 0f, entity.transform.position.z);
            entity.height = baseHeight;
            entity.animstate = 0;
            entity.LockRigid(false);
            yield return new WaitForSeconds(0.15f);
            attackDone++;
        }

        IEnumerator SummonAttack(EntityControl entity, int playerTargetId, Vector3 targetPos)
        {
            yield return new WaitForSeconds(0.4f);

            yield return EventControl.quartersec;
            MainManager.PlaySound("Toss14");
            yield return BattleControl_Ext.LerpPosition(30, entity.transform.position, targetPos, entity.transform);
            if (MainManager.instance.playerdata[playerTargetId].hp > 0)
            {
                battle.DoDamage(null, ref MainManager.instance.playerdata[playerTargetId], damage, null, battle.commandsuccess);
                BattleControl_Ext.Instance.DoConditionPierce(ref MainManager.instance.playerdata[playerTargetId], BASH_STICKY_TURNS, MainManager.BattleCondition.Sticky, "StickyGet", "AhoneynationSpit", 1, 1, battle.playertargetentity.transform.position, Vector3.one);

            }

            yield return new WaitForSeconds(0.15f);
            entity.overrideflip = true;
            entity.height = 0.2f;
            entity.animstate = 102;

            Vector3? freeSpace = battle.GetFreeSpace();
            if (freeSpace != null && fightComponent == null)
            {
                isAddingMothmite = true;
                yield return BattleControl_Ext.LerpPosition(20, entity.transform.position, freeSpace.Value, entity.transform);
                entity.overrideflip = false;
                entity.height = 2;
                entity.animstate = 0;

                battle.AddNewEnemy((int)NewEnemies.Mothmite, new Vector3(0f, 999f));
                yield return null;

                battle.enemydata[battle.lastaddedid].battleentity.transform.position = entity.transform.position;
                battle.enemydata[battle.lastaddedid].cantmove = 1;
                UnityEngine.Object.Destroy(entity.gameObject);

                yield return EventControl.tenthsec;
                MainManager.SetCondition((BattleCondition)NewCondition.Slugskin, ref battle.enemydata[battle.lastaddedid], 1);
                MainManager.PlaySound("Shield", 1.4f, 0.5f);
            }
            else
            {
                MainManager.PlaySound("Toss13");
                entity.animstate = 100;
                yield return BattleControl_Ext.LerpPosition(35, entity.transform.position, new Vector3(-15, 8f), entity.transform);
                UnityEngine.Object.Destroy(entity.gameObject);
            }

            summonRoutine = null;
        }

        IEnumerator BashAttack(EntityControl entity, int actionid, int playerTargetId, Vector3 targetPos)
        {
            yield return EventControl.quartersec;
            MainManager.PlaySound("Toss14");
            yield return BattleControl_Ext.LerpPosition(30, entity.transform.position, targetPos, entity.transform);
            if (MainManager.instance.playerdata[playerTargetId].hp > 0)
            {
                int bashDamage = damage;
                damage = battle.DoDamage(actionid, playerTargetId, bashDamage, null, battle.commandsuccess);
                BattleControl_Ext.Instance.DoConditionPierce(ref MainManager.instance.playerdata[playerTargetId], BASH_STICKY_TURNS, MainManager.BattleCondition.Sticky, "StickyGet", "AhoneynationSpit", 1, 1, battle.playertargetentity.transform.position, Vector3.one);
                MainManager.SetCondition((BattleCondition)NewCondition.Slugskin, ref battle.enemydata[actionid], 1);
                MainManager.PlaySound("Shield", 1.4f, 0.5f);
                damage++;
            }
            yield return new WaitForSeconds(0.15f);
            entity.overrideflip = true;
            entity.height = 0.2f;
            entity.animstate = 102;
        }

        IEnumerator DoDrainBubble(EntityControl entity, int actionid)
        {
            battle.nonphyscal = true;

            if (MainManager.GetAlivePlayerAmmount() == 0)
            {
                attackDone++;
                yield break;
            }

            int playerID = battle.GetRandomAvaliablePlayer();

            EntityControl playerEntity = MainManager.instance.playerdata[playerID].battleentity;

            entity.overrideanim = true;
            entity.animstate = 103;
            entity.StartCoroutine(entity.ShakeSprite(0.1f, 60f));
            MainManager.PlaySound("Charge10", -1, 1.2f, 1f);
            yield return new WaitForSeconds(0.75f);

            entity.Jump(10);
            entity.animstate = 104;
            yield return EventControl.tenthsec;
            entity.overrideanim = false;

            entity.animstate = 0;
            Vector3 targetPos = MainManager.instance.playerdata[playerID].battleentity.transform.position;
            yield return ThrowBubble(Color.gray, entity.transform.position + Vector3.up * 2f, targetPos, 60, 5, Vector3.one * 0.4f);

            int drainDamage = damage;
            damage = battle.DoDamage(actionid, playerID, drainDamage, null, battle.commandsuccess);
            damage++;

            Vector3 startPos = playerEntity.transform.position + Vector3.up * 2f;
            Vector3 endPos = playerEntity.transform.position + Vector3.up * 4f;
            int tpDrain = battle.commandsuccess ? BASE_DRAIN_BUBBLE_TP + 2 : BASE_DRAIN_BUBBLE_TP;
            BattleControl_Ext.Instance.RemoveTP(tpDrain, startPos, endPos);
            BattleControl_Ext.Instance.RecoverEnemyTp(-tpDrain, actionid);
            yield return new WaitForSeconds(0.75f);
            attackDone++;
        }

        delegate IEnumerator AttackDelegate(EntityControl entity, int actionid);
    }
}
