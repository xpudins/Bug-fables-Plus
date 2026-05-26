using System.Collections.Generic;
using UnityEngine;

namespace BFPlus.Extensions.BattleStuff
{
    public struct BattleDataExtra
    {
        private int _dizzyRes;
        private int _maxCharge;
        public Vector3 dizzyStarOffset;
        public Vector3 slugskinOffset;
        public Vector3 slugskinScale;
        public Vector3 vitiationOffset;
        public Vector3 vitationScale;
        public Vector3 inkBubbleOffset;
        public Vector3 inkBubbleScale;
        public bool noDizzySpinAnim;
        public List<int> dizzyAfter;

        public int DizzyRes { get => _dizzyRes; set => _dizzyRes = Mathf.Clamp(value, 0, 999); }
        public int MaxCharge { get => _maxCharge; set => _maxCharge = Mathf.Clamp(value, 0, 999); }
    }
}
