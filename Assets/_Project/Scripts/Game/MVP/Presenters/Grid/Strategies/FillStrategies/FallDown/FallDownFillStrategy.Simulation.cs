using Game.Grid.Contexts;
using Game.Grid.Strategies.Data;
using Game.Grid.Utils;
using Game.Models;
using UnityEngine;

namespace Game.Grid.Strategies
{
    public partial class FallDownFillStrategy
    {
        private void ShiftColumnLogic(IGridModel model, int x, int height)
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

                var fallMoveRecord = new FallDownMoveRecord(item, coord, false);
                AddRecord(fallMoveRecord);
            }
        }

        private void FillColumnLogic(GridStateContext stateContext, int x, int height, float cellSize, float spawnY)
        {
            var model = stateContext.GridModel;
            var view = stateContext.GridView;

            var stack = 0;

            for (int y = height - 1; y >= 0; y--)
            {
                var coord = new Vector2Int(x, y);

                if (!model.IsCellActive(coord)) continue;
                if (model.GetGridObject(coord)) continue;
                
                if(GridFillCalcUtil.HasStationaryAboveInSameSegment(model, coord, false)) continue;

                var itemType = _itemDecider.Decide(model, coord);
                var item = stateContext.GridObjectHandler.GetRegularItem(itemType);

                item.SetParent(view.GridObjectsParent);
                item.SetSpriteSize(cellSize);

                var worldPos = view.GridToWorld(coord);
                var startY = spawnY + (stack * cellSize);
                var start = new Vector3(worldPos.x, startY, worldPos.z);

                item.SetPosition(start);
                model.SetGridObject(coord, item);

                var fallMoveRecord = new FallDownMoveRecord(item, coord, true);
                AddRecord(fallMoveRecord);
                stack++;
            }
        }
    }
}