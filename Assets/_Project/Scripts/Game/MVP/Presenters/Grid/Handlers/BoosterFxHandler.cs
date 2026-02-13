using System;
using System.Collections.Generic;
using Core.Configs;
using Core.Item.Factories;
using Core.UI;
using Core.Utils;
using Core.Views;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Handlers
{
    public class BoosterFxHandler : IBoosterFxHandler
    {
        private readonly IBoosterFxFactory _boosterFxFactory;
        private readonly BoosterConfigContainerSO _boosterConfigContainer;

        public BoosterFxHandler(IBoosterFxFactory boosterFxFactory, GameplayConfigContainer gameplayConfigContainer)
        {
            _boosterFxFactory = boosterFxFactory;
            _boosterConfigContainer = gameplayConfigContainer.GridConfigContainer.GetConfig<BoosterConfigContainerSO>();
        }

        public float PlayBoosterFx(BoosterActionContext action, IGridView view, Action onComplete)
        {
            var animSpeed = _boosterConfigContainer.GetAnimationSpeed(action);

            if (animSpeed == 0)
            {
                onComplete?.Invoke();
                return float.MaxValue;
            }

            switch (action.BoosterAction)
            {
                case RocketHorizontalAction rha:
                    PlayHorizontalRocketFxAsync(animSpeed, action.OriginCoord, rha.LineCount, view).ContinueWith(() => onComplete?.Invoke()).Forget();
                    break;
                case RocketVerticalAction rva:
                    PlayVerticalRocketFxAsync(animSpeed, action.OriginCoord, rva.LineCount, view).ContinueWith(() => onComplete?.Invoke()).Forget();
                    break;
                case BombAction bmb:
                    PlayBombFxAsync(bmb.Radius, action.OriginCoord, view).ContinueWith(() => onComplete?.Invoke()).Forget();
                    break;
            }
            
            return animSpeed;
        }

        private async UniTask PlayHorizontalRocketFxAsync(float animSpeed, Vector2Int originCoord, int lineCount, IGridView view)
        {
            var offsets = BoosterImpactResolver.BuildLineOffsets(lineCount);
            var cellSize = view.GetCellSize();
            var gridSize = view.GridSize;

            List<UniTask> tasks = null;

            foreach (var offset in offsets)
            {
                var lineIndex = originCoord.y + offset;
                if (lineIndex < 0 || lineIndex >= gridSize.y) continue;

                var rocketOrigin = new Vector2Int(originCoord.x, lineIndex);
                var pos = view.GridToWorld(rocketOrigin);

                var fx = _boosterFxFactory.GetRocketFx<HorizontalRocketFxView>(pos, cellSize, animSpeed);

                tasks ??= new List<UniTask>(offsets.Length);
                tasks.Add(PlayAndReleaseAsync(fx, pos, view.FXParent));
            }

            if (tasks == null) return;

            await UniTask.WhenAll(tasks);
        }

        private async UniTask PlayVerticalRocketFxAsync(float animSpeed, Vector2Int originCoord, int lineCount, IGridView view)
        {
            var offsets = BoosterImpactResolver.BuildLineOffsets(lineCount);
            var cellSize = view.GetCellSize();
            var gridSize = view.GridSize;

            List<UniTask> tasks = null;

            foreach (var offset in offsets)
            {
                var lineIndex = originCoord.x + offset;
                if (lineIndex < 0 || lineIndex >= gridSize.x) continue;

                var rocketOrigin = new Vector2Int(lineIndex, originCoord.y);
                var pos = view.GridToWorld(rocketOrigin);

                var fx = _boosterFxFactory.GetRocketFx<VerticalRocketFxView>(pos, cellSize, animSpeed);
                tasks ??= new List<UniTask>(offsets.Length);
                tasks.Add(PlayAndReleaseAsync(fx, pos, view.FXParent));
            }

            if (tasks == null) return;

            await UniTask.WhenAll(tasks);
        }

        private async UniTask PlayBombFxAsync(int radius, Vector2Int originCoord, IGridView view)
        {
            var pos = view.GridToWorld(originCoord);
            var fx = _boosterFxFactory.GetBombFx(pos, radius);
            await PlayAndReleaseAsync(fx, pos, view.FXParent);
        }

        private async UniTask PlayAndReleaseAsync(BoosterFxView fx, Vector3 pos, Transform parent)
        {
            fx.transform.SetParent(parent, false);
            fx.transform.position = pos;
            await fx.Play();
            _boosterFxFactory.ReleaseBooster(fx);
        }
    }
}
