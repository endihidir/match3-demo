using UnityEngine;

namespace Core.Grid
{
    public struct GridLayout
    {
        public float CellSize;
        public Vector3 OriginOffset;
        public float ScreenSidePaddingRatio;
        public float CellSpacingRatio;
    }

    public interface IGridLayoutCalculator
    {
        GridLayout CalculateLayout(Vector2Int gridSize, Camera cam, float screenSidePaddingRatio, float cellSpacingRatio, float maxCellSize, float maxWorldHeightRatio = 0.9f);
        Vector3 GridToWorld(in GridLayout layout, Vector2Int gridSize, Camera cam, Vector2Int pos);
        Vector2Int WorldToGrid(in GridLayout layout, Vector2Int gridSize, Camera cam, Vector3 worldPos, bool clamp = true);
        float GetTopY(in GridLayout layout, Camera cam);
        float GetLeftX(in GridLayout layout, Camera cam);
        float GetRightX(in GridLayout layout, Camera cam);
        float GetBottomY(in GridLayout layout, Camera cam);
    }
    
    public class GridLayoutCalculator : IGridLayoutCalculator
    {
        public GridLayout CalculateLayout(Vector2Int gridSize, Camera cam, float screenSidePaddingRatio, float cellSpacingRatio, float maxCellSize, float maxWorldHeightRatio = 0.9f)
        {
            var layout = new GridLayout
            {
                ScreenSidePaddingRatio = screenSidePaddingRatio,
                CellSpacingRatio = cellSpacingRatio,
                OriginOffset = Vector3.zero,
                CellSize = 0f
            };

            if (!cam) return layout;

            var screenWidth  = Mathf.Abs(GetLeftX(layout, cam) - GetRightX(layout, cam));
            var screenHeight = Mathf.Abs(GetTopY(layout, cam) - GetBottomY(layout, cam));

            var borderOffsetW = screenWidth  * (layout.ScreenSidePaddingRatio / 100f);
            var gridOffsetW   = screenWidth  * (layout.CellSpacingRatio / 100f);
            var gridOffsetH   = screenHeight * (layout.CellSpacingRatio / 100f);

            var cellSizeW = (screenWidth - borderOffsetW - (gridOffsetW * (gridSize.x - 1))) / gridSize.x;

            var totalGridHeight = (cellSizeW * gridSize.y) + (gridOffsetH * (gridSize.y - 1));
            var maxWorldHeight  = screenHeight * maxWorldHeightRatio;

            var cellSize = totalGridHeight > maxWorldHeight ? (maxWorldHeight - (gridOffsetH * (gridSize.y - 1))) / gridSize.y : cellSizeW;

            layout.CellSize = Mathf.Clamp(cellSize, 0f, maxCellSize);
            return layout;
        }

        public Vector3 GridToWorld(in GridLayout layout, Vector2Int gridSize, Camera cam, Vector2Int pos)
        {
            if (!cam) return Vector3.zero;

            var screenWidth  = Mathf.Abs(GetLeftX(layout, cam) - GetRightX(layout, cam));
            var screenHeight = Mathf.Abs(GetTopY(layout, cam) - GetBottomY(layout, cam));

            var gridOffsetX = screenWidth  * (layout.CellSpacingRatio / 100f);
            var gridOffsetY = screenHeight * (layout.CellSpacingRatio / 100f);

            var totalGridWidth = (gridSize.x * layout.CellSize) + ((gridSize.x - 1) * gridOffsetX);
            var leftStartX = GetLeftX(layout, cam) + ((screenWidth - totalGridWidth) * 0.5f);

            var x = leftStartX + (pos.x * (layout.CellSize + gridOffsetX)) + (layout.CellSize * 0.5f);
            var y = GetTopY(layout, cam) - (pos.y * (layout.CellSize + gridOffsetY)) - (layout.CellSize * 0.5f);

            var world = new Vector3(x, y, 0f);

            return world;
        }

        public Vector2Int WorldToGrid(in GridLayout layout, Vector2Int gridSize, Camera cam, Vector3 worldPos, bool clamp = true)
        {
            if (!cam) return new Vector2Int(-1, -1);

            var screenWidth  = Mathf.Abs(GetLeftX(layout, cam) - GetRightX(layout, cam));
            var screenHeight = Mathf.Abs(GetTopY(layout, cam) - GetBottomY(layout, cam));

            var gridOffsetX = screenWidth  * (layout.CellSpacingRatio / 100f);
            var gridOffsetY = screenHeight * (layout.CellSpacingRatio / 100f);

            var totalGridWidth = (gridSize.x * layout.CellSize) + ((gridSize.x - 1) * gridOffsetX);
            var leftStartX = GetLeftX(layout, cam) + ((screenWidth - totalGridWidth) * 0.5f);

            var absXFromGrid = worldPos.x - leftStartX;
            var absYFromTop  = GetTopY(layout, cam) - worldPos.y;

            var dividerX = layout.CellSize + gridOffsetX;
            var dividerY = layout.CellSize + gridOffsetY;

            if ((absXFromGrid % dividerX) > layout.CellSize || (absYFromTop % dividerY) > layout.CellSize)
            {
                return new Vector2Int(-1, -1);
            }

            var gx = Mathf.FloorToInt(absXFromGrid / dividerX);
            var gy = Mathf.FloorToInt(absYFromTop  / dividerY);

            if (clamp)
            {
                gx = Mathf.Clamp(gx, 0, gridSize.x - 1);
                gy = Mathf.Clamp(gy, 0, gridSize.y - 1);
            }

            return new Vector2Int(gx, gy);
        }

        public float GetTopY(in GridLayout layout, Camera cam)
        {
            return GetOriginPos(layout, cam, Vector3.up).y;
        }

        public float GetLeftX(in GridLayout layout, Camera cam)
        {
            return GetOriginPos(layout, cam, Vector3.zero).x;
        }

        public float GetRightX(in GridLayout layout, Camera cam)
        {
            return GetOriginPos(layout, cam, Vector3.right).x;
        }

        public float GetBottomY(in GridLayout layout, Camera cam)
        {
            return GetOriginPos(layout, cam, Vector3.zero).y;
        }

        private Vector3 GetOriginPos(in GridLayout layout, Camera cam, Vector3 origin)
        {
            return cam.ViewportToWorldPoint(new Vector3(origin.x, origin.y, cam.nearClipPlane)) + new Vector3(layout.OriginOffset.x, -layout.OriginOffset.y, 0f);
        }
    }
}