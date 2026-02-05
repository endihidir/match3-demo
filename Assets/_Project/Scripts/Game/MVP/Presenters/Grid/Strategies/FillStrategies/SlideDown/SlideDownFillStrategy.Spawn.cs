using Core.Extensions;
using Core.Models;
using Core.Utils;
using Core.Views;
using UnityEngine;

namespace Core.Handlers
{
    public partial class SlideDownFillStrategy
    {
        // Spawn
        // =========================================================

        private bool SpawnTopOpenSegment(GridStateContext context, int x, int height)
        {
            var model = context.GridModel;
            var view = context.GridView;
            var cellSize = view.GetCellSize();

            var spawnedAny = false;
            var segmentStartY = -1;
            var blockedInSegment = false;
            var segmentTopWorldY = 0f;

            var canSpawn = true;
            var seenAnyActive = false;

            for (int y = 0; y < height; y++)
            {
                var c = new Vector2Int(x, y);

                if (!model.IsCellActive(c))
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
                    segmentTopWorldY = view.GridToWorld(new Vector2Int(x, y)).y + cellSize;
                    blockedInSegment = false;
                }

                if (!canSpawn)
                    continue;

                var obj = model.GetGridObject(c);

                if (obj)
                {
                    blockedInSegment = true;
                    continue;
                }

                if (blockedInSegment)
                    continue;

                var spawnCount = GridFillCalcUtil.CountEmptiesDown(model, x, y, height);
                if (spawnCount <= 0)
                    continue;

                SpawnInto(model, context, view, x, y, spawnCount, segmentTopWorldY, cellSize);

                spawnedAny = true;
                y += spawnCount - 1;
            }

            return spawnedAny;
        }
        
        private void SpawnInto(IGridModel model, GridStateContext context, IGridView view, int x, int startY, int spawnCount, float segmentTopWorldY, float cellSize)
        {
            var baseStack = _spawnStackByX[x];

            for (int i = 0; i < spawnCount; i++)
            {
                var target = new Vector2Int(x, startY + i);

                var type = _itemDecider.Decide(model, target);
                var item = context.GridItemFactory.GetRegularItem(type);

                item.SetParent(view.GridObjectsParent);
                item.SetSpriteSize(cellSize);

                var w = view.GridToWorld(target);

                var reverseIndex = (spawnCount - 1) - i;
                var spawnY = segmentTopWorldY + (baseStack + reverseIndex) * cellSize;

                item.SetPosition(new Vector3(w.x, spawnY, w.z));

                model.SetGridObject(target, item);
                AddStep(item, target, true);
            }

            _spawnStackByX[x] = baseStack + spawnCount;
        }

    }
}