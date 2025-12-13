using System;
using System.Collections.Generic;
using Core.Utils;
using UnityEngine;
using GridLayout = Core.Grid.GridLayout;

namespace Core.Extensions
{
    public static class GridMeshExtensions
    { 
        /// <summary>
        /// Generates a mesh for the Match-3 board with holes (inactive cells).
        /// Submesh 0 = inner cell quads
        /// Submesh 1 = pipe-style frame along board borders and holes
        /// </summary>
        public static void BuildGridMesh(this in GridLayout gridLayout, Vector2Int size, MeshFilter meshFilter, float frameThickness = 0.1f, float cornerSmoothness = 0f, int cornerSegments = 8, Func<int, int, bool> isCellActive = null)
        {
            if (!meshFilter || frameThickness <= 0f) return;

            var mesh = new Mesh { name = "GridWithHolesMesh" };

            var cellSize = gridLayout.cellSize;
            
            var width = size.x;
            var height = size.y;
            
            var w = width * cellSize;
            var h = height * cellSize;

            var halfW = w * 0.5f;
            var halfH = h * 0.5f;

            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();
            var innerTris = new List<int>();
            var frameTris = new List<int>();

            if (isCellActive == null)
            {
                isCellActive = (x, y) => true;
            }

            // ---------- INNER CELL QUADS (SUBMESH 0) ----------

            for (int i = 0; i < height * width; i++)
            {
                var coordinate = CoordinateUtils.ToCoordinate(i, width);
                var x = coordinate.x;
                var y = coordinate.y;

                var visualY = height - 1 - y;
                var y0 = (visualY * cellSize) - halfH;
                var y1 = y0 + cellSize;

                if (!isCellActive(x, y)) continue;

                var x0 = (x * cellSize) - halfW;
                var x1 = x0 + cellSize;

                var baseIndex = vertices.Count;

                AddVertexBoard(new Vector3(x0, y0, 0f), new Vector2(x, y));
                AddVertexBoard(new Vector3(x1, y0, 0f), new Vector2(x + 1, y));
                AddVertexBoard(new Vector3(x1, y1, 0f), new Vector2(x + 1, y + 1));
                AddVertexBoard(new Vector3(x0, y1, 0f), new Vector2(x, y + 1));

                AddQuad(innerTris, baseIndex, baseIndex + 1, baseIndex + 2, baseIndex + 3);
            }

            // ---------- FRAME PATHS + PIPE BORDER (SUBMESH 1) ----------

            var rectPaths = BuildFramePaths(size, gridLayout, isCellActive);

            foreach (var rectPath in rectPaths)
            {
                var closedRect = CloneClosed(rectPath);

                List<Vector3> path;

                if (cornerSmoothness > 0f && cornerSegments > 0)
                {
                    var radius = frameThickness * cornerSmoothness;
                    path = BuildRoundedPath(closedRect, radius, cornerSegments);
                }
                else
                {
                    path = closedRect;
                }

                AddPipeMesh(path, frameThickness, cornerSegments, vertices, uvs, normals, frameTris);
            }

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.subMeshCount = 2;
            mesh.SetTriangles(innerTris, 0);
            mesh.SetTriangles(frameTris, 1);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            meshFilter.sharedMesh = mesh;
            return;

            void AddVertexBoard(Vector3 v, Vector2 uv)
            {
                vertices.Add(v);
                uvs.Add(uv);
                normals.Add(Vector3.back);
            }

            void AddQuad(List<int> tris, int v0, int v1, int v2, int v3)
            {
                tris.Add(v0); tris.Add(v1); tris.Add(v2);
                tris.Add(v0); tris.Add(v2); tris.Add(v3);
            }
        }

        private static List<List<Vector3>> BuildFramePaths(Vector2Int size, GridLayout gridLayout, Func<int, int, bool> isCellActive)
        {
            var result = new List<List<Vector3>>();

            var width = size.x;
            var height = size.y;
            var cellSize = gridLayout.cellSize;

            if (width <= 0 || height <= 0 || cellSize <= 0f) return result;

            var w = width * cellSize;
            var h = height * cellSize;
            var halfW = w * 0.5f;
            var halfH = h * 0.5f;

            var active = new bool[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    active[x, y] = isCellActive(x, y);
                }
            }

            bool IsActive(int x, int y)
            {
                if (x < 0 || x >= width || y < 0 || y >= height) return false;
                return active[x, y];
            }

            var edges = new HashSet<Edge>();
            var neighbors = new Dictionary<Vector2Int, List<Vector2Int>>();

            void AddEdge(Vector2Int c0, Vector2Int c1)
            {
                var e = new Edge(c0, c1);
                if (!edges.Add(e)) return;

                if (!neighbors.TryGetValue(c0, out var list0))
                {
                    list0 = new List<Vector2Int>();
                    neighbors[c0] = list0;
                }
                list0.Add(c1);

                if (!neighbors.TryGetValue(c1, out var list1))
                {
                    list1 = new List<Vector2Int>();
                    neighbors[c1] = list1;
                }
                list1.Add(c0);
            }
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!IsActive(x, y)) continue;

                    if (!IsActive(x, y - 1)) AddEdge(new Vector2Int(x, y),     new Vector2Int(x + 1, y));     // top
                    if (!IsActive(x, y + 1)) AddEdge(new Vector2Int(x, y + 1), new Vector2Int(x + 1, y + 1)); // bottom
                    if (!IsActive(x - 1, y)) AddEdge(new Vector2Int(x, y),     new Vector2Int(x, y + 1));     // left
                    if (!IsActive(x + 1, y)) AddEdge(new Vector2Int(x + 1, y), new Vector2Int(x + 1, y + 1)); // right
                }
            }

            if (edges.Count == 0) return result;

            var used = new HashSet<Edge>();

            foreach (var startEdge in edges)
            {
                if (used.Contains(startEdge)) continue;

                var loopCorners = new List<Vector2Int>();
                var start = startEdge.A;
                var current = start;
                var prev = startEdge.B;

                loopCorners.Add(start);
                used.Add(startEdge);

                while (true)
                {
                    if (!neighbors.TryGetValue(current, out var nextList) || nextList.Count == 0) break;

                    Vector2Int next;
                    if (nextList.Count == 1) next = nextList[0];
                    else next = nextList[0] == prev ? nextList[1] : nextList[0];

                    var e = new Edge(current, next);
                    if (!used.Add(e)) break;

                    prev = current;
                    current = next;

                    loopCorners.Add(current);
                    if (current == start) break;
                }

                if (loopCorners.Count < 3) continue;

                var loopWorld = new List<Vector3>(loopCorners.Count);
                for (int i = 0; i < loopCorners.Count; i++)
                {
                    var c = loopCorners[i];
                    var wx = c.x * cellSize - halfW;
                    var wy = halfH - c.y * cellSize;
                    loopWorld.Add(new Vector3(wx, wy, -.1f));
                }

                result.Add(loopWorld);
            }

            return result;
        }
        
        private readonly struct Edge : IEquatable<Edge>
        {
            public readonly Vector2Int A;
            public readonly Vector2Int B;

            public Edge(Vector2Int a, Vector2Int b)
            {
                if (a.x < b.x || (a.x == b.x && a.y <= b.y))
                {
                    A = a;
                    B = b;
                }
                else
                {
                    A = b;
                    B = a;
                }
            }

            public bool Equals(Edge other) => A.Equals(other.A) && B.Equals(other.B);
            public override bool Equals(object obj) => obj is Edge other && Equals(other);
            public override int GetHashCode()
            {
                unchecked { return (A.GetHashCode() * 397) ^ B.GetHashCode(); }
            }
        }

        private static List<Vector3> CloneClosed(List<Vector3> src)
        {
            var result = new List<Vector3>(src);
            if (result.Count == 0) return result;

            if ((result[0] - result[^1]).sqrMagnitude > 1e-6f)
            {
                result.Add(result[0]);
            }

            return result;
        }
        
        private static List<Vector3> TrimCorners(List<Vector3> closed, float trim)
        {
            if (closed == null || closed.Count < 4 || trim <= 0f) return closed;

            int n = closed.Count - 1;
            
            float area = 0f;
            for (int i = 0; i < n; i++)
            {
                var p0 = closed[i];
                var p1 = closed[(i + 1) % n];
                area += (p0.x * p1.y - p1.x * p0.y);
            }
            bool isCCW = area > 0f;

            var result = new List<Vector3>(closed.Count);

            for (int i = 0; i < n; i++)
            {
                var prev = closed[(i - 1 + n) % n];
                var curr = closed[i];
                var next = closed[(i + 1) % n];

                var vIn = curr - prev;
                var vOut = next - curr;

                float lenIn = vIn.magnitude;
                float lenOut = vOut.magnitude;

                if (lenIn < 1e-6f || lenOut < 1e-6f)
                {
                    result.Add(curr);
                    continue;
                }

                var dirIn = vIn / lenIn;
                var dirOut = vOut / lenOut;
                
                float crossZ = Vector3.Cross(dirIn, dirOut).z;
                bool isConvex = isCCW ? crossZ > 0f : crossZ < 0f;
                
                if (isConvex)
                {
                    result.Add(curr);
                    continue;
                }

                float t = Mathf.Min(trim, lenIn * 0.45f, lenOut * 0.45f);

                var pIn = curr - dirIn * t;
                var pOut = curr + dirOut * t;

                result.Add(pIn);
                result.Add(pOut);
            }

            if (result.Count > 0)
                result.Add(result[0]);

            return result;
        }

        private static List<Vector3> BuildRoundedPath(List<Vector3> closedLoop, float radius, int cornerSegments)
        {
            var result = new List<Vector3>();

            if (closedLoop == null || closedLoop.Count < 4 || radius <= 0f || cornerSegments < 1) return closedLoop;
            
            var n = closedLoop.Count - 1;
            
            float area = 0f;
            for (int i = 0; i < n; i++)
            {
                var p0 = closedLoop[i];
                var p1 = closedLoop[(i + 1) % n];
                area += (p0.x * p1.y - p1.x * p0.y);
            }
            var isCCW = area > 0f;

            const float eps = 1e-4f;

            for (int i = 0; i < n; i++)
            {
                var prev = closedLoop[(i - 1 + n) % n];
                var curr = closedLoop[i];
                var next = closedLoop[(i + 1) % n];

                var vIn = curr - prev;
                var vOut = next - curr;

                var lenIn = vIn.magnitude;
                var lenOut = vOut.magnitude;

                if (lenIn < 1e-6f || lenOut < 1e-6f) continue;

                var dirIn = vIn / lenIn;
                var dirOut = vOut / lenOut;

                var crossZ = Vector3.Cross(dirIn, dirOut).z;
                
                if (Mathf.Abs(crossZ) < eps)
                {
                    if (result.Count == 0 || (result[^1] - curr).sqrMagnitude > 1e-6f)
                        result.Add(curr);
                    
                    continue;
                }
                
                var isConvex = isCCW ? crossZ > 0f : crossZ < 0f;

                if (!isConvex)
                {
                    if (result.Count == 0 || (result[^1] - curr).sqrMagnitude > 1e-6f)
                        result.Add(curr);
                    
                    continue;
                }
                
                var r = Mathf.Min(radius, lenIn * 0.5f, lenOut * 0.5f);

                var pIn = curr - dirIn * r;
                var pOut = curr + dirOut * r;

                var center = curr + (-dirIn + dirOut) * r;

                var fromDir = (pIn - center).normalized;
                var toDir = (pOut - center).normalized;

                var fromAngle = Mathf.Atan2(fromDir.y, fromDir.x);
                var toAngle = Mathf.Atan2(toDir.y, toDir.x);

                var delta = Mathf.DeltaAngle(fromAngle * Mathf.Rad2Deg, toAngle * Mathf.Rad2Deg) * Mathf.Deg2Rad;
                if (delta < 0f) delta += 2f * Mathf.PI;

                if (result.Count == 0 || (result[^1] - pIn).sqrMagnitude > 1e-6f)
                    result.Add(pIn);

                for (int s = 1; s < cornerSegments; s++)
                {
                    var t = s / (float)cornerSegments;
                    var ang = fromAngle + delta * t;
                    var dir = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f);
                    var p = center + dir * r;
                    result.Add(p);
                }

                result.Add(pOut);
            }

            if (result.Count > 0 && (result[0] - result[^1]).sqrMagnitude > 1e-6f)
            {
                result.Add(result[0]);
            }

            return result;
        }
        
        private static void AddPipeMesh(List<Vector3> path, float thickness, int cornerSegments, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> tris)
        {
            if (path == null || path.Count < 2) return;

            float half = thickness * 0.5f;
            cornerSegments = Mathf.Max(1, cornerSegments);

            var pts = CloneClosed(path);
            pts = TrimCorners(pts, half * 0.5f);
            int n = pts.Count;
            
            List<Vector3> left = new List<Vector3>();
            List<Vector3> right = new List<Vector3>();

            for (int i = 0; i < n; i++)
            {
                Vector3 pPrev = pts[(i - 1 + n) % n];
                Vector3 pNow  = pts[i];
                Vector3 pNext = pts[(i + 1) % n];

                Vector2 vA = (new Vector2(pNow.x - pPrev.x, pNow.y - pPrev.y)).normalized;
                Vector2 vB = (new Vector2(pNext.x - pNow.x, pNext.y - pNow.y)).normalized;

                if (vA.sqrMagnitude < 1e-6f) vA = vB;
                if (vB.sqrMagnitude < 1e-6f) vB = vA;

                Vector2 nA = new Vector2(-vA.y, vA.x);
                Vector2 nB = new Vector2(-vB.y, vB.x);

                float angle = Mathf.Acos(Mathf.Clamp(Vector2.Dot(nA, nB), -1f, 1f));
                
                if (angle < 0.01f)
                {
                    Vector2 m = (nA + nB).normalized * half;
                    AddJoinPoint(pNow, m);
                    continue;
                }
                
                for (int s = 0; s <= cornerSegments; s++)
                {
                    float t = s / (float)cornerSegments;
                    Vector2 m = Vector2.Lerp(nA, nB, t).normalized * half;
                    AddJoinPoint(pNow, m);
                }
            }

            void AddJoinPoint(Vector3 p, Vector2 normal2D)
            {
                Vector3 n3 = new Vector3(normal2D.x, normal2D.y, 0f);
                left.Add(p + n3);
                right.Add(p - n3);
            }
            
            int baseIndex = vertices.Count;

            for (int i = 0; i < left.Count; i++)
            {
                vertices.Add(left[i]);
                vertices.Add(right[i]);

                uvs.Add(new Vector2(0, i));
                uvs.Add(new Vector2(1, i));

                normals.Add(Vector3.back);
                normals.Add(Vector3.back);
            }

            for (int i = 0; i < left.Count - 1; i++)
            {
                int i0 = baseIndex + i * 2;
                int i1 = i0 + 1;
                int i2 = i0 + 2;
                int i3 = i0 + 3;

                tris.Add(i0); tris.Add(i2); tris.Add(i1);
                tris.Add(i1); tris.Add(i2); tris.Add(i3);
            }
        }
        
        /// <summary>
        /// Generates a mesh for the Match-3 board with holes (inactive cells).
        /// Submesh 0 = inner cell quads
        /// Submesh 1 = outer frame segments + corners
        /// </summary>
        public static void BuildGridMeshLegacy(this in GridLayout gridLayout, Vector2Int size, MeshFilter meshFilter, float frameThickness = 0.1f, float cornerSmoothness = 0f, int cornerSegments = 6, Func<int, int, bool> isCellActive = null)
        {
            if (!meshFilter || frameThickness <= 0f) return;

            var mesh = new Mesh { name = "GridMesh" };

            var cellSize = gridLayout.cellSize;
            
            var width = size.x;
            var height = size.y;

            var w = width * cellSize;
            var h = height * cellSize;

            var halfW = w * 0.5f;
            var halfH = h * 0.5f;

            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();
            var innerTris = new List<int>();
            var frameTris = new List<int>();

            var useRounded = cornerSmoothness > 0f && cornerSegments > 0;

            for (int i = 0; i < height * width; i++)
            {
                var coordinate = CoordinateUtils.ToCoordinate(i, width);
                var x = coordinate.x;
                var y = coordinate.y;
            
                var visualY = height - 1 - y;
                var y0 = (visualY * cellSize) - halfH;
                var y1 = y0 + cellSize;
                
                if (isCellActive != null && !isCellActive(x, y)) continue;

                var x0 = (x * cellSize) - halfW;
                var x1 = x0 + cellSize;

                var baseIndex = vertices.Count;

                AddVertex(new Vector3(x0, y0, 0f), new Vector2(x, y));
                AddVertex(new Vector3(x1, y0, 0f), new Vector2(x + 1, y));
                AddVertex(new Vector3(x1, y1, 0f), new Vector2(x + 1, y + 1));
                AddVertex(new Vector3(x0, y1, 0f), new Vector2(x, y + 1));

                AddQuad(innerTris, baseIndex, baseIndex + 1, baseIndex + 2, baseIndex + 3);
                
                // Edge strips
                if (IsEmpty(x, y + 1))
                {
                    AddFrameEdge(frameTris, new Vector3(x0, y0, 0f), new Vector3(x1, y0, 0f), Vector3.down);
                }

                if (IsEmpty(x, y - 1))
                {
                    AddFrameEdge(frameTris, new Vector3(x0, y1, 0f), new Vector3(x1, y1, 0f), Vector3.up);
                }

                if (IsEmpty(x - 1, y))
                {
                    AddFrameEdge(frameTris, new Vector3(x0, y0, 0f), new Vector3(x0, y1, 0f), Vector3.left);
                }

                if (IsEmpty(x + 1, y))
                {
                    AddFrameEdge(frameTris, new Vector3(x1, y0, 0f), new Vector3(x1, y1, 0f), Vector3.right);
                }
                
                // Corners (compressed via TryAddCorner)
                TryAddCorner(frameTris, x, y, -1, 0, 0, 1, new Vector3(x0, y0, 0f), Vector3.left,  Vector3.down); // top-left
                TryAddCorner(frameTris, x, y,  1, 0, 0, 1, new Vector3(x1, y0, 0f), Vector3.right, Vector3.down); // top-right
                TryAddCorner(frameTris, x, y, -1, 0, 0,-1, new Vector3(x0, y1, 0f), Vector3.left,  Vector3.up);   // bottom-left
                TryAddCorner(frameTris, x, y,  1, 0, 0,-1, new Vector3(x1, y1, 0f), Vector3.right, Vector3.up);   // bottom-right
                
            }

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.subMeshCount = 2;
            mesh.SetTriangles(innerTris, 0);
            mesh.SetTriangles(frameTris, 1);
            mesh.RecalculateBounds();
            meshFilter.sharedMesh = mesh;
            return;

            // --------- helpers ---------

            void AddVertex(Vector3 v, Vector2 uv)
            {
                vertices.Add(v);
                uvs.Add(uv);
                normals.Add(Vector3.back);
            }

            void AddQuad(List<int> tris, int v0, int v1, int v2, int v3)
            {
                tris.Add(v0); tris.Add(v1); tris.Add(v2);
                tris.Add(v0); tris.Add(v2); tris.Add(v3);
            }

            /// <summary>
            /// Generic edge strip between two inner points, extruded along outwardDir by frameThickness.
            /// innerStart -> innerEnd defines the inner edge on the board.
            /// </summary>
            void AddFrameEdge(List<int> tris, Vector3 innerStart, Vector3 innerEnd, Vector3 outwardDir)
            {
                var n = outwardDir.normalized * frameThickness;

                var vBase = vertices.Count;
                var outerStart = innerStart + n;
                var outerEnd = innerEnd + n;

                AddVertex(outerStart, Vector2.zero);
                AddVertex(outerEnd, Vector2.right);
                AddVertex(innerEnd, Vector2.one);
                AddVertex(innerStart, Vector2.up);

                AddQuad(tris, vBase, vBase + 1, vBase + 2, vBase + 3);
            }

            /// <summary>
            /// Corner wrapper: checks neighbors and calls AddCorner if both sides are empty.
            /// dx1,dy1 and dx2,dy2 are neighbor offsets (e.g. (-1,0) and (0,1) for top-left).
            /// </summary>
            void TryAddCorner(List<int> tris, int gx, int gy, int dx1, int dy1, int dx2, int dy2, Vector3 innerCorner, Vector3 dirX, Vector3 dirY)
            {
                var nx1 = gx + dx1;
                var ny1 = gy + dy1;

                var nx2 = gx + dx2;
                var ny2 = gy + dy2;
                
                if (!IsEmpty(nx1, ny1) || !IsEmpty(nx2, ny2)) return;
                
                var diagX = gx + dx1 + dx2;
                var diagY = gy + dy1 + dy2;
                
                if (!IsEmpty(diagX, diagY)) return;
                
                AddCorner(tris, innerCorner, dirX, dirY);
            }

            /// <summary>
            /// Unified corner entry: decides square vs rounded based on useRounded.
            /// innerCorner = cell corner on the board surface.
            /// dirX, dirY = outward directions along each edge from that corner.
            /// </summary>
            void AddCorner(List<int> tris, Vector3 innerCorner, Vector3 dirX, Vector3 dirY)
            {
                if (useRounded)
                    AddRoundedCorner(tris, innerCorner, dirX, dirY);
                else
                    AddSquareCorner(tris, innerCorner, dirX, dirY);
            }

            void AddSquareCorner(List<int> tris, Vector3 innerCorner, Vector3 dirX, Vector3 dirY)
            {
                var ex = dirX.normalized * frameThickness;
                var ey = dirY.normalized * frameThickness;

                var vBase = vertices.Count;

                AddVertex(innerCorner + ex + ey, Vector2.zero);
                AddVertex(innerCorner + ey, Vector2.right);
                AddVertex(innerCorner, Vector2.one);
                AddVertex(innerCorner + ex, Vector2.up);

                AddQuad(tris, vBase, vBase + 1, vBase + 2, vBase + 3);
            }

            void AddRoundedCorner(List<int> tris, Vector3 center, Vector3 dirX, Vector3 dirY)
            {
                var r = frameThickness * cornerSmoothness;
                if (r <= 0f) return;

                var dx = dirX.normalized;
                var dy = dirY.normalized;

                var centerIndex = vertices.Count;
                AddVertex(center, Vector2.zero);

                var prevIndex = -1;

                for (int i = 0; i <= cornerSegments; i++)
                {
                    var t = i / (float)cornerSegments;
                    var angle = t * 0.5f * Mathf.PI;
                    var offset = (dx * Mathf.Cos(angle) + dy * Mathf.Sin(angle)) * r;
                    var idx = vertices.Count;
                    AddVertex(center + offset, Vector2.zero);

                    if (i > 0)
                    {
                        tris.Add(centerIndex);
                        tris.Add(prevIndex);
                        tris.Add(idx);
                    }

                    prevIndex = idx;
                }
            }

            bool IsEmpty(int gx, int gy)
            {
                if (gx < 0 || gx >= width || gy < 0 || gy >= height) return true;

                return isCellActive != null && !isCellActive(gx, gy);
            }
        }
    }
}