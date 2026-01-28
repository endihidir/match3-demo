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
        public IGridModel GridModel { get; }
        public IGridView GridView { get; }
        public IGridItemFactory GridItemFactory { get; }
        public ILevelGoalModel LevelGoalModel { get; }
        public GridConfigContainerSO GridConfigs { get; }
        public Queue<GridInputSource> Inputs { get; } = new();
        
        public bool HasMergeCenterCoordRequested { get; set; }
        public Vector2Int MergeCenterCoord { get; set; }
        
        public bool HasUnmarkRemoveRequested { get; set; }
        public Vector2Int UnmarkRemoveCoord { get; set; }
        
        public List<BoosterActionContext> PendingBoosterActions { get; } = new();
        public bool HasPendingBoosterActions => PendingBoosterActions.Count > 0;
        
        public bool MatchResolveRequested { get; set; }
        public int GroupIdCounter { get; set; }

        public GridStateContext(IGridModel gridModel, IGridView gridView, IGridItemFactory gridItemFactory, ILevelGoalModel levelGoalModel, 
            GridConfigContainerSO gridConfigs)
        {
            GridModel = gridModel;
            GridView = gridView;
            GridItemFactory = gridItemFactory;
            LevelGoalModel = levelGoalModel;
            GridConfigs = gridConfigs;
        }
        
        public int NextBoosterGroupId()
        {
            GroupIdCounter++;
            if (GroupIdCounter == int.MaxValue) GroupIdCounter = 1;
            return GroupIdCounter;
        }
        
        public void ReleaseAndSetNull(BaseGridObject sourceObj, Vector2Int sourceCoord)
        {
            GridItemFactory.ReleaseItem(sourceObj);
            GridModel.SetGridObject(sourceCoord, null);
        }
    }
}