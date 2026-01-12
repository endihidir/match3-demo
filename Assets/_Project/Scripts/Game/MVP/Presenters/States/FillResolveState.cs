using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public sealed class FillResolveState : StateBase<GridStateContext>
    {
        public override bool NeedsExitPermission => true;
        
        private readonly IFillStrategyResolver _strategyResolver;
        
        public FillResolveState(GridStateContext context, IFillStrategyResolver strategyResolver) : base(context)
        {
            _strategyResolver = strategyResolver;
        }

        protected override void OnEnter()
        {
            RefillGrid().Forget();
            RequestExit();
        }

        private async UniTask RefillGrid()
        {
            var strategy = _strategyResolver.ResolveStrategy(Context.Model);
            
            if (strategy == null)
            {   
                EditorLogger.LogError("Fill strategy not found!");
                await UniTask.CompletedTask;
                return;
            }
            
            await strategy.Execute(Context).WaitAnimationsAsync();
            
            Context.MatchResolveRequested = GridMatchRules.HasAnyRegularMatchOnBoard(Context.Model);
        }
    }
}