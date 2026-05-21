using Game.Grid.Contexts;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;
using Game.Grid.Strategies;
using UnityEngine;

namespace Game.Grid.States
{
    public sealed class FillResolveState : StateBase<GridStateContext>
    {
        public override bool NeedsExitPermission => true;
        
        private readonly IFillStrategyResolver _strategyResolver;
        public bool IsInProgress { get; private set; }
        
        public FillResolveState(GridStateContext context, IFillStrategyResolver strategyResolver) : base(context)
        {
            _strategyResolver = strategyResolver;
        }

        protected override void OnEnter()
        {
            FillGridAsync().Forget();
            
            RequestExit();
        }

        private async UniTask FillGridAsync()
        {
            var strategy = _strategyResolver.ResolveStrategy();
        
            if (strategy == null)
            {   
                EditorLogger.LogError("Fill strategy not found!");
                return;
            }
            
            IsInProgress = true;
        
            await strategy.Execute().WaitAnimationsAsync();
            
            await UniTask.WaitUntil(() => !IsAnyTileFalling());
            
            Context.MatchResolveRequested = GridMatchCalcUtil.HasAnyRegularMatchOnBoard(Context.GridModel);
            
            IsInProgress = false;
        }
        
        private bool IsAnyTileFalling()
        {
            var model = Context.GridModel;
            for (int y = 0; y < model.Height; y++)
            {
                for (int x = 0; x < model.Width; x++)
                {
                    var obj = model.GetGridObject(new Vector2Int(x, y));
                    if (obj && obj.IsFallInProgress) return true;
                }
            }

            return false;
        }
    }
}