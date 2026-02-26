using Game.Grid.Strategies.Data;
using Game.Grid.Utils;
using UnityEngine;

namespace Game.Grid.Strategies
{
    public sealed partial class FallDownFillStrategy
    {
        /// <summary>
        /// Drops every non-stationary item in column <paramref name="x"/>
        /// as far down as it can go in one pass (bottom-to-top scan).
        /// </summary>
        private void ShiftColumn(int x, int height)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                var coord = new Vector2Int(x, y);

                if (!_gridModel.IsCellActive(coord)) continue;
                if (_gridModel.GetGridObject(coord))  continue;

                var srcY = GridFillCalcUtil.FindFallSourceY(_gridModel, x, y - 1);
                if (srcY < 0) continue;

                var src  = new Vector2Int(x, srcY);
                var item = _gridModel.GetGridObject(src);

                if (!item || item.IsStationary) continue;

                _gridModel.SetGridObject(coord, item);
                _gridModel.SetGridObject(src, null);

                AddRecord(new FallDownMoveRecord(item, coord, isSpawn: false));
            }
        }

        /// <summary>
        /// Spawns new items into every empty active cell in column
        /// <paramref name="x"/> that has no stationary object above it
        /// in the same segment, stacking spawn positions above
        /// <paramref name="spawnWorldY"/>.
        /// </summary>
        private void FillColumn(int x, int height, float cellSize, float spawnWorldY)
        {
            var stack = 0;

            for (int y = height - 1; y >= 0; y--)
            {
                var coord = new Vector2Int(x, y);

                if (!_gridModel.IsCellActive(coord)) continue;
                if (_gridModel.GetGridObject(coord))  continue;

                // Skip cells blocked from above by a stationary object.
                if (GridFillCalcUtil.HasStationaryAboveInSameSegment(_gridModel, coord, stopAtInactiveCell: false)) continue;

                var itemType = _itemDecider.Decide(coord);
                var item= _objectCreateHandler.CreateItem(itemType);

                item.SetParent(_gridView.GridObjectsParent);
                item.SetSpriteSize(cellSize);

                var worldPos = _gridView.GridToWorld(coord);
                var startPos = new Vector3(worldPos.x, spawnWorldY + stack * cellSize, worldPos.z);

                item.SetPosition(startPos);
                _gridModel.SetGridObject(coord, item);

                AddRecord(new FallDownMoveRecord(item, coord, isSpawn: true));
                stack++;
            }
        }
    }
}