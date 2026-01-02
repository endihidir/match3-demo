using System.Collections.Generic;
using Core.Config;
using Core.Item.Factories;
using Core.Models;
using Core.Utils;
using Core.Views;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class GridStateContext
    {
        public IGridModel Model { get; }
        public IGridView View { get; }
        public IGridItemFactory Factory { get; }
        public ItemConfigContainerSO Configs { get; }
        public Queue<InputSource> Inputs { get; } = new();
        
        public bool HasForcedBoosterSpawnCoord { get; set; }
        public Vector2Int ForcedBoosterSpawnCoord { get; set; }
        public List<PendingBoosterAction> PendingBoosterActions { get; } = new();
        
        public bool RefillInProgress { get; set; }
        public bool MatchResolveRequested { get; set; }
        public bool HasPendingBoosterActions => PendingBoosterActions.Count > 0;
        public bool HasAnyEmptyActiveCell => GridRefillCalcUtils.HasAnyEmptyActiveCell(Model);

        public GridStateContext(IGridModel model, IGridView view, IGridItemFactory factory, ItemConfigContainerSO configs)
        {
            Model = model;
            View = view;
            Factory = factory;
            Configs = configs;
        }
    }
}