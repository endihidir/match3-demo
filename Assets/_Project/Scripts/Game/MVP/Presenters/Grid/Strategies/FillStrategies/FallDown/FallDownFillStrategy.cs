using Core.Models;
using Core.Utils;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public sealed partial class FallDownFillStrategy : IFillStrategy
    {
        private readonly IFillItemDecider _itemDecider;
        private readonly IFallAnimationScheduler _fallAnimationScheduler;

        public FallDownFillStrategy(IFillItemDecider itemDecider, IFallAnimationScheduler fallAnimationScheduler)
        {
            _itemDecider = itemDecider;
            _fallAnimationScheduler = fallAnimationScheduler;
        }

        public bool CanHandle(IGridModel model) => !GridFillCalcUtil.HasStationaryAndBlocking(model);

        public IFillStrategy Execute(GridStateContext context)
        {
            ResetWorkspace();

            var model = context.GridModel;
            var view = context.GridView;
            var width = model.Width;
            var height = model.Height;
            var cellSize = view.GetCellSize();

            EnsureCapacity(width, height);

            // Process each column
            for (int x = 0; x < width; x++)
            {
                // First: shift existing items down
                ShiftColumn(model, x, height);

                // Then: spawn new items at top
                if (GridFillCalcUtil.TryGetSpawnCellCoord(model, x, height, out var spawnCell))
                {
                    var spawnY = view.GridToWorld(spawnCell).y + cellSize;
                    FillColumn(context, x, height, cellSize, spawnY);
                }
            }

            _runningAnimations = PlayAnimations(context);
            return this;
        }

        public UniTask WaitAnimationsAsync() => _runningAnimations;
    }
}