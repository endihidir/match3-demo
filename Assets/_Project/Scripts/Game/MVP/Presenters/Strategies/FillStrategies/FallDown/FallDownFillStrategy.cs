using Core.Models;
using Core.Utils;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public sealed partial class FallDownFillStrategy : IFillStrategy
    {
        private readonly IFillItemDecider _itemDecider;
        private readonly IShiftAnimationScheduler _shiftAnimationScheduler;
        public bool CanHandle(IGridModel model) => !GridFillCalcUtil.HasStationaryAndBlocking(model);

        public FallDownFillStrategy(IFillItemDecider itemDecider, IShiftAnimationScheduler shiftAnimationScheduler)
        {
            _itemDecider = itemDecider;
            _shiftAnimationScheduler = shiftAnimationScheduler;
        }

        public IFillStrategy Execute(GridStateContext context)
        {
            _recordCount = 0;

            var model = context.Model;
            var view = context.View;

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