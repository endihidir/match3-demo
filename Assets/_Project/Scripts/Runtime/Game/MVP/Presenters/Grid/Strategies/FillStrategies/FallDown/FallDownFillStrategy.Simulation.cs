using Game.Grid.Strategies.Data;
using Game.Grid.Utils;
using UnityEngine;

namespace Game.Grid.Strategies
{
    public sealed partial class FallDownFillStrategy
    {
        private void ShiftColumnLogic(int x, int height)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                var coord = new Vector2Int(x, y);

                if (!_gridModel.IsCellActive(coord)) continue;
                if (_gridModel.GetGridObject(coord)) continue;

                var srcY = GridFillCalcUtil.FindFallSourceY(_gridModel, x, y - 1);
                if (srcY < 0) continue;

                var src = new Vector2Int(x, srcY);
                var item = _gridModel.GetGridObject(src);

                if (!item || item.IsStationary) continue;

                _gridModel.SetGridObject(coord, item);
                _gridModel.SetGridObject(src, null);

                var fallMoveRecord = new FallDownMoveRecord(item, coord, false);
                AddRecord(fallMoveRecord);
            }
        }

        private void FillColumnLogic(int x, int height, float cellSize, float spawnY)
        {
            var stack = 0;

            for (int y = height - 1; y >= 0; y--)
            {
                var coord = new Vector2Int(x, y);

                if (!_gridModel.IsCellActive(coord)) continue;
                if (_gridModel.GetGridObject(coord)) continue;
                
                if(GridFillCalcUtil.HasStationaryAboveInSameSegment(_gridModel, coord, false)) continue;

                var itemType = _itemDecider.Decide(coord);
                var item = _objectCreateHandler.CreateItem(itemType);

                item.SetParent(_gridView.GridObjectsParent);
                item.SetSpriteSize(cellSize);

                var worldPos = _gridView.GridToWorld(coord);
                var startY = spawnY + (stack * cellSize);
                var start = new Vector3(worldPos.x, startY, worldPos.z);

                item.SetPosition(start);
                _gridModel.SetGridObject(coord, item);

                var fallMoveRecord = new FallDownMoveRecord(item, coord, true);
                AddRecord(fallMoveRecord);
                stack++;
            }
        }
    }
}