using System.Collections.Generic;
using Core.Models;
using Core.Views;

namespace Core.Handlers
{
    public sealed class GridContext
    {
        public readonly IGridModel Model;
        public readonly IGridView View;
        public readonly Queue<GridMove> MoveQueue;

        public GridContext(IGridModel model, IGridView view, Queue<GridMove> moveQueue)
        {
            Model = model;
            View = view;
            MoveQueue = moveQueue;
        }
    }
}