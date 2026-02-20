using Game.Grid.Contexts;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;
using Game.Grid.Strategies;

namespace Game.Grid.States
{
    public sealed class FillResolveState : StateBase<GridStateContext>
    {
        public override bool NeedsExitPermission => true;
        
        private readonly IFillStrategyResolver _strategyResolver;
        public bool IsInProgress { get; private set; }
        
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
            IsInProgress = true;
            
            var strategy = _strategyResolver.ResolveStrategy();
        
            if (strategy == null)
            {   
                EditorLogger.LogError("Fill strategy not found!");
                return;
            }
        
            await strategy.Execute().WaitAnimationsAsync();
            
            Context.MatchResolveRequested = GridMatchCalcUtil.HasAnyRegularMatchOnBoard(Context.GridModel);
            
            IsInProgress = false;
        }
    }
}