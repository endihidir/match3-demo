using System;
using System.Collections.Generic;
using Core.Configs;
using Core.Item;
using Core.Item.Factories;
using Core.Models;
using Core.Views;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class GridStateContext
    {
        public Queue<GridInputSource> Inputs { get; } = new();
        
        public IGridModel GridModel { get; }
        public IGridView GridView { get; }
        public IGridItemFactory GridItemFactory { get; }
        public ILevelObjectiveModel LevelObjectiveModel { get; }
        public GridConfigContainerSO GridConfigs { get; }
        public IBlastFxHandler BlastFxHandler { get; }
        
        public bool HasMergeCenterCoordRequested { get; set; }
        public Vector2Int MergeCenterCoord { get; set; }
        
        public bool HasUnmarkRemoveRequested { get; set; }
        public Vector2Int UnmarkRemoveCoord { get; set; }
        
        public bool HasPendingBoosterActions => PendingBoosterActions.Count > 0;
        public List<BoosterActionContext> PendingBoosterActions { get; } = new();
        
        public bool MatchResolveRequested { get; set; }
        public int GroupIdCounter { get; set; }
        
        public event Action OnAnyDestructionComplete;
        public event Action OnAnyObstacleDestroyed;

        public GridStateContext(IGridModel gridModel, IGridView gridView, IGridItemFactory gridItemFactory, ILevelObjectiveModel levelObjectiveModel, 
            GridConfigContainerSO gridConfigs, IBlastFxHandler blastFxHandler)
        {
            GridModel = gridModel;
            GridView = gridView;
            GridItemFactory = gridItemFactory;
            LevelObjectiveModel = levelObjectiveModel;
            GridConfigs = gridConfigs;
            BlastFxHandler = blastFxHandler;
        }
        
        public int NextBoosterGroupId()
        {
            GroupIdCounter++;
            if (GroupIdCounter == int.MaxValue) GroupIdCounter = 1;
            return GroupIdCounter;
        }
        
        public void ReleaseAndSetNull(BaseGridObject obj, Vector2Int coord)
        {
            BlastFxHandler.PlayBlastParticle(obj, GridView.GridToWorld(coord), GridView.FXParent);
            GridItemFactory.ReleaseItem(obj);
            GridModel.SetGridObject(coord, null);
        }

        public void ProgressGoal(IDamageableGridObject damageableGridObject, Vector2Int coord, Vector2 spriteSize)
        {
            var worldPos = GridView.GridToWorld(coord);
            var size = GridView.SpriteToUISize(spriteSize);
            LevelObjectiveModel.ProgressGoal(damageableGridObject, worldPos, size);
        }
        
        public void RaiseAnyObstacleDestroyed() => OnAnyObstacleDestroyed?.Invoke();
        public void RaiseDestructionComplete() => OnAnyDestructionComplete?.Invoke();
    }
}