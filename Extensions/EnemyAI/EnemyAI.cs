using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BFPlus.Extensions.EnemyAI
{
    public abstract class AI
    {
        public abstract IEnumerator DoBattleAI(EntityControl entity, int actionid);

        static private Dictionary<NewEnemies, Type> EnemyTypeToAI = new Dictionary<NewEnemies, Type>()
        {
            {NewEnemies.DarkVi, typeof(DarkViAI) },
            {NewEnemies.DarkKabbu, typeof(DarkKabbuAI) },
            {NewEnemies.DarkLeif, typeof(DarkLeifAI) },
            {NewEnemies.Worm, typeof(WormAI) },
            {NewEnemies.WormSwarm, typeof(WormAI) },
            {NewEnemies.Dewling, typeof(DewlingAI) },
            {NewEnemies.FireAnt, typeof(FireAntAI) },
            {NewEnemies.Belosslow, typeof(BelosslowAI) },
            {NewEnemies.DynamoSpore, typeof(DynamoSporeAI) },
            {NewEnemies.BatteryShroom, typeof(BatteryShroomAI) },
            {NewEnemies.DullScorp, typeof(DullScorpAI) },
            {NewEnemies.Mars, typeof(MarsAI) },
            {NewEnemies.MarsSprout, typeof(MarsSproutAI) },
            {NewEnemies.Levi, typeof(LeviAI) },
            {NewEnemies.Celia, typeof(CeliaAI) },
            {NewEnemies.Mothmite, typeof(MothmiteAI) },
            {NewEnemies.MarsBud, typeof(MarsBudAI) },
            {NewEnemies.TermiteKnight, typeof(TermiteKnightAI) },
            {NewEnemies.LeafbugShaman, typeof(LeafbugShamanAI) },
            {NewEnemies.Jester, typeof(JesterAI) },
            {NewEnemies.IronSuit, typeof(IronSuitAI) },
            {NewEnemies.FirePopper, typeof(FirePopperAI) },
            {NewEnemies.Patton, typeof(PattonAI) },
            {NewEnemies.LonglegsSpider, typeof(LongLegsSpiderAI) },
            {NewEnemies.JumpAnt, typeof(JumpAntAI) },
            {NewEnemies.Frostfly, typeof(FrostflyAI) },
            {NewEnemies.Caveling, typeof(CavelingAI) },
            {NewEnemies.SplotchSpider, typeof(SplotchSpiderAI) },
            {NewEnemies.Moeruki, typeof(MoerukiAI) },
            {NewEnemies.MechaJaw, typeof(MechaJawAI) },
        };

        public static AI GetAI(NewEnemies enemyType)
        {
            if (EnemyTypeToAI.TryGetValue(enemyType, out var enemyClassType))
            {
                return (AI)Activator.CreateInstance(enemyClassType);
            }
            throw new ArgumentException($"No enemy ai class found for enemy type {enemyType}");
        }

        public static bool HasCustomAI(NewEnemies enemyType)
        {
            return EnemyTypeToAI.ContainsKey(enemyType);
        }

        public static IEnumerator ThrowBubble(Color color, Vector3 startPos, Vector3 targetPos, float frameTime, float height, Vector3 scale)
        {
            GameObject bubble = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Objects/WaterBubble"), startPos, Quaternion.identity) as GameObject;
            bubble.transform.localScale = scale;
            bubble.GetComponent<SpriteBounce>().startscale = true;
            bubble.GetComponent<SpriteRenderer>().color = color;

            Vector3 bubbleStartPos = bubble.transform.position;
            MainManager.PlaySound("Blosh");
            MainManager.PlaySound("Wub", 9, 0.8f, 1f, true);

            float a = 0f;
            while (a < frameTime)
            {
                bubble.transform.position = MainManager.BeizierCurve3(bubbleStartPos, targetPos, height, a / frameTime);
                a += MainManager.TieFramerate(1f);
                yield return null;
            }
            UnityEngine.Object.Destroy(bubble);
            MainManager.StopSound(9);
            MainManager.PlaySound("BubbleBurst");
            MainManager.PlayParticle("WaterSplash", targetPos + Vector3.up)
                .GetComponent<ParticleSystemRenderer>().material.color = color;
        }
    }
}
