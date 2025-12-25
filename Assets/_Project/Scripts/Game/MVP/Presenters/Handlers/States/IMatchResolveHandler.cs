using System.Collections.Generic;
using Core.Item;
using UnityEngine;

namespace Core.Handlers
{
    public interface IMatchResolveHandler
    {
        bool CanHandle(GridObjectType startData);

        void Handle(GridStateContext context, GridObjectType[,] typeGrid, List<Vector2Int> group, int width, int height, CellResolveData[,] cells, List<BoosterSpawn> spawns);
    }
}