using System;
using System.Collections.Generic;
using Game.Grid.Handlers.Data;
using Game.Models;
using Game.Views;
using UnityEngine;

namespace Game.Grid.Contexts
{
    public sealed class GridStateContext
    {
        public Queue<GridInputSource> Inputs { get; } = new();
        
        public IGridModel GridModel { get; }
        public IGridView GridView { get; }
        
        public bool MatchResolveRequested { get; set; }
        
        public bool HasPendingBoosterActions => PendingBoosterActions.Count > 0;
        public List<BoosterActionContext> PendingBoosterActions { get; } = new();
        
        public Vector2Int? MergeCenterCoord { get; set; }
        public Vector2Int? UnmarkRemoveCoord { get; set; }
        
        public event Action OnDestructionStateComplete;

        public GridStateContext(IGridModel gridModel, IGridView gridView)
        {
            GridModel = gridModel;
            GridView = gridView;
        }
        
        public void RaiseDestructionStateComplete() => OnDestructionStateComplete?.Invoke();
    }
}