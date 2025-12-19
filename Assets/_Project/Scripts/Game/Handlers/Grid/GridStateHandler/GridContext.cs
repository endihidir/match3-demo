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

        public GridContext(IGridModel model, IGridView view, IGridItemFactory factory, Queue<GridMove> moveQueue)
        {
            Model = model;
            View = view;
            Factory = factory;
            MoveQueue = moveQueue;
        }
    }
}