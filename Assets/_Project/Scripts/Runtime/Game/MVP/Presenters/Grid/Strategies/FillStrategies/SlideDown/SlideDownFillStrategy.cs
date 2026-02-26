using Cysharp.Threading.Tasks;
using Game.Grid.Handlers;
using Game.Grid.Strategies.Schedulers;
using Game.Grid.Utils;
using Game.Models;
using Game.Views;

namespace Game.Grid.Strategies
{
    /// <summary>
    /// Fill strategy that simulates gravity with diagonal-slide fallback and
    /// top-of-column spawning. Designed to be called any number of times in
    /// sequence without state bleed between runs.
    ///
    /// Execution flow per call:
    /// 1. PrepareForRun  — reset all per-run counters, ensure buffers
    /// 2. Simulate — repeatedly apply vertical falls → diagonal slides → spawns
    /// 3. Schedule — convert recorded paths into DOTween tasks ordered by column timeline
    /// 4. WaitAsync — caller awaits the returned UniTask
    /// </summary>
    public sealed partial class SlideDownFillStrategy : IFillStrategy
    {
        private readonly IGridModel _gridModel;
        private readonly IGridView _gridView;
        private readonly IGridObjectCreateHandler _objectCreateHandler;
        private readonly IFillItemDecider _itemDecider;
        private readonly IFallAnimationScheduler _fallAnimationScheduler;
        private readonly ISlideAnimationScheduler _slideAnimationScheduler;

        public SlideDownFillStrategy(IGridModel gridModel, IGridView gridView, IGridObjectCreateHandler objectCreateHandler, IFillItemDecider itemDecider,
            IFallAnimationScheduler  fallAnimationScheduler, ISlideAnimationScheduler slideAnimationScheduler)
        {
            _gridModel = gridModel;
            _gridView = gridView;
            _objectCreateHandler = objectCreateHandler;
            _itemDecider = itemDecider;
            _fallAnimationScheduler = fallAnimationScheduler;
            _slideAnimationScheduler = slideAnimationScheduler;
        }

        public bool CanHandle() => GridFillCalcUtil.HasStationaryWithMovableSpaceBelow(_gridModel);

        public IFillStrategy Execute()
        {
            PrepareForRun(_gridModel.Width, _gridModel.Height);
            RunSimulation();
            _pendingAnimations = ScheduleAnimations();
            return this;
        }

        public UniTask WaitAnimationsAsync() => _pendingAnimations;
    }
}