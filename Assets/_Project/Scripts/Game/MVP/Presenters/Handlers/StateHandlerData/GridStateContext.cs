using System.Collections.Generic;
using Core.Config;
using Core.Item;
using Core.Item.Factories;
using Core.Models;
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
        public List<BoosterActionContext> PendingBoosterActions { get; } = new();
        
        public bool MatchResolveRequested { get; set; }
        public bool HasPendingBoosterActions => PendingBoosterActions.Count > 0;
        public int GroupIdCounter { get; private set; }

        public GridStateContext(IGridModel model, IGridView view, IGridItemFactory factory, ItemConfigContainerSO configs)
        {
            Model = model;
            View = view;
            Factory = factory;
            Configs = configs;
        }
        
        public int NextBoosterGroupId()
        {
            GroupIdCounter++;
            if (GroupIdCounter == int.MaxValue) GroupIdCounter = 1;
            return GroupIdCounter;
        }
        
        public void ReleaseAndSetNull(BaseGridObject sourceObj, Vector2Int sourceCoord)
        {
            Factory.ReleaseItem(sourceObj);
            Model.SetGridObject(sourceCoord, null);
        }
    }
}