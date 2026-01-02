using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public sealed class RefillResolveState : StateBase<GridStateContext>
    {
        public override bool NeedsExitPermission => true;
        
        private readonly IRefillStrategyHandler _strategyHandler;
        
        public RefillResolveState(IRefillStrategyHandler strategyHandler, GridStateContext context) : base(context)
        {
            _strategyHandler = strategyHandler;
        }

        protected override void OnEnter()
        {
            RefillGrid().Forget();
            RequestExit();
        }

        private async UniTaskVoid RefillGrid()
        {
            Context.RefillInProgress = true;
            var strategy = _strategyHandler.SelectStrategy(Context.Model);
            await strategy.Execute(Context).WaitAnimationsAsync();
            Context.RefillInProgress = false;
            Context.MatchResolveRequested = GridMatchRules.HasAnyRegularMatchOnBoard(Context.Model);
            //RequestExit();
        }
    }
}