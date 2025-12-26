using System.Collections.Generic;
using Core.Item;
using UnityEngine;

namespace Core.Handlers
{
    public interface IBoosterSelectionHandler
    {
        void Initialize(GridStateContext context);
        BoosterSpawnResult Decide(GridObjectType[,] grid, List<Vector2Int> cells, int width, int height, int id);
    }
}