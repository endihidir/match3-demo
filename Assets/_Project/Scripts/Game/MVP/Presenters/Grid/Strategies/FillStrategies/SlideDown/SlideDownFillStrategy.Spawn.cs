using Game.Grid.Utils;
using UnityEngine;

namespace Game.Grid.Strategies
{
    public sealed partial class SlideDownFillStrategy
    {
        // Spawn
        // =========================================================

        private bool SpawnTopOpenSegment(int x, int height)
        {
            var cellSize = _gridView.GetCellSize();

            var spawnedAny = false;
            var segmentStartY = -1;
            var blockedInSegment = false;
            var segmentTopWorldY = 0f;

            var canSpawn = true;
            var seenAnyActive = false;

            for (int y = 0; y < height; y++)
            {
                var c = new Vector2Int(x, y);

                if (!_gridModel.IsCellActive(c))
                {
                    if (seenAnyActive)
                        canSpawn = false;

                    segmentStartY = -1;
                    blockedInSegment = false;
                    continue;
                }

                seenAnyActive = true;

                if (segmentStartY < 0)
                {
                    segmentStartY = y;
                    segmentTopWorldY = _gridView.GridToWorld(new Vector2Int(x, y)).y + cellSize;
                    blockedInSegment = false;
                }

                if (!canSpawn)
                    continue;

                var obj = _gridModel.GetGridObject(c);

                if (obj)
                {
                    blockedInSegment = true;
                    continue;
                }

                if (blockedInSegment)
                    continue;

                var spawnCount = GridFillCalcUtil.CountEmptiesDown(_gridModel, x, y, height);
                if (spawnCount <= 0)
                    continue;

                SpawnInto(x, y, spawnCount, segmentTopWorldY, cellSize);

                spawnedAny = true;
                y += spawnCount - 1;
            }

            return spawnedAny;
        }
        
        private void SpawnInto(int x, int startY, int spawnCount, float segmentTopWorldY, float cellSize)
        {
            var baseStack = _spawnStackByX[x];

            for (int i = 0; i < spawnCount; i++)
            {
                var target = new Vector2Int(x, startY + i);

                var type = _itemDecider.Decide(target);
                var item = _gridObjectSpawnHandler.GetRegularItem(type);

                item.SetParent(_gridView.GridObjectsParent);
                item.SetSpriteSize(cellSize);

                var w = _gridView.GridToWorld(target);

                var reverseIndex = (spawnCount - 1) - i;
                var spawnY = segmentTopWorldY + (baseStack + reverseIndex) * cellSize;

                item.SetPosition(new Vector3(w.x, spawnY, w.z));

                _gridModel.SetGridObject(target, item);
                AddStep(item, target, true);
            }

            _spawnStackByX[x] = baseStack + spawnCount;
        }

    }
}