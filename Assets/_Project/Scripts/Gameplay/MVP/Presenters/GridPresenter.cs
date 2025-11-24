using Core.Systems;
using Core.Item;
using Core.MVPContext.Interfaces;
using UnityEngine;

namespace Core.SceneService
{
    public interface IGridPresenter : IPresenter
    {
        void Initialize(IGridModel gridModel);
        IGridItemBehaviour GetItem(Vector2Int pos);

        bool TryGetNeighbor(IGridItemBehaviour ıtem, Direction2D direction, out IGridItemBehaviour neighbour);
        bool TryGetNeighbors(IGridItemBehaviour ıtem, out IGridItemBehaviour[] neighbours);

        void Swap(IGridItemBehaviour a, IGridItemBehaviour b);
        void Move(IGridItemBehaviour ıtem, Vector2Int to);
        void RemoveAt(Vector2Int pos);
        void InsertAt(Vector2Int pos, IGridItemBehaviour ıtem);
    }
    
    public class GridPresenter : IGridPresenter
    {
        private IGridModel _gridModel;

        public GridPresenter()
        {
            //_grid = new UIGrid<GridItemModel>();
        }
        public void Initialize(IGridModel gridModel)
        {
            _gridModel = gridModel;
        }

        public IGridItemBehaviour GetItem(Vector2Int pos)
        {
            return _gridModel.GetGridObject(pos);
        }

        public bool TryGetNeighbor(IGridItemBehaviour ıtem, Direction2D direction, out IGridItemBehaviour neighbour)
        {
            neighbour = null;
            
            if (ıtem == null) return false;

            return _gridModel.TryGetNeighbor(ıtem.GridPos, direction, out neighbour);
        }

        public bool TryGetNeighbors(IGridItemBehaviour ıtem, out IGridItemBehaviour[] neighbours)
        {
            neighbours = null;
            
            if (ıtem == null) return false;

            return _gridModel.TryGetNeighbors(ıtem.GridPos, out neighbours);
        }

        public void Swap(IGridItemBehaviour a, IGridItemBehaviour b)
        {
            if (a == null || b == null) return;

            var posA = a.GridPos;
            var posB = b.GridPos;

            if (!_gridModel.IsInRange(posA) || !_gridModel.IsInRange(posB)) return;
            
            _gridModel.SetGridObject(posA, b);
            _gridModel.SetGridObject(posB, a);
            
            
        }

        public void Move(IGridItemBehaviour ıtem, Vector2Int to)
        {
            if (ıtem == null) return;

            var from = ıtem.GridPos;

            if (!_gridModel.IsInRange(from) || !_gridModel.IsInRange(to)) return;
            
            _gridModel.SetGridObject(from, null);
            _gridModel.SetGridObject(to, ıtem);

            ıtem.SetGridPos(to);
        }

        public void RemoveAt(Vector2Int pos)
        {
            if (!_gridModel.IsInRange(pos)) return;

            var item = _gridModel.GetGridObject(pos);
            if (item == null) return;

            _gridModel.SetGridObject(pos, null);
            
            item.SetGridPos(new Vector2Int(-1, -1));
        }

        public void InsertAt(Vector2Int pos, IGridItemBehaviour ıtem)
        {
            if (ıtem == null) return;
            if (!_gridModel.IsInRange(pos)) return;

            _gridModel.SetGridObject(pos, ıtem);
            ıtem.SetGridPos(pos);

            /*if (item.Transform)
            {
                item.Transform.position = _grid.GridToWorld(pos);
            }*/
        }
    }
}
