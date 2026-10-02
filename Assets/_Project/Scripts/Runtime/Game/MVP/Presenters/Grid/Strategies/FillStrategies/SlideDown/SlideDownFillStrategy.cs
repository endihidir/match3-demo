using Cysharp.Threading.Tasks;
using Game.Grid.Handlers;
using Game.Grid.Utils;
using Game.Models;
using Game.Views;

namespace Game.Grid.Strategies
{
    /// <summary>
    /// Fill strategy that simulates gravity with diagonal-slide fallback and
    /// top-of-column spawning. Designed to be called any number of times in
    /// sequence without state bleed between runs.
    /// </summary>
    public sealed partial class SlideDownFillStrategy : IFillStrategy
    {
        private readonly IGridModel _gridModel;
        private readonly IGridView _gridView;
        private readonly IGridObjectCreateHandler _objectCreateHandler;
        private readonly IFillItemDecider _itemDecider;
        private readonly IFillMotionPlanner _motionPlanner;

        private UniTask _pendingAnimations = UniTask.CompletedTask;

        public SlideDownFillStrategy(IGridModel gridModel, IGridView gridView, IGridObjectCreateHandler objectCreateHandler, IFillItemDecider itemDecider,
            IFillMotionPlanner motionPlanner)
        {
            _gridModel = gridModel;
            _gridView = gridView;
            _objectCreateHandler = objectCreateHandler;
            _itemDecider = itemDecider;
            _motionPlanner = motionPlanner;
        }

        public bool CanHandle() => GridFillCalcUtil.HasStationaryWithMovableSpaceBelow(_gridModel);

        public IFillStrategy Execute()
        {
            PrepareForRun(_gridModel.Width);
            _motionPlanner.Begin();
            RunSimulation();
            _pendingAnimations = _motionPlanner.Commit();
            return this;
        }

        public UniTask WaitAnimationsAsync() => _pendingAnimations;
    }
}