using Game.Grid.Item;
using Game.Grid.Item.Factories;
using Game.Models;
using Game.Views;
using UnityEngine;

namespace Game.Grid.Handlers
{
    public sealed class GridObjectDestroyHandler : IGridObjectDestroyHandler
    {
        private readonly IGridModel _gridModel;
        private readonly IGridView _gridView;
        private readonly IGridObjectFactory _gridObjectFactory;
        private readonly IBlastFxHandler _blastFxHandler;

        public GridObjectDestroyHandler(IGridModel gridModel, IGridView gridView, IGridObjectFactory gridObjectFactory,
            IBlastFxHandler blastFxHandler)
        {
            _gridModel = gridModel;
            _gridView = gridView;
            _gridObjectFactory = gridObjectFactory;
            _blastFxHandler = blastFxHandler;
        }

        public void DestroyGridObject(BaseGridObject obj, Vector2Int coord)
        {
            _blastFxHandler.PlayBlastParticle(obj, _gridView.GridToWorld(coord), _gridView.FXParent);
            _gridObjectFactory.ReleaseObject(obj);
            _gridModel.SetGridObject(coord, null);
        }

        public void SetNull(Vector2Int coord) => _gridModel.SetGridObject(coord, null);
        public void ReleaseObject(BaseGridObject obj) => _gridObjectFactory.ReleaseObject(obj);
    }
}