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
        private readonly IGridObjectSpawnHandler _gridObjectSpawnHandler;
        private readonly IFillItemDecider _itemDecider;
        private readonly IFallAnimationScheduler _fallAnimationScheduler;

        public FallDownFillStrategy(IGridModel gridModel, IGridView gridView, IGridObjectSpawnHandler gridObjectSpawnHandler, IFillItemDecider itemDecider, 
            IFallAnimationScheduler fallAnimationScheduler)
        {
            _gridModel = gridModel;
            _gridView = gridView;
            _gridObjectSpawnHandler = gridObjectSpawnHandler;
            _itemDecider = itemDecider;
            _fallAnimationScheduler = fallAnimationScheduler;
        }
        
        public bool CanHandle(IGridModel model) => !GridFillCalcUtil.HasStationaryAndBlocking(model);

        public IFillStrategy Execute()
        {
            _recordCount = 0;

            var width = _gridModel.Width;
            var height = _gridModel.Height;
            var cellSize = _gridView.GetCellSize();

            EnsureBuffers(width);
            EnsureRecordCapacity(width * height);

            for (int x = 0; x < width; x++)
            {
                ShiftColumnLogic(x, height);

                if (GridFillCalcUtil.TryGetSpawnCellCoord(_gridModel, x, out var spawnCell))
                {
                    var spawnY = _gridView.GridToWorld(spawnCell).y + cellSize;
                    FillColumnLogic(x, height, cellSize, spawnY);
                }
            }

            _runningAnimations = PlayAnimations();
            return this;
        }

        public UniTask WaitAnimationsAsync() => _runningAnimations;
    }
}