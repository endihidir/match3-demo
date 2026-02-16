using System;
using System.Collections.Generic;
using Core.Configs;
using Core.Item;
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
        public IGridObjectHandler GridObjectHandler { get; }
        public ILevelObjectiveModel LevelObjectiveModel { get; }
        public GridConfigContainerSO GridConfigs { get; }
        public IBlastFxHandler BlastFxHandler { get; }
        
        public bool MatchResolveRequested { get; set; }
        
        public bool HasPendingBoosterActions => PendingBoosterActions.Count > 0;
        public List<BoosterActionContext> PendingBoosterActions { get; } = new();
        
        public Vector2Int? MergeCenterCoord { get; set; }
        public Vector2Int? UnmarkRemoveCoord { get; set; }
        
        public event Action OnDestructionStateComplete;

        public GridStateContext(IGridModel gridModel, IGridView gridView, IGridObjectHandler gridObjectHandler, ILevelObjectiveModel levelObjectiveModel, 
            GridConfigContainerSO gridConfigs, IBlastFxHandler blastFxHandler)
        {
            GridModel = gridModel;
            GridView = gridView;
            GridObjectHandler = gridObjectHandler;
            LevelObjectiveModel = levelObjectiveModel;
            GridConfigs = gridConfigs;
            BlastFxHandler = blastFxHandler;
        }
        
        public void ReleaseAndSetNull(BaseGridObject obj, Vector2Int coord)
        {
            BlastFxHandler.PlayBlastParticle(obj, GridView.GridToWorld(coord), GridView.FXParent);
            GridObjectHandler.ReleaseItem(obj);
            GridModel.SetGridObject(coord, null);
        }

        public void ProgressGoal(IDamageableGridObject damageableGridObject, Vector2Int coord, Vector2 spriteSize)
        {
            var worldPos = GridView.GridToWorld(coord);
            var rectSize = GridView.SpriteToRectSize(spriteSize);
            LevelObjectiveModel.ProgressGoal(damageableGridObject, worldPos, rectSize);
        }
        
        public void RaiseDestructionStateComplete() => OnDestructionStateComplete?.Invoke();
    }
}