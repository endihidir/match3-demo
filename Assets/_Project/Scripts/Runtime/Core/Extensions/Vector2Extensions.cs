using UnityEngine;

namespace Core.Extensions
{
    public static class Vector2Extensions
    {
        public static float Distance(this Vector2 vector, Vector2 pos)
        {
            var xDist = Mathf.Abs(vector.x - pos.x);
            var yDist = Mathf.Abs(vector.y - pos.y);

            return (xDist + yDist) / 2f;
        }
        
        public static Vector2 With(this Vector2 vector, float? x = null, float? y = null)
        {
            return new Vector2(x ?? vector.x, y ?? vector.y);
        }
        
        public static Vector2 Add(this Vector2 vector, float? x = null, float? y = null) 
        {
            return new Vector2(vector.x + (x ?? 0), vector.y + (y ?? 0));
        }
		
        public static T GetClosest<T>(this Vector2 v, T[] to) where T : MonoBehaviour
        {
            T closestT = null;

            var minDist = float.MaxValue;

            var targetsLength = to.Length;

            for (int i = 0; i < targetsLength; i++)
            {
                var pos = new Vector2(to[i].transform.position.x, to[i].transform.position.y);

                var potentialT = pos - v;

                if (potentialT.sqrMagnitude < minDist)
                {
                    minDist = potentialT.sqrMagnitude;

                    closestT = to[i];
                }
            }

            return closestT;
        }

    }
}