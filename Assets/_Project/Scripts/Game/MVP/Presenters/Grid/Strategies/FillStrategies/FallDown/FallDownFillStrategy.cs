using Cysharp.Threading.Tasks;
using Game.Grid.Contexts;
using Game.Grid.Strategies.Schedulers;
using Game.Grid.Utils;
using Game.Models;

namespace Game.Grid.Strategies
{
    public sealed partial class FallDownFillStrategy : IFillStrategy
    {
        private readonly IFillItemDecider _itemDecider;
        private readonly IFallAnimationScheduler _fallAnimationScheduler;
        public bool CanHandle(IGridModel model) => !GridFillCalcUtil.HasStationaryAndBlocking(model);

        public FallDownFillStrategy(IFillItemDecider itemDecider, IFallAnimationScheduler fallAnimationScheduler)
        {
            _itemDecider = itemDecider;
            _fallAnimationScheduler = fallAnimationScheduler;
        }

        public IFillStrategy Execute(GridStateContext context)
        {
            _recordCount = 0;

            var model = context.GridModel;
            var view = context.GridView;

            var width = model.Width;
            var height = model.Height;
            var cellSize = view.GetCellSize();

            EnsureBuffers(width);
            EnsureRecordCapacity(width * height);

            for (int x = 0; x < width; x++)
            {
                ShiftColumnLogic(model, x, height);

                if (GridFillCalcUtil.TryGetSpawnCellCoord(model, x, model.Height, out var spawnCell))
                {
                    var spawnY = view.GridToWorld(spawnCell).y + cellSize;
                    FillColumnLogic(context, x, height, cellSize, spawnY);
                }
            }

            _runningAnimations = PlayAnimations(context);
            return this;
        }

        public UniTask WaitAnimationsAsync() => _runningAnimations;
    }
}