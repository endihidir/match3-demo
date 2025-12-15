using System;
using Core.Item;

namespace Core.Utils
{
    public static class InitialMatchCleanupUtil
    {
        public static void RemoveInitialMatches(GridObjectTypeData[,] grid, bool[,] randomMask, int width, int height, Random rng)
        {
            bool found;

            do
            {
                found = false;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (!randomMask[x, y]) continue;

                        if (!IsRegularItem(grid[x, y])) continue;

                        if (HasMatchAt(grid, x, y, width, height, grid[x, y].TypeId, assumeCenterIsId: false))
                        {
                            grid[x, y] = CreateRandomNonMatchingCell(grid, x, y, width, height, rng);
                            found = true;
                        }
                    }
                }

            } while (found);
        }

        private static GridObjectTypeData CreateRandomNonMatchingCell(GridObjectTypeData[,] grid, int x, int y, int width, int height, Random rng)
        {
            var ids = LevelGridRandomUtil.GetItemTypeIds();

            Span<int> candidates = stackalloc int[ids.Length];
            var candidateCount = 0;

            for (int i = 0; i < ids.Length; i++)
            {
                var id = ids[i];

                if (!HasMatchAt(grid, x, y, width, height, id, assumeCenterIsId: true))
                {
                    candidates[candidateCount] = id;
                    candidateCount++;
                }
            }

            if (candidateCount > 0)
            {
                var pick = LevelGridRandomUtil.NextIndex(rng, candidateCount);
                return new GridObjectTypeData(GridItemKind.Regular, candidates[pick]);
            }

            var attempts = 0;

            while (true)
            {
                var id = ids[LevelGridRandomUtil.NextIndex(rng, ids.Length)];

                if (!HasMatchAt(grid, x, y, width, height, id, assumeCenterIsId: true))
                    return new GridObjectTypeData(GridItemKind.Regular, id);

                attempts++;

                if (attempts > 64)
                    return new GridObjectTypeData(GridItemKind.Regular, id);
            }
        }

        private static bool HasMatchAt(GridObjectTypeData[,] grid, int x, int y, int width, int height, int id, bool assumeCenterIsId)
        {
            var centerCountsAsOne = assumeCenterIsId ? 1 : (IsRegularItem(grid[x, y]) && grid[x, y].TypeId == id ? 1 : 0);

            var left = CountSame(grid, x, y, -1, 0, width, height, id);
            var right = CountSame(grid, x, y, 1, 0, width, height, id);

            if (left + centerCountsAsOne + right >= 3) return true;

            var down = CountSame(grid, x, y, 0, -1, width, height, id);
            var up = CountSame(grid, x, y, 0, 1, width, height, id);

            return down + centerCountsAsOne + up >= 3;
        }

        private static int CountSame(GridObjectTypeData[,] grid, int x, int y, int dx, int dy, int width, int height, int id)
        {
            var count = 0;

            var cx = x + dx;
            var cy = y + dy;

            while (cx >= 0 && cx < width && cy >= 0 && cy < height)
            {
                var data = grid[cx, cy];

                if (!IsRegularItem(data))
                    break;

                if (data.TypeId != id)
                    break;

                count++;

                cx += dx;
                cy += dy;
            }

            return count;
        }

        private static bool IsRegularItem(GridObjectTypeData data) => data is { ItemKind: GridItemKind.Regular, TypeId: > 0 };
    }
}