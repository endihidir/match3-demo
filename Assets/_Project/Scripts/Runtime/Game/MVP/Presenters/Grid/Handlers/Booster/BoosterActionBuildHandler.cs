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

        public void Build(BoosterObject booster, List<BoosterActionContext> output)
        {
            if (!booster || booster.BoosterAction == null) return;

            var actionContext = new BoosterActionContext(booster.Coord, booster.BoosterAction);
            
            output.Add(actionContext);
            
            _destroyHandler.DestroyGridObject(booster);
        }

        public void BuildCombo(BoosterObject source, BoosterObject target, List<BoosterActionContext> output)
        {
            if (_comboData.TryGetRule(source.BoosterType, target.BoosterType, out var rule) && rule.Actions != null)
            {
                foreach (var action in rule.Actions)
                {
                    if (action == null) continue;
                    
                    var actionContext = new BoosterActionContext(source.Coord, action);
                    
                    output.Add(actionContext);
                }
            }
            else
            {
                EditorLogger.LogError($"{source.BoosterType} - {target.BoosterType} merge rule does not exist!");
            }

            _destroyHandler.DestroyGridObject(source);
            _destroyHandler.DestroyGridObject(target);
        }
    }
}