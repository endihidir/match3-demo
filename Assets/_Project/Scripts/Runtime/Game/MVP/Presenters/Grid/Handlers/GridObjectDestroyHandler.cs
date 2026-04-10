using Game.Grid.Item;
using Game.Grid.Item.Factories;
using Game.Models;
using Game.Views;

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

        public void DestroyGridObject(BaseGridObject obj)
        {
            PlayBlastFx(obj);
            RemoveObject(obj);
            ReleaseObject(obj);
        }
        
        public void RemoveObject(BaseGridObject obj) => _gridModel.SetGridObject(obj.Coord, null);

        public void PlayBlastFx(BaseGridObject obj)
        {
            var pos = _gridView.GridToWorld(obj.Coord);
            _blastFxHandler.PlayBlastParticle(obj, pos, _gridView.FXParent);
        }
        public void ReleaseObject(BaseGridObject obj) => _gridObjectFactory.ReleaseObject(obj);
    }
}