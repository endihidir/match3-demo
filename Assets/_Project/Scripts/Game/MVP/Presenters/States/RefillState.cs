using System.Collections.Generic;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public sealed class RefillState : StateBase<GridStateContext>
    {
        public override bool NeedsExitPermission => true;
        
        private readonly IRefillStrategyHandler _strategyHandler;

        public RefillState(IRefillStrategyHandler strategyHandler) => _strategyHandler = strategyHandler;

        protected override void OnInit()
        {
            _strategyHandler.Initialize(Context.Configs.RefillSettingsSo);
        }

        protected override void OnEnter()
        {
            /*if (!Context.ResolvedAnyMatch && !Context.ResolvedAnyEffect)
            {
                RequestExit();
                return;
            }*/

            Context.RefillInProgress = true;
            Run().Forget();
            RequestExit();
        }

        private async UniTask Run()
        {
            var tasks = new List<UniTask>();
            var strategy = _strategyHandler.SelectStrategy(Context.Model);
            await strategy.Execute(Context, tasks);
            await UniTask.WhenAll(tasks);
            FinishCascade();
        }
        
        private void FinishCascade()
        {
            Context.RefillInProgress = false;
            Context.RefillResolveRequested = GridMatchRules.HasAnyRegularMatchOnBoard(Context.Model);
        }
    }
}