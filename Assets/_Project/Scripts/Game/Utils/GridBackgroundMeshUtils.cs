using System;
using System.Collections.Generic;
using Core.Models;
using UnityEngine;

namespace Core.Utils
{
    public static class GridWithHolesMeshUtils
    {
        public static void BuildGridWithHoles<T>(this IGridModel<T> model, MeshFilter meshFilter, float frameThickness = 0.1f, float cornerSmoothness = 0f, int cornerSegments = 6, Func<int, int, bool> isCellActive = null) where T : class
        {
            if (!meshFilter || frameThickness <= 0f) return;

            var mesh = new Mesh { name = "GridWithHolesMesh" };

            float w = model.Width * model.CellSize;
            float h = model.Height * model.CellSize;
            float halfW = w * 0.5f;
            float halfH = h * 0.5f;

            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();
            var innerTris = new List<int>();
            var frameTris = new List<int>();

            bool useRounded = cornerSmoothness > 0f && cornerSegments > 0;
            float cornerRadius = (frameThickness * 10f) * cornerSmoothness;

            for (int y = 0; y < model.Height; y++)
            {
                int visualY = model.Height - 1 - y;
                float y0 = (visualY * model.CellSize) - halfH;
                float y1 = y0 + model.CellSize;

                for (int x = 0; x < model.Width; x++)
                {
                    if (isCellActive != null && !isCellActive(x, y)) continue;

                    float x0 = (x * model.CellSize) - halfW;
                    float x1 = x0 + model.CellSize;

                    int baseIndex = vertices.Count;

                    AddVertex(new Vector3(x0, y0, 0f), new Vector2(x, y));
                    AddVertex(new Vector3(x1, y0, 0f), new Vector2(x + 1, y));
                    AddVertex(new Vector3(x1, y1, 0f), new Vector2(x + 1, y + 1));
                    AddVertex(new Vector3(x0, y1, 0f), new Vector2(x, y + 1));

                    AddQuad(innerTris, baseIndex + 0, baseIndex + 1, baseIndex + 2, baseIndex + 3);

                    if (IsEmpty(x, y + 1))
                    {
                        int vBase = vertices.Count;
                        float fy0 = y0 - frameThickness;
                        AddVertex(new Vector3(x0, fy0, 0f), Vector2.zero);
                        AddVertex(new Vector3(x1, fy0, 0f), Vector2.right);
                        AddVertex(new Vector3(x1, y0, 0f), Vector2.one);
                        AddVertex(new Vector3(x0, y0, 0f), Vector2.up);
                        AddQuad(frameTris, vBase + 0, vBase + 1, vBase + 2, vBase + 3);
                    }

                    if (IsEmpty(x, y - 1))
                    {
                        int vBase = vertices.Count;
                        float fy1 = y1 + frameThickness;
                        AddVertex(new Vector3(x0, y1, 0f), Vector2.zero);
                        AddVertex(new Vector3(x1, y1, 0f), Vector2.right);
                        AddVertex(new Vector3(x1, fy1, 0f), Vector2.one);
                        AddVertex(new Vector3(x0, fy1, 0f), Vector2.up);
                        AddQuad(frameTris, vBase + 0, vBase + 1, vBase + 2, vBase + 3);
                    }

                    if (IsEmpty(x - 1, y))
                    {
                        int vBase = vertices.Count;
                        float fx0 = x0 - frameThickness;
                        AddVertex(new Vector3(fx0, y0, 0f), Vector2.zero);
                        AddVertex(new Vector3(x0, y0, 0f), Vector2.right);
                        AddVertex(new Vector3(x0, y1, 0f), Vector2.one);
                        AddVertex(new Vector3(fx0, y1, 0f), Vector2.up);
                        AddQuad(frameTris, vBase + 0, vBase + 1, vBase + 2, vBase + 3);
                    }

                    if (IsEmpty(x + 1, y))
                    {
                        int vBase = vertices.Count;
                        float fx1 = x1 + frameThickness;
                        AddVertex(new Vector3(x1, y0, 0f), Vector2.zero);
                        AddVertex(new Vector3(fx1, y0, 0f), Vector2.right);
                        AddVertex(new Vector3(fx1, y1, 0f), Vector2.one);
                        AddVertex(new Vector3(x1, y1, 0f), Vector2.up);
                        AddQuad(frameTris, vBase + 0, vBase + 1, vBase + 2, vBase + 3);
                    }

                    if (IsEmpty(x - 1, y) && IsEmpty(x, y + 1))
                    {
                        bool innerCorner = IsHoleCell(x - 1, y) && IsHoleCell(x, y + 1);
                        if (useRounded)
                        {
                            AddRoundedCorner(frameTris, new Vector3(x0, y0, 0f), Vector3.left, Vector3.down, innerCorner);
                        }
                        else
                        {
                            int vBase = vertices.Count;
                            float fx0 = x0 - frameThickness;
                            float fy0 = y0 - frameThickness;
                            AddVertex(new Vector3(fx0, fy0, 0f), Vector2.zero);
                            AddVertex(new Vector3(x0, fy0, 0f), Vector2.right);
                            AddVertex(new Vector3(x0, y0, 0f), Vector2.one);
                            AddVertex(new Vector3(fx0, y0, 0f), Vector2.up);
                            AddQuad(frameTris, vBase + 0, vBase + 1, vBase + 2, vBase + 3);
                        }
                    }

                    if (IsEmpty(x + 1, y) && IsEmpty(x, y + 1))
                    {
                        bool innerCorner = IsHoleCell(x + 1, y) && IsHoleCell(x, y + 1);
                        if (useRounded)
                        {
                            AddRoundedCorner(frameTris, new Vector3(x1, y0, 0f), Vector3.right, Vector3.down, innerCorner);
                        }
                        else
                        {
                            int vBase = vertices.Count;
                            float fx1 = x1 + frameThickness;
                            float fy0 = y0 - frameThickness;
                            AddVertex(new Vector3(x1, fy0, 0f), Vector2.zero);
                            AddVertex(new Vector3(fx1, fy0, 0f), Vector2.right);
                            AddVertex(new Vector3(fx1, y0, 0f), Vector2.one);
                            AddVertex(new Vector3(x1, y0, 0f), Vector2.up);
                            AddQuad(frameTris, vBase + 0, vBase + 1, vBase + 2, vBase + 3);
                        }
                    }

                    if (IsEmpty(x - 1, y) && IsEmpty(x, y - 1))
                    {
                        bool innerCorner = IsHoleCell(x - 1, y) && IsHoleCell(x, y - 1);
                        if (useRounded)
                        {
                            AddRoundedCorner(frameTris, new Vector3(x0, y1, 0f), Vector3.left, Vector3.up, innerCorner);
                        }
                        else
                        {
                            int vBase = vertices.Count;
                            float fx0 = x0 - frameThickness;
                            float fy1 = y1 + frameThickness;
                            AddVertex(new Vector3(fx0, y1, 0f), Vector2.zero);
                            AddVertex(new Vector3(x0, y1, 0f), Vector2.right);
                            AddVertex(new Vector3(x0, fy1, 0f), Vector2.one);
                            AddVertex(new Vector3(fx0, fy1, 0f), Vector2.up);
                            AddQuad(frameTris, vBase + 0, vBase + 1, vBase + 2, vBase + 3);
                        }
                    }

                    if (IsEmpty(x + 1, y) && IsEmpty(x, y - 1))
                    {
                        bool innerCorner = IsHoleCell(x + 1, y) && IsHoleCell(x, y - 1);
                        if (useRounded)
                        {
                            AddRoundedCorner(frameTris, new Vector3(x1, y1, 0f), Vector3.right, Vector3.up, innerCorner);
                        }
                        else
                        {
                            int vBase = vertices.Count;
                            float fx1 = x1 + frameThickness;
                            float fy1 = y1 + frameThickness;
                            AddVertex(new Vector3(x1, y1, 0f), Vector2.zero);
                            AddVertex(new Vector3(fx1, y1, 0f), Vector2.right);
                            AddVertex(new Vector3(fx1, fy1, 0f), Vector2.one);
                            AddVertex(new Vector3(x1, fy1, 0f), Vector2.up);
                            AddQuad(frameTris, vBase + 0, vBase + 1, vBase + 2, vBase + 3);
                        }
                    }
                }
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

            void AddVertex(Vector3 v, Vector2 uv)
            {
                vertices.Add(v);
                uvs.Add(uv);
                normals.Add(Vector3.back);
            }

            void AddQuad(List<int> tris, int v0, int v1, int v2, int v3)
            {
                tris.Add(v0);
                tris.Add(v1);
                tris.Add(v2);
                tris.Add(v0);
                tris.Add(v2);
                tris.Add(v3);
            }

            void AddRoundedCorner(List<int> tris, Vector3 center, Vector3 dirX, Vector3 dirY, bool inner)
            {
                float r = cornerRadius;
                Vector3 dx = inner ? -dirX.normalized : dirX.normalized;
                Vector3 dy = inner ? -dirY.normalized : dirY.normalized;

                int centerIndex = vertices.Count;
                AddVertex(center, Vector2.zero);

                int prevIndex = -1;

                for (int i = 0; i <= cornerSegments; i++)
                {
                    float t = i / (float)cornerSegments;
                    float angle = t * 0.5f * Mathf.PI;
                    Vector3 offset = (dx * Mathf.Cos(angle) + dy * Mathf.Sin(angle)) * r;
                    int idx = vertices.Count;
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

            bool IsHoleCell(int gx, int gy)
            {
                if (gx < 0 || gx >= model.Width || gy < 0 || gy >= model.Height) return false;
                return isCellActive != null && !isCellActive(gx, gy);
            }
        }
    }
}
