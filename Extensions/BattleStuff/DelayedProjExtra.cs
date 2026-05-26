using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static BattleControl;

namespace BFPlus.Extensions
{
    public class DelayedProjExtra : MonoBehaviour
    {
        public Func<int, int[], int, IEnumerator> extraAction;
        public int[] data;
        public DelayedProjectileData delProjData;
        public DelProjType type;
        public EntityControl targetEntity = null;
        public List<DamageOverride> overrides;
        public List<BattlePosition> possiblePositions = new List<BattlePosition>();

        public IEnumerator DoExtraEffect(int damageDone, int projId)
        {
            if (extraAction != null)
            {
                yield return extraAction(projId, data, damageDone);
            }
        }

        public static DelayedProjExtra AddDelayedProjExtra(GameObject obj, int[] data, Func<int, int[], int, IEnumerator> extraAction) => AddDelayedProjExtra(obj, data, extraAction, null);
        public static DelayedProjExtra AddDelayedProjExtra(GameObject obj, int[] data, Func<int, int[], int, IEnumerator> extraAction, List<DamageOverride> overrides)
        {
            DelayedProjExtra extra = obj.GetComponent<DelayedProjExtra>();
            if (extra != null)
            {
                if (extra.data == null)
                    extra.data = data;

                if (extra.extraAction == null)
                    extra.extraAction = extraAction;

                if (overrides == null) 
                    overrides = new List<DamageOverride>();

                if (extra.overrides == null)
                    extra.overrides = overrides;
                else
                    extra.overrides.AddRange(overrides);

                return extra;
            }

            GameObject delayedExtraObj = new GameObject("delayedExtra");
            delayedExtraObj.transform.parent = obj.transform;
            extra = delayedExtraObj.AddComponent<DelayedProjExtra>();
            extra.data = data;
            extra.extraAction = extraAction;

            if (overrides == null) 
                overrides = new List<DamageOverride>();
            overrides.Add((DamageOverride)NewDamageOverride.DelayedDamage);
            extra.overrides = overrides;

            return extra;
        }
    }
}