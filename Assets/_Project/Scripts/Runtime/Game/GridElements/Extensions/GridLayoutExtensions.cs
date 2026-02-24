using System;
using Game.Utils;
using UnityEngine;
using GridLayout = Core.Grid.GridLayout;

namespace Game.Extensions
{
    public static class GridLayoutExtensions
    {
        public static float CalculateCellSize(this in GridLayout layout, Vector2Int gridSize, Camera cam, float maxWorldHeightRatio = 0.9f)
        {
            if (!cam) return 0;

            var screenWidth  = Mathf.Abs(layout.GetLeftX(cam) - layout.GetRightX(cam));
            var screenHeight = Mathf.Abs(layout.GetTopY(cam) - layout.GetBottomY(cam));

            var borderOffsetW = screenWidth  * (layout.screenSidePaddingRatio / 100f);
            var gridOffsetW   = screenWidth  * (layout.cellSpacingRatio / 100f);
            var gridOffsetH   = screenHeight * (layout.cellSpacingRatio / 100f);

            var cellSizeW = (screenWidth - borderOffsetW - (gridOffsetW * (gridSize.x - 1))) / gridSize.x;

            var totalGridHeight = (cellSizeW * gridSize.y) + (gridOffsetH * (gridSize.y - 1));
            var maxWorldHeight  = screenHeight * maxWorldHeightRatio;

            var cellSize = totalGridHeight > maxWorldHeight ? (maxWorldHeight - (gridOffsetH * (gridSize.y - 1))) / gridSize.y : cellSizeW;
            
            return cellSize;
        }
        
        public static Vector3 GridToWorld(this in GridLayout layout, Vector2Int gridSize, Vector2Int cellCoordinate, Camera cam)
        {
            if (!cam) return Vector3.zero;

            var screenWidth  = Mathf.Abs(layout.GetLeftX(cam) - layout.GetRightX(cam));
            var screenHeight = Mathf.Abs(layout.GetTopY(cam) - layout.GetBottomY(cam));

            var gridOffsetX = screenWidth  * (layout.cellSpacingRatio / 100f);
            var gridOffsetY = screenHeight * (layout.cellSpacingRatio / 100f);

            var totalGridWidth = (gridSize.x * layout.cellSize) + ((gridSize.x - 1) * gridOffsetX);
            var leftStartX = layout.GetLeftX(cam) + ((screenWidth - totalGridWidth) * 0.5f);

            var x = leftStartX + (cellCoordinate.x * (layout.cellSize + gridOffsetX)) + (layout.cellSize * 0.5f);
            var y = layout.GetTopY(cam) - (cellCoordinate.y * (layout.cellSize + gridOffsetY)) - (layout.cellSize * 0.5f);

            return new Vector3(x, y, 0f);
        }

        public static Vector2Int WorldToGrid(this in GridLayout layout, Vector2Int gridSize, Vector3 worldPos, Camera cam, bool clamp = false)
        {
            if (!cam) return new Vector2Int(-1, -1);

            var screenWidth  = Mathf.Abs(layout.GetLeftX(cam) - layout.GetRightX(cam));
            var screenHeight = Mathf.Abs(layout.GetTopY(cam) - layout.GetBottomY(cam));

            var gridOffsetX = screenWidth  * (layout.cellSpacingRatio / 100f);
            var gridOffsetY = screenHeight * (layout.cellSpacingRatio / 100f);

            var totalGridWidth = (gridSize.x * layout.cellSize) + ((gridSize.x - 1) * gridOffsetX);
            var leftStartX = layout.GetLeftX(cam) + ((screenWidth - totalGridWidth) * 0.5f);

            var absXFromGrid = worldPos.x - leftStartX;
            var absYFromTop  = layout.GetTopY(cam) - worldPos.y;

            var dividerX = layout.cellSize + gridOffsetX;
            var dividerY = layout.cellSize + gridOffsetY;

            if ((absXFromGrid % dividerX) > layout.cellSize || (absYFromTop % dividerY) > layout.cellSize)
                return new Vector2Int(-1, -1);

            var gx = Mathf.FloorToInt(absXFromGrid / dividerX);
            var gy = Mathf.FloorToInt(absYFromTop  / dividerY);

            if (clamp)
            {
                gx = Mathf.Clamp(gx, 0, gridSize.x - 1);
                gy = Mathf.Clamp(gy, 0, gridSize.y - 1);
            }

            return new Vector2Int(gx, gy);
        }
        
        public static void DrawGrid(this GridLayout layout, Vector2Int gridSize, Color gizmosColor, Camera camera = null, Func<int, int, bool> isCellActive = null)
        {
            var cam = camera ? camera : Camera.main;
            
            if (!cam) return;

            Gizmos.color = gizmosColor;

            var width = gridSize.x;
            var height = gridSize.y;

            for (int i = 0; i < width * height; i++)
            {
                var coordinate = GridIndexUtil.ToCoord(i, width);
                if (isCellActive != null && !isCellActive(coordinate.x, coordinate.y)) continue;
                
                var center = layout.GridToWorld(gridSize, coordinate, cam);
                var half = layout.cellSize * 0.5f;

                var topLeft = new Vector3(center.x - half, center.y + half, center.z);
                var topRight = new Vector3(center.x + half, center.y + half, center.z);
                var bottomRight = new Vector3(center.x + half, center.y - half, center.z);
                var bottomLeft  = new Vector3(center.x - half, center.y - half, center.z);

                Gizmos.DrawLine(topLeft, topRight);
                Gizmos.DrawLine(topRight, bottomRight);
                Gizmos.DrawLine(bottomRight, bottomLeft);
                Gizmos.DrawLine(bottomLeft, topLeft);
            }
        }

        public static float GetTopYRaw(this in GridLayout layout, Camera cam) => GetViewportWorldRaw(cam, Vector3.up).y;
        public static float GetBottomYRaw(this in GridLayout layout, Camera cam) => GetViewportWorldRaw(cam, Vector3.zero).y;
        public static float GetLeftXRaw(this in GridLayout layout, Camera cam) => GetViewportWorldRaw(cam, Vector3.zero).x;
        public static float GetRightXRaw(this in GridLayout layout, Camera cam) => GetViewportWorldRaw(cam, Vector3.right).x;

        public static float GetTopY(this in GridLayout layout, Camera cam) => ApplyOriginOffset(layout, GetViewportWorldRaw(cam, Vector3.up)).y;
        public static float GetBottomY(this in GridLayout layout, Camera cam) => ApplyOriginOffset(layout, GetViewportWorldRaw(cam, Vector3.zero)).y;
        public static float GetLeftX(this in GridLayout layout, Camera cam) => ApplyOriginOffset(layout, GetViewportWorldRaw(cam, Vector3.zero)).x;
        public static float GetRightX(this in GridLayout layout, Camera cam) => ApplyOriginOffset(layout, GetViewportWorldRaw(cam, Vector3.right)).x;

        private static Vector3 GetViewportWorldRaw(Camera cam, Vector3 origin) => 
            cam.ViewportToWorldPoint(new Vector3(origin.x, origin.y, cam.nearClipPlane));

        private static Vector3 ApplyOriginOffset(in GridLayout layout, Vector3 worldPos) => 
            worldPos + new Vector3(layout.originOffset.x, -layout.originOffset.y, 0f);
    }
}