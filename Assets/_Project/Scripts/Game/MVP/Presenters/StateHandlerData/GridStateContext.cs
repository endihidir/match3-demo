using System.Collections.Generic;
using Core.Config;
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
        public ItemConfigContainer Configs { get; }
        public Queue<GridMove> MoveQueue { get; }

        public bool ResolvedAnyMatch { get; set; }
        public bool RefillInProgress { get; set; }
        public bool RefillResolveRequested { get; set; }
        
        public bool HasForcedBoosterSpawnCoord { get; set; }
        public Vector2Int ForcedBoosterSpawnCoord { get; set; }
        public List<PendingEffect> PendingEffects { get; } = new();

        public GridStateContext(IGridModel model, IGridView view, IGridItemFactory factory, ItemConfigContainer configs, Queue<GridMove> moveQueue)
        {
            Model = model;
            View = view;
            Factory = factory;
            Configs = configs;
            MoveQueue = moveQueue;
        }
    }
}