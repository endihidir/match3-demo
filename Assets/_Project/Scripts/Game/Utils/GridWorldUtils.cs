using Core.Extensions;
using UnityEngine;

namespace Core.Utils
{
    public static class GridWorldUtils
    {
        public static float CalculateCellSize(int width, int height, float screenSidePaddingRatio, float cellSpacingRatio, Vector3 originOffset, float minCellSize = 0f, float maxCellSize = float.MaxValue, Camera camera = null)
        {
            var cam = camera ? camera : Camera.main;
            if (!cam) return 0f;

            var screenWidth  = GetScreenWidth(cam, originOffset);
            var screenHeight = GetScreenHeight(cam, originOffset);

            var borderOffsetW = screenWidth  * (screenSidePaddingRatio / 100f);
            var gridOffsetW   = screenWidth  * (cellSpacingRatio       / 100f);
            var gridOffsetH   = screenHeight * (cellSpacingRatio       / 100f);

            var cellSizeW = (screenWidth - borderOffsetW - (gridOffsetW * (width - 1))) / width;

            var totalGridHeight = (cellSizeW * height) + (gridOffsetH * (height - 1));
            var maxWorldHeight  = screenHeight * 0.9f;

            float calculated = totalGridHeight > maxWorldHeight ? (maxWorldHeight - (gridOffsetH * (height - 1))) / height : cellSizeW;

            return Mathf.Clamp(calculated, minCellSize, maxCellSize);
        }

        public static Vector3 GridToWorld(int width, int height, float cellSize, float cellSpacingRatio, Vector3 originOffset, Vector2Int pos, Camera camera = null)
        {
            var cam = camera ? camera : Camera.main;

            if (!cam || !IsInRange(width, height, pos)) return Vector3.zero;

            var screenWidth  = GetScreenWidth(cam, originOffset);
            var screenHeight = GetScreenHeight(cam, originOffset);

            var gridOffsetX = screenWidth  * (cellSpacingRatio / 100f);
            var gridOffsetY = screenHeight * (cellSpacingRatio / 100f);

            var totalGridWidth = (width * cellSize) + ((width - 1) * gridOffsetX);
            var leftStartX = GetLeftX(cam, originOffset) + ((screenWidth - totalGridWidth) * 0.5f);

            var x = leftStartX + (pos.x * (cellSize + gridOffsetX)) + (cellSize * 0.5f);
            var y = GetTopY(cam, originOffset) - (pos.y * (cellSize + gridOffsetY)) - (cellSize * 0.5f);

            return new Vector3(x, y, 0f);
        }

        public static Vector2Int WorldToGrid(int width, int height, float cellSize, float cellSpacingRatio, Vector3 originOffset, Vector3 worldPos, bool clamp = true, Camera camera = null)
        {
            var cam = camera ? camera : Camera.main;

            if (!cam) return new Vector2Int(-1, -1);

            var position = new Vector2Int(-1, -1);

            var screenWidth  = GetScreenWidth(cam, originOffset);
            var screenHeight = GetScreenHeight(cam, originOffset);

            var gridOffsetX = screenWidth  * (cellSpacingRatio / 100f);
            var gridOffsetY = screenHeight * (cellSpacingRatio / 100f);

            var totalGridWidth = (width * cellSize) + ((width - 1) * gridOffsetX);
            var leftStartX = GetLeftX(cam, originOffset) + ((screenWidth - totalGridWidth) * 0.5f);

            var absXFromGrid = worldPos.x - leftStartX;
            var absYFromTop  = GetTopY(cam, originOffset) - worldPos.y;

            var dividerX = cellSize + gridOffsetX;
            var dividerY = cellSize + gridOffsetY;

            if ((absXFromGrid % dividerX) > cellSize || (absYFromTop % dividerY) > cellSize)
            {
                return position;
            }

            var gx = Mathf.FloorToInt(absXFromGrid / dividerX);
            var gy = Mathf.FloorToInt(absYFromTop  / dividerY);

            if (clamp)
            {
                gx = Mathf.Clamp(gx, 0, width  - 1);
                gy = Mathf.Clamp(gy, 0, height - 1);
            }

            position.x = gx;
            position.y = gy;
            return position;
        }

        public static bool TryGetGridPositionFromMousePosition(int width, int height, float cellSize, float cellSpacingRatio, Vector3 originOffset, out Vector2Int pos, Camera camera = null)
        {
            var cam = camera ? camera : Camera.main;

            if (!cam)
            {
                pos = new Vector2Int(-1, -1);
                return false;
            }

            var worldPosition = cam.ScreenToWorldPoint(Input.mousePosition).With(z: cam.nearClipPlane);

            pos = WorldToGrid(width, height, cellSize, cellSpacingRatio, originOffset, worldPosition, false, cam);

            return IsInRange(width, height, pos);
        }

        public static bool TryGetGridObjectFromMousePosition<T>(int width, int height, float cellSize, float cellSpacingRatio, Vector3 originOffset, System.Func<Vector2Int, T> getGridObject, out T obj, Camera camera = null) where T : class
        {
            var cam = camera ? camera : Camera.main;

            if (!cam)
            {
                obj = null;
                return false;
            }

            var worldPosition = cam.ScreenToWorldPoint(Input.mousePosition).With(z: cam.nearClipPlane);

            var pos = WorldToGrid(width, height, cellSize, cellSpacingRatio, originOffset, worldPosition, false, cam);

            if (!IsInRange(width, height, pos))
            {
                obj = null;
                return false;
            }

            obj = getGridObject?.Invoke(pos);
            return obj != null;
        }

        public static void DrawGrid(int width, int height, float cellSize, float cellSpacingRatio, Vector3 originOffset, bool drawGizmos, Color gizmosColor, Camera camera = null)
        {
            if (!drawGizmos) return;

            var cam = camera ? camera : Camera.main;

            if (!cam) return;

            Gizmos.color = gizmosColor;

            var cellCount = width * height;

            for (int i = 0; i < cellCount; i++)
            {
                var coordinate = CoordinateUtils.ToCoordinate(i, width);
                var center = GridToWorld(width, height, cellSize, cellSpacingRatio, originOffset, coordinate, cam);
                var half = cellSize * 0.5f;

                var topLeft     = new Vector3(center.x - half, center.y + half, center.z);
                var topRight    = new Vector3(center.x + half, center.y + half, center.z);
                var bottomRight = new Vector3(center.x + half, center.y - half, center.z);
                var bottomLeft  = new Vector3(center.x - half, center.y - half, center.z);

                Gizmos.DrawLine(topLeft, topRight);
                Gizmos.DrawLine(topRight, bottomRight);
                Gizmos.DrawLine(bottomRight, bottomLeft);
                Gizmos.DrawLine(bottomLeft, topLeft);
            }
        }

        public static float GetScreenWidth(Camera cam, Vector3 originOffset)
        {
            return Mathf.Abs(GetLeftX(cam, originOffset) - GetRightX(cam, originOffset));
        }

        public static float GetScreenHeight(Camera cam, Vector3 originOffset)
        {
            return Mathf.Abs(GetTopY(cam, originOffset) - GetBottomY(cam, originOffset));
        }

        public static float GetRightX(Camera cam, Vector3 originOffset)
        {
            return GetOriginPos(cam, originOffset, Vector3.right).x;
        }

        public static float GetTopY(Camera cam, Vector3 originOffset)
        {
            return GetOriginPos(cam, originOffset, Vector3.up).y;
        }

        public static float GetLeftX(Camera cam, Vector3 originOffset)
        {
            return GetOriginPos(cam, originOffset, Vector3.zero).x;
        }

        public static float GetBottomY(Camera cam, Vector3 originOffset)
        {
            return GetOriginPos(cam, originOffset, Vector3.zero).y;
        }

        public static Vector3 GetOriginPos(Camera cam, Vector3 originOffset, Vector3 origin)
        {
            return cam.ViewportToWorldPoint(origin.With(z: cam.nearClipPlane)) + new Vector3(originOffset.x, -originOffset.y, 0f);
        }

        public static bool IsInRange(int width, int height, Vector2Int pos)
        {
            return pos.x >= 0 && pos.y >= 0 && pos.x < width && pos.y < height;
        }
    }
}