using System.Collections.Generic;
using Core.Item;
using Core.Utils;
using UnityEngine;

namespace Core.Handlers
{
    public interface IBoosterSelectionPolicy
    {
        void Initialize(GridStateContext context);
        BoosterSpawnResult Decide(GridObjectType[,] grid, List<Vector2Int> cells, int width, int height, int id);
    }
    
    public sealed class BoosterSelectionPolicy :  IBoosterSelectionPolicy
    {
        private GridStateContext _context;

        public void Initialize(GridStateContext context) => _context = context;

        public BoosterSpawnResult Decide(GridObjectType[,] grid, List<Vector2Int> cells, int width, int height, int id)
        {
            var bestPriority = 0;
            var bestPos = new Vector2Int(-1, -1);
            var bestType = BoosterType.None;

            for (int i = 0; i < cells.Count; i++)
            {
                var coord = cells[i];

                GridMatchDetectUtil.GetLineLengthsAt(grid, coord.x, coord.y, width, height, id, assumeCenterIsId: true, out var h, out var v);

                if (h >= 5 || v >= 5)
                {
                    var rocket = v >= h ? BoosterType.RocketHorizontal : BoosterType.RocketVertical;

                    TrySet(5, coord, rocket);
                    continue;
                }

                if (h >= 3 && v >= 3)
                {
                    TrySet(4, coord, BoosterType.Bomb);
                    continue;
                }

                if (GridMatchDetectUtil.Has2X2Square(grid, coord.x, coord.y, width, height, id, assumeCenterIsId: true))
                {
                    TrySet(3, coord, BoosterType.Bomb);
                    continue;
                }

                if (h >= 4 || v >= 4)
                {
                    var rocket = v >= h ? BoosterType.RocketHorizontal : BoosterType.RocketVertical;
                    TrySet(2, coord, rocket);
                }
            }

            if (_context is { HasForcedBoosterSpawnCoord: true })
            {
                var forced = _context.ForcedBoosterSpawnCoord;

                for (int i = 0; i < cells.Count; i++)
                {
                    if (cells[i] == forced)
                    {
                        _context.HasForcedBoosterSpawnCoord = false;
                        bestPos = forced;
                        break;
                    }
                }
            }

            return bestPriority == 0 ? default : new BoosterSpawnResult(true, bestPos, bestType, cells);

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