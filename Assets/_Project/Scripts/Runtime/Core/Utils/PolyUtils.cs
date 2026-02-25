using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Utils
{
    public static class PolyUtils
    {
        public static float SignedArea(List<Vector3> pts)
        {
            var n = pts.Count; 
            
            var area = 0f;
            
            for (int i = 0; i < n; i++)
            {
                var a = pts[i]; var b = pts[(i + 1) % n];
                area += a.x * b.y - b.x * a.y;
            }
            
            return area;
        }

        public static List<Vector3> CloneClosed(List<Vector3> src)
        {
            var r = new List<Vector3>(src);
            
            if (r.Count > 0 && (r[0] - r[^1]).sqrMagnitude > 1e-6f)
            {
                r.Add(r[0]);
            }

            return r;
        }

        /// <summary>
        /// Offsets a closed polygon outward (+) or inward (−) via miter normals.
        /// </summary>
        public static List<Vector3> Offset(List<Vector3> path, float offset)
        {
            var n = path.Count;
            if (n < 3 || Mathf.Abs(offset) < 1e-6f) return path;

            var ws  = WindingSign(path);
            var result = new List<Vector3>(n);

            for (int i = 0; i < n; i++)
            {
                var vA = Dir2D(path[(i - 1 + n) % n], path[i]);
                var vB = Dir2D(path[i], path[(i + 1) % n]);
                var nA = new Vector2(-vA.y, vA.x) * ws;
                var nB = new Vector2(-vB.y, vB.x) * ws;

                var miter = (nA + nB).normalized;
                var dot = Vector2.Dot(miter, nA);
                var len = dot > 1e-5f ? offset / dot : offset;

                var c = path[i];
                result.Add(new Vector3(c.x + miter.x * len, c.y + miter.y * len, c.z));
            }
            return result;
        }

        /// <summary>
        /// Rounds all corners of a closed loop.
        /// Convex → arc outward (CCW sweep), Concave → arc inward (CW sweep).
        /// </summary>
        public static List<Vector3> Rounded(List<Vector3> loop, float radius, int segs)
        {
            if (loop == null || loop.Count < 4 || radius <= 0f || segs < 1) return loop;

            const float eps = 1e-4f;
            
            var n = loop.Count - 1;
            
            var isCCW = SignedArea(loop) > 0f;
            
            var result = new List<Vector3>();

            for (int i = 0; i < n; i++)
            {
                var prev = loop[(i - 1 + n) % n];
                var curr = loop[i];
                var next = loop[(i + 1) % n];

                var vIn = curr - prev; float lenIn  = vIn.magnitude;
                var vOut = next - curr; float lenOut = vOut.magnitude;
                if (lenIn < 1e-6f || lenOut < 1e-6f) continue;

                var dirIn  = vIn  / lenIn;
                var dirOut = vOut / lenOut;
                var crossZ = Vector3.Cross(dirIn, dirOut).z;

                if (Mathf.Abs(crossZ) < eps)
                {
                    TryAdd(result, curr); 
                    continue;
                }

                var isConvex = isCCW ? crossZ > 0f : crossZ < 0f;
                var r = Mathf.Min(radius, lenIn * 0.5f, lenOut * 0.5f);

                var pIn = curr - dirIn  * r;
                var pOut = curr + dirOut * r;
                var center = curr + (-dirIn + dirOut) * r;

                var fromAngle = Mathf.Atan2((pIn  - center).y, (pIn  - center).x);
                var toAngle = Mathf.Atan2((pOut - center).y, (pOut - center).x);
                var delta = Mathf.DeltaAngle(fromAngle * Mathf.Rad2Deg, toAngle * Mathf.Rad2Deg) * Mathf.Deg2Rad;

                // Convex = CCW sweep (+), Concave = CW sweep (−)
                var sign = isConvex ? 1f : -1f;
                if (delta * sign < 0f) delta += sign * 2f * Mathf.PI;

                TryAdd(result, pIn);
                
                for (int s = 1; s < segs; s++)
                {
                    var ang = fromAngle + delta * (s / (float)segs);
                    result.Add(center + new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * r);
                }
                
                result.Add(pOut);
            }

            if (result.Count > 0) 
                result.Add(result[0]);
            
            return result;
        }

        /// <summary>
        /// Chamfers concave corners to avoid pipe mesh self-intersection.
        /// </summary>
        public static List<Vector3> TrimCorners(List<Vector3> closed, float trim)
        {
            if (closed == null || closed.Count < 4 || trim <= 0f) return closed;

            var n = closed.Count - 1;
            var isCCW = SignedArea(closed) > 0f;
            var result = new List<Vector3>(closed.Count);

            for (int i = 0; i < n; i++)
            {
                var prev = closed[(i - 1 + n) % n];
                var curr = closed[i];
                var next = closed[(i + 1) % n];

                var vIn = curr - prev; 
                var lenIn  = vIn.magnitude;
                
                var vOut = next - curr; 
                var lenOut = vOut.magnitude;
                
                if (lenIn < 1e-6f || lenOut < 1e-6f)
                {
                    result.Add(curr); 
                    continue;
                }

                var  dirIn = vIn  / lenIn;
                var  dirOut = vOut / lenOut;
                
                var isConvex = isCCW ? Vector3.Cross(dirIn, dirOut).z > 0f : Vector3.Cross(dirIn, dirOut).z < 0f;

                if (isConvex)
                {
                    result.Add(curr); 
                    continue;
                }

                var t = Mathf.Min(trim, lenIn * 0.45f, lenOut * 0.45f);
                result.Add(curr - dirIn  * t);
                result.Add(curr + dirOut * t);
            }

            if (result.Count > 0) result.Add(result[0]);
            return result;
        }

        public static Vector2 Dir2D(Vector3 from, Vector3 to) => new Vector2(to.x - from.x, to.y - from.y).normalized;
        
        private static void TryAdd(List<Vector3> list, Vector3 v)
        {
            if (list.Count == 0 || (list[^1] - v).sqrMagnitude > 1e-6f) 
                list.Add(v);
        }
        
        private static float WindingSign(List<Vector3> pts) => SignedArea(pts) >= 0f ? 1f : -1f;
    }
    
    public readonly struct Edge : IEquatable<Edge>
    {
        public readonly Vector2Int A, B;

        public Edge(Vector2Int a, Vector2Int b)
        {
            if (a.x < b.x || (a.x == b.x && a.y <= b.y)) { A = a; B = b; }
            else { A = b; B = a; }
        }

        public bool Equals(Edge o) => A.Equals(o.A) && B.Equals(o.B);
        public override bool Equals(object obj) => obj is Edge o && Equals(o);
        public override int  GetHashCode() { unchecked { return (A.GetHashCode() * 397) ^ B.GetHashCode(); } }
    }
}