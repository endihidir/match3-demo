using Core.Utils;
using Game.Grid.Utils;
using UnityEngine;

namespace Game.Grid.Strategies
{
    public sealed partial class SlideDownFillStrategy
    {
        /// <summary>
        /// Fills all contiguous top-open empty cells in column <paramref name="x"/>
        /// with newly created items.  Returns true if at least one item was spawned.
        /// </summary>
        private bool SpawnColumn(int x, int height)
        {
            var cellSize = _gridView.GetCellSize();
            var spawnedAny = false;
            var segmentOpen = false;
            var segmentBlocked = false;
            var segmentTopWorldY = 0f;
            var seenActiveBefore = false;

            for (int y = 0; y < height; y++)
            {
                var c = new Vector2Int(x, y);

                if (!_gridModel.IsCellActive(c))
                {
                    if (seenActiveBefore) return spawnedAny;
                    continue;
                }

                if (!segmentOpen)
                {
                    segmentOpen = true;
                    seenActiveBefore = true;
                    segmentTopWorldY = _gridView.GridToWorld(c).y + cellSize;
                }

                var obj = _gridModel.GetGridObject(c);
                if (obj) { segmentBlocked = true; continue; }
                if (segmentBlocked) continue;

                var runLength = GridFillCalcUtil.CountEmptiesDown(_gridModel, x, y, height);
                if (runLength <= 0) continue;

                spawnedAny |= SpawnRun(x, y, runLength, segmentTopWorldY, cellSize);
                y += runLength - 1;
            }

            return spawnedAny;
        }

        private bool SpawnRun(int x, int startY, int count, float segmentTopWorldY, float cellSize)
        {
            var baseStack = _spawnStackByX[x];
            var spawnedAny = false;

            for (int i = 0; i < count; i++)
            {
                var target = new Vector2Int(x, startY + i);
                var type = _itemDecider.Decide(target);
                var item= _objectCreateHandler.CreateItem(type);

                if (!item)
                {
                    EditorLogger.LogError($"[SlideDownFillStrategy] Item could not be created: {type} at {target}");
                    continue;
                }

                item.SetParent(_gridView.GridObjectsParent);
                item.SetSpriteSize(cellSize);

                var worldTarget  = _gridView.GridToWorld(target);
                var reverseIndex = (count - 1) - i;
                var spawnWorldY= segmentTopWorldY + (baseStack + reverseIndex) * cellSize;

                item.SetPosition(new Vector3(worldTarget.x, spawnWorldY, worldTarget.z));

                _gridModel.SetGridObject(target, item);
                AddStep(item, target, isSpawn: true);
                spawnedAny = true;
            }

            _spawnStackByX[x] = baseStack + count;
            return spawnedAny;
        }
    }
}