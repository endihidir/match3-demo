using System.Collections.Generic;
using Game.Configs;
using Game.View.Factories;
using Cysharp.Threading.Tasks;
using Game.Grid.Contexts;
using Game.Grid.Utils;
using Game.Models;
using Game.Views;
using UnityEngine;

namespace Game.Grid.Handlers
{
    public sealed class BoosterFxHandler : IBoosterFxHandler
    {
        private readonly IGridModel _gridModel;
        private readonly IGridView _gridView;
        private readonly IFXViewFactory _fxViewFactory;
        private readonly BoosterConfigContainerSO _boosterConfigContainer;

        public BoosterFxHandler(IGridModel gridModel, IGridView gridView, IFXViewFactory fxViewFactory, GridConfigContainerSO gridConfigContainer)
        {
            _gridModel = gridModel;
            _gridView = gridView;
            _fxViewFactory = fxViewFactory;
            _boosterConfigContainer = gridConfigContainer.GetConfig<BoosterConfigContainerSO>();
        }

        public UniTask PlayBoosterFxAsync(BoosterActionContext action, out float animSpeed)
        {
            animSpeed = _boosterConfigContainer.GetAnimationSpeed(action);

            return action.BoosterAction switch
            {
                RocketHorizontalAction rha => PlayRocketFxAsync(rha, animSpeed, action.OriginCoord, action.TriggerDelay),
                RocketVerticalAction rva => PlayRocketFxAsync(rva, animSpeed, action.OriginCoord, action.TriggerDelay),
                BombAction bmb => PlayBombFxAsync(bmb, animSpeed, action.OriginCoord, action.TriggerDelay),
                _ => UniTask.CompletedTask
            };
        }

        private async UniTask PlayRocketFxAsync(RocketActionBase action, float animSpeed, Vector2Int originCoord, float triggerDelay)
        {
            if (triggerDelay > 0f)
                await UniTask.WaitForSeconds(triggerDelay);

            var offsets = BoosterTimelineBuilder.BuildLineOffsets(action.LineCount);

            var cellSize = _gridView.GetCellSize();
            var gridSize = _gridModel.GridSize;
            var isHorizontal = action is RocketHorizontalAction;

            List<UniTask> tasks = null;

            foreach (var offset in offsets)
            {
                var lineIndex = isHorizontal ? originCoord.y + offset : originCoord.x + offset;
                var maxIndex = isHorizontal ? gridSize.y : gridSize.x;

                if (lineIndex < 0 || lineIndex >= maxIndex) continue;

                var rocketOrigin = isHorizontal ? new Vector2Int(originCoord.x, lineIndex) : new Vector2Int(lineIndex, originCoord.y);
                var pos = _gridView.GridToWorld(rocketOrigin);

                RocketFxView fx = isHorizontal ? _fxViewFactory.GetFX<HorizontalRocketFxView>() : _fxViewFactory.GetFX<VerticalRocketFxView>();

                fx.ApplyData(animSpeed);
                fx.UpdateRocketVisuals(cellSize);
                fx.UpdateTargetPositions(_gridView.Cam, pos);

                tasks ??= new List<UniTask>(offsets.Length);
                tasks.Add(PlayAndReleaseAsync(fx, pos, _gridView.FXParent));
            }

            if (tasks == null) return;

            await UniTask.WhenAll(tasks);
        }

        private async UniTask PlayBombFxAsync(BombAction bombAction, float animSpeed, Vector2Int originCoord, float triggerDelay)
        {
            if (triggerDelay > 0f)
                await UniTask.WaitForSeconds(triggerDelay);

            var pos = _gridView.GridToWorld(originCoord);
            var fx = _fxViewFactory.GetFX<BombFxView>();
            fx.ApplyData(bombAction.Radius, _gridView.GetCellSize(), animSpeed);
            await PlayAndReleaseAsync(fx, pos, _gridView.FXParent);
        }

        private async UniTask PlayAndReleaseAsync(BoosterFxView fx, Vector3 pos, Transform parent)
        {
            fx.transform.SetParent(parent, false);
            fx.transform.position = pos;
            await fx.PlayAsync();
            _fxViewFactory.ReleaseFX(fx);
        }
    }
}