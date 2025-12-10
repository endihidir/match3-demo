using System;
using System.Collections.Generic;
using Core.Models;
using UnityEngine;

namespace Core.Utils
{
    public static class GridWithHolesMeshUtils
    {
        /// <summary>
        /// Generates a mesh for the Match-3 board with holes (inactive cells).
        /// Submesh 0 = inner cell quads
        /// Submesh 1 = outer frame segments + corners
        /// </summary>
        public static void BuildGridMesh<T>(this IGridModel<T> model, MeshFilter meshFilter, float frameThickness = 0.1f, float cornerSmoothness = 0f, int cornerSegments = 6, Func<int, int, bool> isCellActive = null) where T : class
        {
            if (!meshFilter || frameThickness <= 0f) return;

            var mesh = new Mesh { name = "GridWithHolesMesh" };

            var cellSize = model.CellSize;

            var w = model.Width * cellSize;
            var h = model.Height * cellSize;

            var halfW = w * 0.5f;
            var halfH = h * 0.5f;

            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();
            var innerTris = new List<int>();
            var frameTris = new List<int>();

            var useRounded = cornerSmoothness > 0f && cornerSegments > 0;

            for (int i = 0; i < model.Height * model.Width; i++)
            {
                var coordinate = CoordinateUtils.ToCoordinate(i, model.Width);
                var x = coordinate.x;
                var y = coordinate.y;
            
                var visualY = model.Height - 1 - y;
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
                if (gx < 0 || gx >= model.Width || gy < 0 || gy >= model.Height) return true;

                return isCellActive != null && !isCellActive(gx, gy);
            }
        }
    }
}