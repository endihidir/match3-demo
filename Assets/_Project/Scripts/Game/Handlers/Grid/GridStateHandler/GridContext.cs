using System.Collections.Generic;
using Core.Config;
using Core.Item;
using Core.Item.Factories;
using Core.Models;
using Core.Views;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class GridContext
    {
        public IGridModel Model { get; }
        public IGridView View { get; }
        public IGridItemFactory Factory { get; }
        public ItemConfigContainer Configs { get; }
        public Queue<GridMove> MoveQueue { get; }

        public bool ResolvedAnyMatch { get; set; }
        public bool CascadeInProgress { get; set; }
        public bool CascadeResolveRequested { get; set; }
        
        public bool IsForcedBoosterSpawnPos { get; set; }
        public Vector2Int ForcedBoosterSpawnPos { get; set; }
        public List<PendingEffect> PendingEffects { get; } = new();

        public GridContext(IGridModel model, IGridView view, IGridItemFactory factory, ItemConfigContainer configs, Queue<GridMove> moveQueue)
        {
            Model = model;
            View = view;
            Factory = factory;
            Configs = configs;
            MoveQueue = moveQueue;
        }

        public readonly struct PendingEffect
        {
            public readonly Vector2Int Origin;
            public readonly BoosterEffectBase BoosterEffect;

            public PendingEffect(Vector2Int origin, BoosterEffectBase boosterEffect)
            {
                Origin = origin;
                BoosterEffect = boosterEffect;
            }
        }
    }
}