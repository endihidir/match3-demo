using System.Collections.Generic;
using Core.Utils;
using Game.Configs;
using Game.Grid.Contexts;
using Game.Grid.Item;
using UnityEngine;

namespace Game.Grid.Handlers
{
    public sealed class BoosterActionBuildHandler : IBoosterActionBuildHandler
    {
        private readonly BoosterComboDataSO _comboData;
        private readonly IGridObjectDestroyHandler _destroyHandler;

        public BoosterActionBuildHandler(GridConfigContainerSO gridConfigContainer, IGridObjectDestroyHandler destroyHandler)
        {
            _comboData = gridConfigContainer.GetConfig<BoosterConfigContainerSO>().BoosterComboData;
            _destroyHandler = destroyHandler;
        }

        public void Build(Vector2Int coord, BoosterObject booster, List<BoosterActionContext> output)
        {
            if (!booster || booster.BoosterAction == null) return;

            output.Add(new BoosterActionContext(coord, booster.BoosterAction));
            _destroyHandler.DestroyGridObject(booster, coord);
        }

        public void BuildCombo(Vector2Int coord, BoosterObject source, BoosterObject target, List<BoosterActionContext> output)
        {
            if (_comboData.TryGetRule(source.BoosterType, target.BoosterType, out var rule) && rule.Actions != null)
            {
                foreach (var action in rule.Actions)
                {
                    if (action == null) continue;
                    output.Add(new BoosterActionContext(coord, action));
                }
            }
            else
            {
                EditorLogger.LogError($"{source.BoosterType} - {target.BoosterType} merge rule does not exist!");
            }

            _destroyHandler.DestroyGridObject(source, source.Coord);
            _destroyHandler.DestroyGridObject(target, target.Coord);
        }
    }
}