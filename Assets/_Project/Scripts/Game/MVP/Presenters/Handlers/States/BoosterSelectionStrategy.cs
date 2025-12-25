using System.Collections.Generic;
using Core.Item;
using Core.Utils;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class BoosterSelectionStrategy
    {
        private readonly GridStateContext _context;

        public BoosterSelectionStrategy(GridStateContext context)
        {
            _context = context;
        }

        public BoosterSpawn Decide(GridObjectType[,] grid, List<Vector2Int> cells, int width, int height, int id)
        {
            var bestPriority = 0;
            var bestPos = new Vector2Int(-1, -1);
            var bestType = BoosterType.None;

            for (int i = 0; i < cells.Count; i++)
            {
                var c = cells[i];

                GridMatchDetectUtil.GetLineLengthsAt(grid, c.x, c.y, width, height, id, assumeCenterIsId: true, out var h, out var v);

                if (h >= 5 || v >= 5)
                {
                    var rocket = v >= h ? BoosterType.RocketHorizontal : BoosterType.RocketVertical;

                    TrySet(5, c, rocket);
                    continue;
                }

                if (h >= 3 && v >= 3)
                {
                    TrySet(4, c, BoosterType.Bomb);
                    continue;
                }

                if (GridMatchDetectUtil.Has2x2Square(grid, c.x, c.y, width, height, id, assumeCenterIsId: true))
                {
                    TrySet(3, c, BoosterType.Bomb);
                    continue;
                }

                if (h >= 4 || v >= 4)
                {
                    var rocket = v >= h ? BoosterType.RocketHorizontal : BoosterType.RocketVertical;

                    TrySet(2, c, rocket);
                }
            }

            if (_context.IsForcedBoosterSpawnPos)
            {
                var forced = _context.ForcedBoosterSpawnPos;

                for (int i = 0; i < cells.Count; i++)
                {
                    if (cells[i] == forced)
                    {
                        _context.IsForcedBoosterSpawnPos = false;
                        bestPos = forced;
                        break;
                    }
                }
            }

            return bestPriority == 0 ? default : new BoosterSpawn(true, bestPos, bestType, cells);

            void TrySet(int priority, Vector2Int pos, BoosterType type)
            {
                if (priority <= bestPriority) return;

                bestPriority = priority;
                bestPos = pos;
                bestType = type;
            }
        }
    }
}