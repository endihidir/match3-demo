using Core.Models;
using Core.Utils;
using UnityEngine;

namespace Core.Extensions
{
    public static class GridModelWorldExtensions
    {
        public static void CalculateCellSize<T>(this IBaseGridModel<T> model, float maxCellSize = float.MaxValue, Camera camera = null) where T : class
        {
            var cam = camera ? camera : Camera.main;
            if (!cam) return;

            var screenWidth  = GetScreenWidth(model, cam);
            var screenHeight = GetScreenHeight(model, cam);

            var borderOffsetW = screenWidth  * (model.ScreenSidePaddingRatio / 100f);
            var gridOffsetW   = screenWidth  * (model.CellSpacingRatio       / 100f);
            var gridOffsetH   = screenHeight * (model.CellSpacingRatio       / 100f);

            var cellSizeW = (screenWidth - borderOffsetW - (gridOffsetW * (model.Width - 1))) / model.Width;

            var totalGridHeight = (cellSizeW * model.Height) + (gridOffsetH * (model.Height - 1));
            var maxWorldHeight  = screenHeight * 0.9f;

            var cellSize = totalGridHeight > maxWorldHeight
                ? (maxWorldHeight - (gridOffsetH * (model.Height - 1))) / model.Height
                : cellSizeW;
            
            cellSize = Mathf.Clamp(cellSize, 0f, maxCellSize);

            model.CellSize = cellSize;
        }

        public static Vector3 GridToWorld<T>(this IBaseGridModel<T> model, Vector2Int pos, Camera camera = null) where T : class
        {
            var cam = camera ? camera : Camera.main;
            
            if (!cam || !model.IsInRange(pos)) return Vector3.zero;

            var screenWidth  = GetScreenWidth(model, cam);
            var screenHeight = GetScreenHeight(model, cam);

            var gridOffsetX = screenWidth  * (model.CellSpacingRatio / 100f);
            var gridOffsetY = screenHeight * (model.CellSpacingRatio / 100f);

            var totalGridWidth = (model.Width * model.CellSize) + ((model.Width - 1) * gridOffsetX);
            var leftStartX = GetLeftX(model, cam) + ((screenWidth - totalGridWidth) * 0.5f);

            var x = leftStartX + (pos.x * (model.CellSize + gridOffsetX)) + (model.CellSize * 0.5f);
            var y = GetTopY(model, cam) - (pos.y * (model.CellSize + gridOffsetY)) - (model.CellSize * 0.5f);

            return new Vector3(x, y, 0f);
        }
        
        public static Vector2Int WorldToGrid<T>(this IBaseGridModel<T> model, Vector3 worldPos, bool clamp = true, Camera camera = null) where T : class
        {
            var cam = camera ? camera : Camera.main;
            
            if (!cam) return new Vector2Int(-1, -1);
            
            var position = new Vector2Int(-1, -1);
            
            var screenWidth  = GetScreenWidth(model, cam);
            var screenHeight = GetScreenHeight(model, cam);

            var gridOffsetX = screenWidth  * (model.CellSpacingRatio / 100f);
            var gridOffsetY = screenHeight * (model.CellSpacingRatio / 100f);

            var totalGridWidth = (model.Width * model.CellSize) + ((model.Width - 1) * gridOffsetX);
            var leftStartX = GetLeftX(model, cam) + ((screenWidth - totalGridWidth) * 0.5f);
            
            var absXFromGrid = worldPos.x - leftStartX;
            var absYFromTop  = GetTopY(model, cam) - worldPos.y;

            var dividerX = model.CellSize + gridOffsetX;
            var dividerY = model.CellSize + gridOffsetY;
            
            if ((absXFromGrid % dividerX) > model.CellSize || (absYFromTop % dividerY) > model.CellSize)
            {
                return position;
            }

            var gx = Mathf.FloorToInt(absXFromGrid / dividerX);
            var gy = Mathf.FloorToInt(absYFromTop  / dividerY);

            if (clamp)
            {
                gx = Mathf.Clamp(gx, 0, model.Width  - 1);
                gy = Mathf.Clamp(gy, 0, model.Height - 1);
            }

            position.x = gx;
            position.y = gy;
            return position;
        }

        public static bool TryGetGridObjectFromMousePosition<T>(this IBaseGridModel<T> model, out T obj, Camera camera = null) where T : class
        {
            var cam = camera ? camera : Camera.main;
            
            if (!cam)
            {
                obj = null;
                return false;
            }

            var worldPosition = cam.ScreenToWorldPoint(Input.mousePosition).With(z: cam.nearClipPlane);
            
            var pos = model.WorldToGrid(worldPosition, false, cam);
            
            if (!model.IsInRange(pos))
            {
                obj = null;
                return false;
            }

            obj = model.GetGridObject(pos);
            return obj != null;
        }

        public static void DrawGrid<T>(this IBaseGridModel<T> model, Camera camera = null) where T : class
        {
            if (!model.DrawGizmos) return;

            var cam = camera ? camera : Camera.main;
            
            if (!cam) return;
    
            Gizmos.color = model.GizmosColor;
            
            var cellCount = model.Width * model.Height;

            for (int i = 0; i < cellCount; i++)
            {
                var coordinate = CoordinateUtils.ToCoordinate(i, model.Width);
                var center = model.GridToWorld(coordinate, cam);
                var half= model.CellSize * 0.5f;
                
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

        public static float GetScreenWidth<T>(this IBaseGridModel<T> model, Camera cam) where T : class
        {
            return Mathf.Abs(GetLeftX(model, cam) - GetRightX(model, cam));
        }

        public static float GetScreenHeight<T>(this IBaseGridModel<T> model, Camera cam) where T : class
        {
            return Mathf.Abs(GetTopY(model, cam) - GetBottomY(model, cam));
        }

        public static float GetRightX<T>(this IBaseGridModel<T> model, Camera cam) where T : class
        {
            return GetOriginPos(model, cam, Vector3.right).x;
        }

        public static float GetTopY<T>(this IBaseGridModel<T> model, Camera cam) where T : class
        {
            return GetOriginPos(model, cam, Vector3.up).y;
        }

        public static float GetLeftX<T>(this IBaseGridModel<T> model, Camera cam) where T : class
        {
            return GetOriginPos(model, cam, Vector3.zero).x;
        }

        public static float GetBottomY<T>(this IBaseGridModel<T> model, Camera cam) where T : class
        {
            return GetOriginPos(model, cam, Vector3.zero).y;
        }

        public static Vector3 GetOriginPos<T>(this IBaseGridModel<T> model, Camera cam, Vector3 origin) where T : class
        {
            return cam.ViewportToWorldPoint(origin.With(z: cam.nearClipPlane)) + new Vector3(model.OriginOffset.x, -model.OriginOffset.y, 0f);
        }
    }
}
