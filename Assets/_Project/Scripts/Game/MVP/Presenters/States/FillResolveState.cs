using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public sealed class FillResolveState : StateBase<GridStateContext>
    {
        public override bool NeedsExitPermission => true;
        
        private readonly IFillStrategyResolver _strategyResolver;
        public bool IsInProgress { get; private set; }
        
        private int _fillRunId;
        
        public FillResolveState(GridStateContext context, IFillStrategyResolver strategyResolver) : base(context)
        {
            _strategyResolver = strategyResolver;
        }

        protected override void OnEnter()
        {
            FillGridAsync().Forget();
            RequestExit();
        }

        private async UniTask FillGridAsync()
        {
            var runId = ++_fillRunId;
            
            IsInProgress = true;

            try
            {
                await UniTask.Yield();

                if (runId != _fillRunId) return;
            
                var strategy = _strategyResolver.ResolveStrategy(Context.Model);
            
                if (strategy == null)
                {   
                    EditorLogger.LogError("Fill strategy not found!");
                    return;
                }
            
                await strategy.Execute(Context).WaitAnimationsAsync();
            
                if (runId != _fillRunId) return;
                
                Context.MatchResolveRequested = GridMatchCalcUtil.HasAnyRegularMatchOnBoard(Context.Model);
            }
            finally
            {
                if (runId == _fillRunId)
                    IsInProgress = false;
            }
        }
    }
}