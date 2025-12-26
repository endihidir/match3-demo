using System.Collections.Generic;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public sealed class RefillState : StateBase<GridStateContext>
    {
        public override bool NeedsExitTime => true;
        
        private readonly IRefillStrategyHandler _refillStrategyHandler;

        public RefillState(IRefillStrategyHandler refillStrategyHandler)
        {
            _refillStrategyHandler = refillStrategyHandler;
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
            var strategy = _refillStrategyHandler.SelectStrategy(Context.Model);
            await strategy.Execute(Context, tasks, 0.15f);
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