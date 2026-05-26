using InputIOManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MainManager;
using static UnityEngine.Object;

namespace BFPlus.Extensions.BattleStuff.NewCommands
{
    public class CursorCommand
    {
        public static int commandTarget = -1;

        public static IEnumerator DoCursorCommand(BattleData[] targets, float frameTime, Vector2 boundsOffset, float moveSpeed, float reticleSpinSpeed)
        {
            commandTarget = -1;
            if (targets == null || targets.Length == 0)
                yield break;

            SpriteRenderer reticle = NewUIObject("cursor", null, Vector3.one, Vector3.one, guisprites[41]).GetComponent<SpriteRenderer>();
            SpinAround reticleSpin = reticle.gameObject.AddComponent<SpinAround>();

            Vector2 minBounds = default;
            Vector2 maxBounds;

            List<Vector2> avg = new List<Vector2>();
            List<SpriteRenderer> crosshairs = new List<SpriteRenderer>();
            if (targets.Length == 1)
            {
                crosshairs.Add(battle.TempCrosshair(targets[0], false).GetComponent<SpriteRenderer>());
                maxBounds = minBounds = crosshairs[0].transform.position;
            }
            else
            {
                for (int i = targets.Length - 1; i >= 0; i--)
                {
                    crosshairs.Insert(0, battle.TempCrosshair(targets[i], false).GetComponent<SpriteRenderer>());
                    crosshairs[0].transform.localScale = Vector3.one * 1.5f;
                    avg.Add(crosshairs[0].transform.position);
                    minBounds += (Vector2)crosshairs[0].transform.position;
                }
                maxBounds = minBounds /= avg.Count;
                for (int i = 0; i < avg.Count; i++)
                {
                    if (avg[i].x < minBounds.x)
                        minBounds.x = avg[i].x;
                    if (avg[i].y < minBounds.y)
                        minBounds.y = avg[i].y;
                    if (avg[i].x > maxBounds.x)
                        maxBounds.x = avg[i].x;
                    if (avg[i].y > maxBounds.y)
                        maxBounds.y = avg[i].y;
                }
            }

            minBounds -= boundsOffset;
            maxBounds += boundsOffset;
            Vector2 reticleVel = Vector2.zero;
            Vector2 reticlePos = reticle.transform.position;
            float friction = 0.97f;
            float range = 1f;

            float a = 0;
            PlaySound("Crosshair", 9, 0.9f, 0.35f, true);
            do
            {
                float inputX = 0;
                float inputY = 0;
                Vector2 lastaxis = new Vector3(InputIO.JoyStick(0), InputIO.JoyStick(1));
                if (GetKey(2, true))
                {
                    inputX = -1;
                    if (lastaxis.x != 0f)
                        inputX = lastaxis.x;
                }
                else if (GetKey(3, true))
                {
                    inputX = 1;
                    if (lastaxis.x != 0f)
                        inputX = lastaxis.x;
                }
                if (GetKey(0, true))
                {
                    inputY = 1;
                    if (lastaxis.y != 0f)
                        inputY = -lastaxis.y;
                }
                else if (GetKey(1, true))
                {
                    inputY = -1;
                    if (lastaxis.y != 0f)
                        inputY = -lastaxis.y;
                }

                if (Mathf.Abs(inputX) > 0.01f || Mathf.Abs(inputY) > 0.01f)
                    reticleVel += new Vector2(inputX, inputY) * moveSpeed * Time.smoothDeltaTime;

                reticleVel *= Mathf.Pow(friction, Time.deltaTime * 60f);
                reticlePos += reticleVel * Time.deltaTime * 60f;

                reticlePos.x = Mathf.Clamp(reticlePos.x, minBounds.x, maxBounds.x);
                reticlePos.y = Mathf.Clamp(reticlePos.y, -2, maxBounds.y);
                reticle.transform.position = reticlePos;

                commandTarget = -1;
                for (int i = crosshairs.Count - 1; i >= 0; i--)
                {
                    if (Vector2.Distance(reticle.transform.position, crosshairs[i].transform.position) <= range)
                    {
                        crosshairs[i].color = Color.green;

                        // prioritizes targets that are earlier in the loadout, if there's ever an overlap somehow
                        commandTarget = targets[i].battleentity.battleid;
                    }
                    else
                    {
                        crosshairs[i].color = Color.white;
                    }
                }

                float retProg = a / frameTime;
                reticle.color = commandTarget != -1 ? Color.green : Color.white;
                reticleSpin.itself.z = 15f * reticleSpinSpeed * (1f - Mathf.Pow(retProg, 3f));

                sounds[9].pitch = Mathf.Lerp(0.9f, 1.2f, Mathf.Pow(retProg, 4f));

                a += TieFramerate(1f);
                yield return null;
            }
            while (a < frameTime);

            battle.commandsuccess = commandTarget > -1;
            StopSound(9);
            Destroy(reticle.gameObject);
            for (int i = crosshairs.Count - 1; i >= 0; i--)
            {
                Destroy(crosshairs[i].gameObject);
            }
        }
    }
}
