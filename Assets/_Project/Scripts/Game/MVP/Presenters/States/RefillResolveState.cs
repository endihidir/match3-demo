using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public sealed class RefillResolveState : StateBase<GridStateContext>
    {
        public override bool NeedsExitPermission => true;
        
        private readonly IRefillStrategyHandler _strategyHandler;
        
        public RefillResolveState(IRefillStrategyHandler strategyHandler, GridStateContext context, bool showLogs = true) : base(context, showLogs)
        {
            _strategyHandler = strategyHandler;
        }

        protected override void OnEnter()
        {
            Context.RefillInProgress = true;
            Run().Forget();
            RequestExit();
        }

        private async UniTask Run()
        {
            var strategy = _strategyHandler.SelectStrategy(Context.Model);
            await strategy.Execute(Context);
            Context.RefillInProgress = false;
            Context.RefillResolveRequested = GridMatchRules.HasAnyRegularMatchOnBoard(Context.Model);
        }
    }
}