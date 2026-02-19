using Cysharp.Threading.Tasks;
using Game.Grid.Contexts;
using Game.Grid.Strategies.Schedulers;
using Game.Grid.Utils;
using Game.Models;

namespace Game.Grid.Strategies
{
    public sealed partial class SlideDownFillStrategy : IFillStrategy
    {
        private readonly IFillItemDecider _itemDecider;
        private readonly IFallAnimationScheduler _fallAnimationScheduler;
        private readonly ISlideAnimationScheduler _slideAnimationScheduler;
        
        public SlideDownFillStrategy(IFillItemDecider itemDecider, IFallAnimationScheduler fallAnimationScheduler, ISlideAnimationScheduler slideAnimationScheduler)
        {
            _itemDecider = itemDecider;
            _fallAnimationScheduler = fallAnimationScheduler;
            _slideAnimationScheduler = slideAnimationScheduler;
        }

        public bool CanHandle(IGridModel model) => GridFillCalcUtil.HasStationaryAndBlocking(model);

        public IFillStrategy Execute(GridStateContext context)
        {
            ResetWorkspace();

            var model = context.GridModel;

            EnsureBuffers(model.Width, model.Height);

            var movedAny = true;

            // Keep looping while we can apply any movement (vertical fall, diagonal slide, spawn)
            while (movedAny)
            {
                movedAny = TryApplyAnyMove(context);
            }

            _runningAnimations = PlayAnimations(context);
            
            return this;
        }

        public UniTask WaitAnimationsAsync() => _runningAnimations;
    }
}