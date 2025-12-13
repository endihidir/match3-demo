using UnityEngine;
using GridLayout = Core.Grid.GridLayout;

namespace Core.Extensions
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
        
        public static Vector3 GridToWorld(this in GridLayout layout, Vector2Int gridSize, Camera cam, Vector2Int cellCoordinate)
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

        public static Vector2Int WorldToGrid(this in GridLayout layout, Vector2Int gridSize, Camera cam, Vector3 worldPos, bool clamp = true)
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

        public static float GetTopY(this in GridLayout layout, Camera cam) => GetOriginPos(layout, cam, Vector3.up).y;
        public static float GetLeftX(this in GridLayout layout, Camera cam) => GetOriginPos(layout, cam, Vector3.zero).x;
        public static float GetRightX(this in GridLayout layout, Camera cam) => GetOriginPos(layout, cam, Vector3.right).x;
        public static float GetBottomY(this in GridLayout layout, Camera cam) => GetOriginPos(layout, cam, Vector3.zero).y;

        private static Vector3 GetOriginPos(in GridLayout layout, Camera cam, Vector3 origin)
        {
            return cam.ViewportToWorldPoint(new Vector3(origin.x, origin.y, cam.nearClipPlane)) + 
                   new Vector3(layout.originOffset.x, -layout.originOffset.y, 0f);
        }
    }
}