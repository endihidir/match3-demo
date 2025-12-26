using System.Collections.Generic;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public sealed class RefillState : StateBase<GridStateContext>
    {
        public override bool NeedsExitTime => true;
        
        private readonly IRefillStrategySelectionHandler _refillStrategySelectionHandler;

        public RefillState(IRefillStrategySelectionHandler refillStrategySelectionHandler)
        {
            _refillStrategySelectionHandler = refillStrategySelectionHandler;
        }

        protected override void OnEnter()
        {
            if (!Context.ResolvedAnyMatch)
            {
                RequestExit();
                return;
            }

            Context.RefillInProgress = true;
            Run().Forget();
            RequestExit();
        }

        private async UniTask Run()
        {
            var tasks = new List<UniTask>();
            var strategy = _refillStrategySelectionHandler.SelectStrategy(Context.Model);
            await strategy.Execute(Context, tasks, 0.05f);
            await UniTask.WhenAll(tasks);
            FinishCascade();
        }
        
        private void FinishCascade()
        {
            Context.RefillInProgress = false;
            var model = Context.Model;
            var grid = model.BuildTypeDataGrid();
            Context.RefillResolveRequested = GridMatchDetectUtil.HasAnyRegularMatchOnBoard(grid, model.Width, model.Height);
        }
    }
}