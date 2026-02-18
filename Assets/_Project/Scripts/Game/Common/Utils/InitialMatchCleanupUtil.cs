using System;
using Game.Grid.Item;

namespace Game.Utils
{
    public static class InitialMatchCleanupUtil
    {
        public static void RemoveInitialMatches(GridObjectType[,] grid, bool[,] randomMask, int width, int height, Random rng)
        {
            bool found;

            do
            {
                found = false;

                for (int i = 0; i < width * height; i++)
                {
                    var coord = GridIndexUtil.ToCoord(i, width);
                    var x = coord.x;
                    var y = coord.y;
            
                    if (!randomMask[x, y]) continue;
                    if (!GridMatchDetectUtil.IsRegularItem(grid[x, y])) continue;

                    if (GridMatchDetectUtil.WouldCreateBlastGroup(grid, x, y, width, height, grid[x, y].TypeId, assumeCenterIsId: false))
                    {
                        grid[x, y] = CreateRandomNonGroupingCell(grid, x, y, width, height, rng);
                        found = true;
                    }
                }

            } while (found);
        }

        private static GridObjectType CreateRandomNonGroupingCell(GridObjectType[,] grid, int x, int y, int width, int height, Random rng)
        {
            var ids = LevelGridRandomUtil.GetItemTypeIds();

            Span<int> candidates = stackalloc int[ids.Length];
            var candidateCount = 0;

            for (int i = 0; i < ids.Length; i++)
            {
                var id = ids[i];

                if (!GridMatchDetectUtil.WouldCreateBlastGroup(grid, x, y, width, height, id, assumeCenterIsId: true))
                {
                    candidates[candidateCount] = id;
                    candidateCount++;
                }
            }

            if (candidateCount > 0)
            {
                var pick = LevelGridRandomUtil.NextIndex(rng, candidateCount);
                return new GridObjectType(GridItemKind.Regular, candidates[pick]);
            }
            
            var attempts = 0;

            while (true)
            {
                var id = ids[LevelGridRandomUtil.NextIndex(rng, ids.Length)];

                if (!GridMatchDetectUtil.WouldCreateBlastGroup(grid, x, y, width, height, id, assumeCenterIsId: true))
                    return new GridObjectType(GridItemKind.Regular, id);

                attempts++;

                if (attempts > 64)
                    return new GridObjectType(GridItemKind.Regular, id);
            }
        }
    }
}
