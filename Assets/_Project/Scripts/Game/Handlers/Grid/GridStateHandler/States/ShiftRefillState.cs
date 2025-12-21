using System.Collections.Generic;
using Core.Models;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public sealed class ShiftRefillState : StateBase<GridContext>
    {
        public override bool NeedsExitTime => true;
        
        private readonly IGridFillStrategy _fall = new FallDownFillStrategy();
        private readonly IGridFillStrategy _slide = new SlideDownFillStrategy();

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
            await UniTask.WaitForSeconds(0.02f);
            var tasks = new List<UniTask>();
            var strategy = SelectStrategy(Context.Model);
            await strategy.Execute(Context, tasks);
            await UniTask.WhenAll(tasks);
            FinishCascade();
        }

        private IGridFillStrategy SelectStrategy(IGridModel model) => model.HasStationaryObject() ? _slide : _fall;
        private void FinishCascade()
        {
            Context.CascadeInProgress = false;
            var model = Context.Model;
            var grid = model.BuildTypeDataGrid();
            Context.CascadeResolveRequested = GridMatchDetectUtil.HasAnyRegularMatchOnBoard(grid, model.Width, model.Height);
        }
    }
}