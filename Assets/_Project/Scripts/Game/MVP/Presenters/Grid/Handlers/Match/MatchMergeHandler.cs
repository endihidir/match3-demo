using Cysharp.Threading.Tasks;
using Game.Grid.Item;
using Game.Models;
using Game.Views;
using UnityEngine;

namespace Game.Grid.Handlers
{
    public sealed class MatchMergeHandler : IMatchMergeHandler
    {
        private readonly IGridModel _gridModel;
        private readonly IGridView _gridView;
        private readonly IGridObjectCreateHandler _createHandler;

        public MatchMergeHandler(IGridModel gridModel, IGridView gridView, IGridObjectCreateHandler createHandler)
        {
            _gridModel = gridModel;
            _gridView = gridView;
            _createHandler = createHandler;
        }

        public async UniTask PlayMergeAnimationAsync(BaseGridObject[] mergeObjs, Vector2Int centerCoord)
        {
            var targetWorld = _gridView.GridToWorld(centerCoord);
            var tasks = new UniTask[mergeObjs.Length];

            for (int i = 0; i < mergeObjs.Length; i++)
            {
                var obj = mergeObjs[i];

                if (!obj)
                {
                    tasks[i] = UniTask.CompletedTask;
                    continue;
                }

                var tween = obj.Animation.MoveTo(targetWorld);
                tasks[i] = tween?.ToUniTask() ?? UniTask.CompletedTask;
            }

            await UniTask.WhenAll(tasks);
        }

        public void SpawnBooster(Vector2Int pos, BoosterType boosterType)
        {
            var booster = _createHandler.CreateBooster(boosterType);
            _gridModel.SetGridObject(pos, booster);
            booster.SetPosition(_gridView.GridToWorld(pos));
            booster.SetSpriteSize(_gridView.GetCellSize());
            booster.SetParent(_gridView.GridObjectsParent);
        }
    }
}
