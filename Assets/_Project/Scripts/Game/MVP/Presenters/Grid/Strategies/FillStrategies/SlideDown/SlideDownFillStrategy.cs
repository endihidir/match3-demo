using Core.Models;
using Core.Utils;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public sealed partial class SlideDownFillStrategy : IFillStrategy
    {
        private readonly IFillItemDecider _itemDecider;
        private readonly IShiftAnimationScheduler _shiftAnimationScheduler;
        private readonly ISlideAnimationScheduler _slideAnimationScheduler;
        
        public SlideDownFillStrategy(IFillItemDecider itemDecider, IShiftAnimationScheduler shiftAnimationScheduler, ISlideAnimationScheduler slideAnimationScheduler)
        {
            _itemDecider = itemDecider;
            _shiftAnimationScheduler = shiftAnimationScheduler;
            _slideAnimationScheduler = slideAnimationScheduler;
        }

        public bool CanHandle(IGridModel model) => GridFillCalcUtil.HasStationaryAndBlocking(model);

        public IFillStrategy Execute(GridStateContext context)
        {
            ResetWorkspace();

            var model = context.Model;

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