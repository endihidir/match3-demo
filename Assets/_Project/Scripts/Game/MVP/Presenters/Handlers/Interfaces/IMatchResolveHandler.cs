using System.Collections.Generic;
using Core.Item;
using UnityEngine;

namespace Core.Handlers
{
    public interface IMatchResolveHandler
    {
        void Initialize(GridStateContext context);
        void Handle(GridStateContext context, GridObjectType[,] typeGrid, List<Vector2Int> group, int width, int height, CellResolveData[,] cells, List<BoosterSpawnResult> spawns);
    }
}