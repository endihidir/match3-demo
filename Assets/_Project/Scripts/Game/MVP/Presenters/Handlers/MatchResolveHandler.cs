using System.Collections.Generic;
using Core.Item;
using Core.Utils;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class MatchResolveHandler : IMatchResolveHandler
    {
        private readonly BoosterSelectionStrategy _boosterSelection;
        private readonly MatchDamageStrategy _matchDamage;
        private readonly SpawnOverrideStrategy _spawnOverride;

        public MatchResolveHandler(GridStateContext context)
        {
            _boosterSelection = new BoosterSelectionStrategy(context);
            _matchDamage = new MatchDamageStrategy();
            _spawnOverride = new SpawnOverrideStrategy();
        }

        public bool CanHandle(GridObjectType startData) => GridMatchDetectUtil.IsRegularItem(startData);

        public void Handle(GridStateContext context, GridObjectType[,] typeGrid, List<Vector2Int> group, int width, int height, CellResolveData[,] cells, 
            List<BoosterSpawnResult> spawns)
        {
            if (group == null || group.Count == 0) return;

            var id = typeGrid[group[0].x, group[0].y].TypeId;

            var spawn = _boosterSelection.Decide(typeGrid, group, width, height, id);

            _matchDamage.Apply(cells, width, height, group);

            if (spawn.HasSpawn)
            {
                spawns.Add(spawn);
                _spawnOverride.Apply(cells, group);
            }
        }
    }
}