using System.Collections.Generic;
using Core.Models;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public sealed class RefillState : StateBase<GridStateContext>
    {
        public override bool NeedsExitTime => true;
        
        private readonly IRefillStrategy _fallStrategy = new FallDownRefillStrategy();
        private readonly IRefillStrategy _slideStrategy = new SlideDownRefillStrategy();

        protected override void OnEnter()
        {
            if (!Context.ResolvedAnyMatch)
            {
                RequestExit();
                return;
            }

            Context.CascadeInProgress = true;
            Run().Forget();
            RequestExit();
        }

        private async UniTask Run()
        {
            var tasks = new List<UniTask>();
            var strategy = SelectStrategy(Context.Model);
            await strategy.Execute(Context, tasks, 0.15f);
            await UniTask.WhenAll(tasks);
            FinishCascade();
        }

        private IRefillStrategy SelectStrategy(IGridModel model) => model.HasStationaryAndBlocking() ? _slideStrategy : _fallStrategy;
        private void FinishCascade()
        {
            Context.CascadeInProgress = false;
            var model = Context.Model;
            var grid = model.BuildTypeDataGrid();
            Context.CascadeResolveRequested = GridMatchDetectUtil.HasAnyRegularMatchOnBoard(grid, model.Width, model.Height);
        }
    }
}