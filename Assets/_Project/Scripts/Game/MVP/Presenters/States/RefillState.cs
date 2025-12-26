using System.Collections.Generic;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public sealed class RefillState : StateBase<GridStateContext>
    {
        public override bool NeedsExitTime => true;
        
        private readonly IRefillStrategyHandler _strategyHandler;
        private float _refillStartDelay;

        public RefillState(IRefillStrategyHandler strategyHandler) => _strategyHandler = strategyHandler;

        protected override void OnInit()
        {
            _refillStartDelay = Context.Configs.RefillSettings.RefillStartDelay;
            _strategyHandler.Initialize(Context.Configs.RefillSettings);
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
            var strategy = _strategyHandler.SelectStrategy(Context.Model);
            await strategy.Execute(Context, tasks, _refillStartDelay);
            await UniTask.WhenAll(tasks);
            FinishCascade();
        }
        
        private void FinishCascade()
        {
            Context.RefillInProgress = false;
            var model = Context.Model;
            var gridObjectTypes = model.BuildTypeDataGrid();
            Context.RefillResolveRequested = GridMatchDetectUtil.HasAnyRegularMatchOnBoard(gridObjectTypes, model.Width, model.Height);
        }
    }
}