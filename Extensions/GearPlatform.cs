using UnityEngine;

namespace BFPlus.Extensions
{
    public class GearPlatform : MonoBehaviour
    {
        void LateUpdate()
        {
            foreach (Transform t in transform)
            {
                t.eulerAngles = Vector3.zero;
            }
        }
    }
}
