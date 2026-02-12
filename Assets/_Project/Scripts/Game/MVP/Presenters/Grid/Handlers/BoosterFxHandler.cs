using Core.Configs;
using Core.Item.Factories;
using Core.Models;
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
    
        public BoosterFxHandler(IBoosterFxFactory boosterFxFactory) => _boosterFxFactory = boosterFxFactory;

        public void PlayBoosterFx(BoosterActionBase boosterAction, Vector2Int originCoord, IGridModel model, IGridView view)
        {
            switch (boosterAction)
            {
                case RocketHorizontalAction rocketHorizontalAction:
                    PlayHorizontalRocketsFx(originCoord, rocketHorizontalAction.LineCount, model, view).Forget();
                    break;
                case RocketVerticalAction rocketVerticalAction:
                    PlayVerticalRocketsFx(originCoord, rocketVerticalAction.LineCount, model, view).Forget();
                    break;
                case BombAction bombAction:
                    /*var pos = view.GridToWorld(originCoord);
                    PlayBombFx(pos, bombAction.Radius, view.FXParent);*/
                    break;
            }
        }

        private async UniTask PlayHorizontalRocketsFx(Vector2Int originCoord, int lineCount, IGridModel model, IGridView view)
        {
            var offsets = BoosterImpactResolver.BuildLineOffsets(lineCount);
            
            foreach (var offset in offsets)
            {
                var y = originCoord.y + offset;
                if (y < 0 || y >= model.Height) continue;
                
                var coord = new Vector2Int(originCoord.x, y);
                var pos = view.GridToWorld(coord);
                var fx = _boosterFxFactory.GetRocketFx<HorizontalRocketFxView>(pos, view.GetCellSize());
                await PlayBoosterAt(fx, pos, view.FXParent);
            }
        }

        private async UniTask PlayVerticalRocketsFx(Vector2Int originCoord, int lineCount, IGridModel model, IGridView view)
        {
            var offsets = BoosterImpactResolver.BuildLineOffsets(lineCount);
            
            foreach (var offset in offsets)
            {
                var x = originCoord.x + offset;
                if (x < 0 || x >= model.Width) continue;
                
                var coord = new Vector2Int(x, originCoord.y);
                var pos = view.GridToWorld(coord);
                var fx = _boosterFxFactory.GetRocketFx<VerticalRocketFxView>(pos, view.GetCellSize());
                await PlayBoosterAt(fx, pos, view.FXParent);
            }
        }

        private async UniTask PlayBombFx(Vector3 pos, int radius, Transform parent)
        {
            var fx = _boosterFxFactory.GetBombFx(pos, radius);
            await PlayBoosterAt(fx, pos, parent);
        }

        private async UniTask PlayBoosterAt(BoosterFxView fx, Vector3 pos, Transform parent)
        {
            fx.transform.SetParent(parent, false);
            fx.transform.position = pos;
            await fx.Play(() => OnBoosterFxComplete(fx));
        }

        private void OnBoosterFxComplete(BoosterFxView fx) => _boosterFxFactory.ReleaseBooster(fx);
    }
}