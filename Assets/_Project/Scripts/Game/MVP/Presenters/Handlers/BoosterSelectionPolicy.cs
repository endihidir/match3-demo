using System.Collections.Generic;
using Core.Item;
using Core.Utils;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class BoosterSelectionPolicy : IBoosterSelectionPolicy
    {
        private GridStateContext _context;

        public void Initialize(GridStateContext context) => _context = context;

        public BoosterSpawnResult Decide(GridObjectType[,] grid, List<Vector2Int> matchedCells, int width, int height, int id)
        {
            var best = new BoosterCandidate();

            for (int i = 0; i < matchedCells.Count; i++)
            {
                EvaluateCellForBooster(grid, matchedCells[i], width, height, id, ref best);
            }

            ApplyForcedSpawnCoordIfInGroup(matchedCells, ref best);

            return best.Priority == 0 ? default : new BoosterSpawnResult(true, best.Pos, best.Type, matchedCells);
        }

        private void EvaluateCellForBooster(GridObjectType[,] grid, Vector2Int coord, int width, int height, int id, ref BoosterCandidate best)
        {
            GridMatchDetectUtil.GetLineLengthsAt(grid, coord.x, coord.y, width, height, id, assumeCenterIsId: true, out var h, out var v);

            if (IsFiveOrMoreLine(h, v))
            {
                best.TrySet(5, coord, GetRocketTypeByDominantAxis(h, v));
                return;
            }

            if (IsCrossMatch(h, v))
            {
                best.TrySet(4, coord, BoosterType.Bomb);
                return;
            }

            if (Has2X2Square(grid, coord, width, height, id))
            {
                best.TrySet(3, coord, BoosterType.Bomb);
                return;
            }

            if (IsFourLine(h, v))
            {
                best.TrySet(2, coord, GetRocketTypeByDominantAxis(h, v));
            }
        }

        private bool IsFiveOrMoreLine(int h, int v) => h >= 5 || v >= 5;
        private bool IsCrossMatch(int h, int v) => h >= 3 && v >= 3;
        private bool IsFourLine(int h, int v) => h >= 4 || v >= 4;
        private bool Has2X2Square(GridObjectType[,] grid, Vector2Int coord, int width, int height, int id) => 
            GridMatchDetectUtil.Has2X2Square(grid, coord.x, coord.y, width, height, id, assumeCenterIsId: true);

        private BoosterType GetRocketTypeByDominantAxis(int h, int v) => v >= h ? BoosterType.RocketHorizontal : BoosterType.RocketVertical;
        private void ApplyForcedSpawnCoordIfInGroup(List<Vector2Int> matchedCells, ref BoosterCandidate best)
        {
            if (_context is not { HasForcedBoosterSpawnCoord: true }) return;

            var forced = _context.ForcedBoosterSpawnCoord;

            for (int i = 0; i < matchedCells.Count; i++)
            {
                if (matchedCells[i] != forced) continue;

                _context.HasForcedBoosterSpawnCoord = false;

                best.Pos = forced;
                return;
            }
        }

        private struct BoosterCandidate
        {
            public int Priority;
            public Vector2Int Pos;
            public BoosterType Type;

            public bool TrySet(int priority, Vector2Int pos, BoosterType type)
            {
                if (priority <= Priority) return false;

                Priority = priority;
                Pos = pos;
                Type = type;
                return true;
            }
        }
    }
}
