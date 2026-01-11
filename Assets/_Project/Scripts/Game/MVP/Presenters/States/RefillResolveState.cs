using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public sealed class RefillResolveState : StateBase<GridStateContext>
    {
        public override bool NeedsExitPermission => true;
        
        private readonly IFillStrategyHandler _strategyHandler;
        
        public RefillResolveState(IFillStrategyHandler strategyHandler, GridStateContext context) : base(context)
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
            var strategy = _strategyHandler.SelectStrategy(Context.Model);
            await strategy.Execute(Context).WaitAnimationsAsync();
            Context.MatchResolveRequested = GridMatchRules.HasAnyRegularMatchOnBoard(Context.Model);
        }
    }
}