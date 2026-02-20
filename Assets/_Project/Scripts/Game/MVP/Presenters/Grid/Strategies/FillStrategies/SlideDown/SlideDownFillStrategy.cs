using Cysharp.Threading.Tasks;
using Game.Grid.Handlers;
using Game.Grid.Strategies.Schedulers;
using Game.Grid.Utils;
using Game.Models;
using Game.Views;

namespace Game.Grid.Strategies
{
    public sealed partial class SlideDownFillStrategy : IFillStrategy
    {
        private readonly IGridModel _gridModel;
        private readonly IGridView _gridView;
        private readonly IGridObjectCreateHandler _objectCreateHandler;
        private readonly IFillItemDecider _itemDecider;
        private readonly IFallAnimationScheduler _fallAnimationScheduler;
        private readonly ISlideAnimationScheduler _slideAnimationScheduler;
        
        public SlideDownFillStrategy(IGridModel gridModel, IGridView gridView, IGridObjectCreateHandler objectCreateHandler, IFillItemDecider itemDecider, 
            IFallAnimationScheduler fallAnimationScheduler, ISlideAnimationScheduler slideAnimationScheduler)
        {
            _gridModel = gridModel;
            _gridView = gridView;
            _objectCreateHandler = objectCreateHandler;
            _itemDecider = itemDecider;
            _fallAnimationScheduler = fallAnimationScheduler;
            _slideAnimationScheduler = slideAnimationScheduler;
        }

        public bool CanHandle() => GridFillCalcUtil.HasStationaryAndBlocking(_gridModel);

        public IFillStrategy Execute()
        {
            ResetWorkspace();

            EnsureBuffers(_gridModel.Width, _gridModel.Height);

            var movedAny = true;
            
            while (movedAny)
            {
                movedAny = TryApplyAnyMove();
            }

            _runningAnimations = PlayAnimations();
            
            return this;
        }

        public UniTask WaitAnimationsAsync() => _runningAnimations;
    }
}