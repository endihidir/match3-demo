using UnityEngine;

namespace Core.Extensions
{
    public static class QuaternionExtensions
    {
        public static Quaternion LookAt2D(this Quaternion q, Vector3 startPos, Vector3 endPos)
        {
            var dir = endPos - startPos;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            return Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}