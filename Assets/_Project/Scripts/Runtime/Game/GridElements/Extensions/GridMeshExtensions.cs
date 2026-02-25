using System;
using System.Collections.Generic;
using Core.Utils;
using Game.Utils;
using UnityEngine;
using GridLayout = Core.Grid.GridLayout;

namespace Game.Extensions
{
    public static class GridMeshExtensions
    {
        /// <summary>
        /// Generates a mesh for the Match-3 board with holes (inactive cells).
        /// Submesh 0 = inner cell quads
        /// Submesh 1 = pipe-style frame along board borders and holes
        /// </summary>
        public static void BuildGridMesh(this in GridLayout gridLayout, Vector2Int size, MeshFilter meshFilter, float frameThickness = 0.1f, float frameOffset = 0f, float cornerSmoothness = 0f, int cornerSegments = 8, Func<int, int, bool> isCellActive = null)
        {
            if (!meshFilter || frameThickness <= 0f) return;

            isCellActive ??= (_, _) => true;

            var cellSize = gridLayout.cellSize;
            var (width, height) = (size.x, size.y);
            var halfW = width  * cellSize * 0.5f;
            var halfH = height * cellSize * 0.5f;

            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var norms = new List<Vector3>();
            var innerTris = new List<int>();
            var frameTris = new List<int>();

            // ── Submesh 0: cell quads ──────────────────────────────────
            for (int i = 0; i < height * width; i++)
            {
                var coord  = GridIndexUtil.ToCoord(i, width);
                var (x, y) = (coord.x, coord.y);
                if (!isCellActive(x, y)) continue;

                var x0   = x * cellSize - halfW;
                var y0   = (height - 1 - y) * cellSize - halfH;
                var base4 = verts.Count;

                AddVert(new Vector3(x0, y0, 0f), new Vector2(x, y));
                AddVert(new Vector3(x0 + cellSize, y0, 0f), new Vector2(x + 1, y));
                AddVert(new Vector3(x0 + cellSize, y0 + cellSize, 0f), new Vector2(x + 1, y + 1));
                AddVert(new Vector3(x0, y0 + cellSize, 0f), new Vector2(x, y + 1));
                AddQuad(innerTris, base4);
            }
            
            var radius = cornerSmoothness > 0f && cornerSegments > 0 ? frameThickness * cornerSmoothness : 0f;

            foreach (var rawPath in BuildFramePaths(size, gridLayout, isCellActive))
            {
                var path = frameOffset != 0f ? PolyUtils.Offset(rawPath, frameOffset) : rawPath;
                
                path = PolyUtils.CloneClosed(path);
                
                if (radius > 0f) path = PolyUtils.Rounded(path, radius, cornerSegments);
                
                AddPipeMesh(path, frameThickness, cornerSegments, verts, uvs, norms, frameTris);
            }
            
            var mesh = new Mesh { name = "GridWithHolesMesh" };
            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.subMeshCount = 2;
            mesh.SetTriangles(innerTris, 0);
            mesh.SetTriangles(frameTris, 1);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            meshFilter.sharedMesh = mesh;
            return;
            
            void AddVert(Vector3 v, Vector2 uv) { verts.Add(v); uvs.Add(uv); norms.Add(Vector3.back); }
            void AddQuad(List<int> t, int b) { t.Add(b); t.Add(b+1); t.Add(b+2); t.Add(b); t.Add(b+2); t.Add(b+3); }
        }

        private static List<List<Vector3>> BuildFramePaths(Vector2Int size, GridLayout gridLayout, Func<int, int, bool> isCellActive)
        {
            var result = new List<List<Vector3>>();
            var (w, h) = (size.x, size.y);
            var cellSize = gridLayout.cellSize;
            if (w <= 0 || h <= 0 || cellSize <= 0f) return result;

            var halfW = w * cellSize * 0.5f;
            var halfH = h * cellSize * 0.5f;

            var active = new bool[w, h];
            
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    active[x, y] = isCellActive(x, y);
                }
            }

            var edges = new HashSet<Edge>();
            var neighbors = new Dictionary<Vector2Int, List<Vector2Int>>();

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (!IsActive(x, y)) continue;
                    if (!IsActive(x, y - 1)) AddEdge(new Vector2Int(x, y), new Vector2Int(x + 1, y));
                    if (!IsActive(x, y + 1)) AddEdge(new Vector2Int(x, y + 1), new Vector2Int(x + 1, y + 1));
                    if (!IsActive(x - 1, y)) AddEdge(new Vector2Int(x, y), new Vector2Int(x, y + 1));
                    if (!IsActive(x + 1, y)) AddEdge(new Vector2Int(x + 1, y), new Vector2Int(x + 1, y + 1));
                }
            }

            if (edges.Count == 0) return result;

            var used = new HashSet<Edge>();

            foreach (var startEdge in edges)
            {
                if (!used.Add(startEdge)) continue;

                var loop = new List<Vector2Int> { startEdge.A };
                var (current, prev) = (startEdge.A, startEdge.B);

                while (neighbors.TryGetValue(current, out var nexts))
                {
                    var next = nexts.Count == 1 ? nexts[0] : nexts[0] == prev  ? nexts[1] : nexts[0];
                    if (!used.Add(new Edge(current, next))) break;
                    (prev, current) = (current, next);
                    loop.Add(current);
                    if (current == startEdge.A) break;
                }

                if (loop.Count < 3) continue;

                var world = new List<Vector3>(loop.Count);
                
                foreach (var c in loop)
                    world.Add(new Vector3(c.x * cellSize - halfW, halfH - c.y * cellSize, -.1f));
                
                result.Add(world);
            }

            return result;

            void AddEdge(Vector2Int a, Vector2Int b)
            {
                if (!edges.Add(new Edge(a, b))) return;
                GetOrAdd(neighbors, a).Add(b);
                GetOrAdd(neighbors, b).Add(a);
            }

            bool IsActive(int x, int y) => x >= 0 && x < w && y >= 0 && y < h && active[x, y];

            static List<Vector2Int> GetOrAdd(Dictionary<Vector2Int, List<Vector2Int>> d, Vector2Int k)
            {
                if (!d.TryGetValue(k, out var list)) d[k] = list = new List<Vector2Int>();
                return list;
            }
        }
        

        private static void AddPipeMesh(List<Vector3> path, float thickness, int segs, List<Vector3> verts, List<Vector2> uvs, List<Vector3> norms, List<int> tris)
        {
            if (path == null || path.Count < 2) return;

            var half = thickness * 0.5f;
            segs = Mathf.Max(1, segs);

            var pts = PolyUtils.TrimCorners(PolyUtils.CloneClosed(path), half * 0.5f);
            var n = pts.Count;

            var left = new List<Vector3>(n);
            var right = new List<Vector3>(n);

            for (int i = 0; i < n; i++)
            {
                var vA = PolyUtils.Dir2D(pts[(i - 1 + n) % n], pts[i]);
                var vB = PolyUtils.Dir2D(pts[i], pts[(i + 1) % n]);
                if (vA.sqrMagnitude < 1e-6f) vA = vB;
                if (vB.sqrMagnitude < 1e-6f) vB = vA;

                var nA = new Vector2(-vA.y, vA.x);
                var nB = new Vector2(-vB.y, vB.x);

                if (Mathf.Acos(Mathf.Clamp(Vector2.Dot(nA, nB), -1f, 1f)) < 0.01f)
                {
                    Push((nA + nB).normalized * half); 
                    continue;
                }
                
                for (int s = 0; s <= segs; s++)
                    Push(Vector2.Lerp(nA, nB, s / (float)segs).normalized * half);
                
                continue;

                void Push(Vector2 m)
                {
                    var n3 = new Vector3(m.x, m.y, 0f);
                    left.Add(pts[i] + n3); right.Add(pts[i] - n3);
                }
            }

            var baseIdx = verts.Count;
            
            for (int i = 0; i < left.Count; i++)
            {
                verts.Add(left[i]); verts.Add(right[i]);
                uvs.Add(new Vector2(0, i)); uvs.Add(new Vector2(1, i));
                norms.Add(Vector3.back); norms.Add(Vector3.back);
            }
            
            for (int i = 0; i < left.Count - 1; i++)
            {
                var i0 = baseIdx + i * 2;
                tris.Add(i0); tris.Add(i0+2); tris.Add(i0+1);
                tris.Add(i0+1); tris.Add(i0+2); tris.Add(i0+3);
            }
        }
    }
}