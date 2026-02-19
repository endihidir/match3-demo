using System.Collections.Generic;
using Core.Utils;
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
        private readonly IFXViewFactory _fxViewFactory;
        private readonly BoosterConfigContainerSO _boosterConfigContainer;

        public BoosterFxHandler(IFXViewFactory fxViewFactory, GridConfigContainerSO gridConfigContainer)
        {
            _fxViewFactory = fxViewFactory;
            _boosterConfigContainer = gridConfigContainer.GetConfig<BoosterConfigContainerSO>();
        }

        public UniTask PlayBoosterFxAsync(BoosterActionContext action, IGridModel model, IGridView view, out float animSpeed)
        {
            animSpeed = _boosterConfigContainer.GetAnimationSpeed(action);
            
            return action.BoosterAction switch
            {
                RocketHorizontalAction rha => PlayRocketFxAsync(rha, view, animSpeed, action.OriginCoord, model.GridSize),
                RocketVerticalAction rva => PlayRocketFxAsync(rva, view, animSpeed, action.OriginCoord, model.GridSize),
                BombAction bmb => PlayBombFxAsync(bmb, view, animSpeed, action.OriginCoord),
                _ => UniTask.CompletedTask
            };
        }

        private async UniTask PlayRocketFxAsync(RocketActionBase action, IGridView view, float animSpeed, Vector2Int originCoord, Vector2Int gridSize)
        {
            var offsets = BoosterTimelineBuilder.BuildLineOffsets(action.LineCount);
            var cellSize = view.GetCellSize();
            var isHorizontal = action is RocketHorizontalAction;

            List<UniTask> tasks = null;

            foreach (var offset in offsets)
            {
                var lineIndex = isHorizontal ? originCoord.y + offset : originCoord.x + offset;
                var maxIndex = isHorizontal ? gridSize.y : gridSize.x;
        
                if (lineIndex < 0 || lineIndex >= maxIndex) continue;

                var rocketOrigin = isHorizontal ? new Vector2Int(originCoord.x, lineIndex) : new Vector2Int(lineIndex, originCoord.y);

                var pos = view.GridToWorld(rocketOrigin);

                RocketFxView fx = isHorizontal ? _fxViewFactory.GetFX<HorizontalRocketFxView>() : _fxViewFactory.GetFX<VerticalRocketFxView>();

                fx.ApplyData(animSpeed);
                fx.UpdateRocketVisuals(cellSize);
                fx.UpdateTargetPositions(view.Cam, pos);

                tasks ??= new List<UniTask>(offsets.Length);
                tasks.Add(PlayAndReleaseAsync(fx, pos, view.FXParent));
            }

            if (tasks == null) return;

            await UniTask.WhenAll(tasks);
        }

        private async UniTask PlayBombFxAsync(BombAction bombAction, IGridView view, float animSpeed, Vector2Int originCoord)
        {
            var pos = view.GridToWorld(originCoord);
            var fx = _fxViewFactory.GetFX<BombFxView>();
            fx.ApplyData(bombAction.Radius, view.GetCellSize(), animSpeed);
            await PlayAndReleaseAsync(fx, pos, view.FXParent);
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