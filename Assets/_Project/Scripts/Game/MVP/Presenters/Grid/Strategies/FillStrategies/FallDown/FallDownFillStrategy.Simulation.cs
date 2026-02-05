using Core.Extensions;
using Core.Models;
using Core.Utils;
using UnityEngine;

namespace Core.Handlers
{
    public partial class FallDownFillStrategy
    {
        private void ShiftColumn(IGridModel model, int x, int height)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                var coord = new Vector2Int(x, y);

                if (!model.IsCellActive(coord)) continue;
                if (model.GetGridObject(coord)) continue;

                var srcY = GridFillCalcUtil.FindFallSourceY(model, x, y - 1);
                if (srcY < 0) continue;

                var src = new Vector2Int(x, srcY);
                var item = model.GetGridObject(src);

                if (!item || item.IsStationary) continue;

                model.SetGridObject(coord, item);
                model.SetGridObject(src, null);

                AddRecord(new FallDownMoveRecord(item, coord, isSpawn: false));
            }
        }

        private void FillColumn(GridStateContext context, int x, int height, float cellSize, float spawnY)
        {
            var model = context.GridModel;
            var view = context.GridView;
            var stack = 0;

            for (int y = height - 1; y >= 0; y--)
            {
                var coord = new Vector2Int(x, y);

                if (!model.IsCellActive(coord)) continue;
                if (model.GetGridObject(coord)) continue;
                if (GridFillCalcUtil.HasStationaryAboveInSameSegment(model, coord, false)) continue;

                var itemType = _itemDecider.Decide(model, coord);
                var item = context.GridItemFactory.GetRegularItem(itemType);

                item.SetParent(view.GridObjectsParent);
                item.SetSpriteSize(cellSize);

                var worldPos = view.GridToWorld(coord);
                var startY = spawnY + (stack * cellSize);
                item.SetPosition(new Vector3(worldPos.x, startY, worldPos.z));

                model.SetGridObject(coord, item);
                AddRecord(new FallDownMoveRecord(item, coord, isSpawn: true));
                stack++;
            }
        }
    }
}