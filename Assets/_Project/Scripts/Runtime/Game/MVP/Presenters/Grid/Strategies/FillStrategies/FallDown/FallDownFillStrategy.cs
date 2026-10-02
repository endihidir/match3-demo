using Cysharp.Threading.Tasks;
using Game.Grid.Handlers;
using Game.Grid.Utils;
using Game.Models;
using Game.Views;

namespace Game.Grid.Strategies
{
    public sealed partial class FallDownFillStrategy : IFillStrategy
    {
        private readonly IGridModel _gridModel;
        private readonly IGridView _gridView;
        private readonly IGridObjectCreateHandler _objectCreateHandler;
        private readonly IFillItemDecider _itemDecider;
        private readonly IFillMotionPlanner _motionPlanner;

        private UniTask _pendingAnimations = UniTask.CompletedTask;

        public FallDownFillStrategy(IGridModel gridModel, IGridView gridView, IGridObjectCreateHandler objectCreateHandler, IFillItemDecider itemDecider,
            IFillMotionPlanner motionPlanner)
        {
            _gridModel = gridModel;
            _gridView = gridView;
            _objectCreateHandler = objectCreateHandler;
            _itemDecider = itemDecider;
            _motionPlanner = motionPlanner;
        }

        public bool CanHandle() => !GridFillCalcUtil.HasStationaryWithMovableSpaceBelow(_gridModel);

        public IFillStrategy Execute()
        {
            var width = _gridModel.Width;
            var height = _gridModel.Height;
            var cellSize = _gridView.GetCellSize();

            _motionPlanner.Begin();

            for (int x = 0; x < width; x++)
                ShiftColumn(x, height);

            for (int x = 0; x < width; x++)
                FillColumn(x, height, cellSize);

            _pendingAnimations = _motionPlanner.Commit();
            return this;
        }

        public UniTask WaitAnimationsAsync() => _pendingAnimations;
    }
}