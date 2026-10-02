using Cysharp.Threading.Tasks;
using Game.Grid.Handlers;
using Game.Grid.Strategies.Schedulers;
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
        private readonly IFallAnimationScheduler _fallAnimationScheduler;

        public FallDownFillStrategy(IGridModel gridModel, IGridView gridView, IGridObjectCreateHandler objectCreateHandler, IFillItemDecider itemDecider,
            IFallAnimationScheduler fallAnimationScheduler)
        {
            _gridModel = gridModel;
            _gridView = gridView;
            _objectCreateHandler = objectCreateHandler;
            _itemDecider = itemDecider;
            _fallAnimationScheduler = fallAnimationScheduler;
        }

        public bool CanHandle() => !GridFillCalcUtil.HasStationaryWithMovableSpaceBelow(_gridModel);

        public IFillStrategy Execute()
        {
            var width= _gridModel.Width;
            var height= _gridModel.Height;
            var cellSize= _gridView.GetCellSize();

            PrepareForRun(width, height);

            for (int x = 0; x < width; x++)
                ShiftColumn(x, height);

            for (int x = 0; x < width; x++)
            {
                if (GridFillCalcUtil.TryGetSpawnCellCoord(_gridModel, x, out var spawnCell))
                {
                    var spawnY = _gridView.GridToWorld(spawnCell).y + cellSize;
                    FillColumn(x, height, cellSize, spawnY);
                }
            }

            _pendingAnimations = ScheduleAnimations();
            return this;
        }

        public UniTask WaitAnimationsAsync() => _pendingAnimations;
    }
}