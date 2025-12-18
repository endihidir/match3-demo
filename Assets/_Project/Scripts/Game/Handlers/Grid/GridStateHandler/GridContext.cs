using System.Collections.Generic;
using Core.Item.Factories;
using Core.Models;
using Core.Views;

namespace Core.Handlers
{
    public sealed class GridContext
    {
        public IGridModel Model { get; }
        public IGridView View { get; }
        public Queue<GridMove> MoveQueue { get; }
        public IGridItemFactory Factory { get; }

        public bool ResolvedAnyMatch { get; set; }
        public bool CascadeInProgress { get; set; }
        public bool CascadeResolveRequested { get; set; }

        public GridContext(IGridModel model, IGridView view, Queue<GridMove> moveQueue, IGridItemFactory factory)
        {
            Model = model;
            View = view;
            MoveQueue = moveQueue;
            Factory = factory;
        }
    }
}