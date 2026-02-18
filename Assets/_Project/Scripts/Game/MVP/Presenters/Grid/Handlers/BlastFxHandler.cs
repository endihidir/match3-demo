using Game.Configs;
using Game.Grid.Item;
using Game.Views;
using Game.View.Factories;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Grid.Handlers
{
    public sealed class BlastFxHandler : IBlastFxHandler
    {
        private readonly IFXViewFactory _fxViewFactory;
        private readonly GridConfigContainerSO _gridConfigContainer;
        
        public BlastFxHandler(IFXViewFactory fxViewFactory, GridConfigContainerSO gridConfigContainer)
        {
            _fxViewFactory = fxViewFactory;
            _gridConfigContainer = gridConfigContainer;
        }

        public void PlayBlastParticle(BaseGridObject obj, Vector3 pos, Transform parent)
        {
            switch (obj)
            {
                case ItemObject item:
                    if (TryGetItemBlastFx(item, out var itemBlastFx)) return;
                    PlayBlastFxAsync(itemBlastFx, pos, parent).Forget();
                    break;
                case ObstacleObject obstacle:
                    if (TryGetObstacleBlastFx(obstacle, out var obstacleBlastFx)) return;
                    PlayBlastFxAsync(obstacleBlastFx, pos, parent).Forget();
                    break;
            }
        }

        private bool TryGetItemBlastFx(ItemObject item, out ItemBlastFxView itemBlastFx)
        {
            itemBlastFx = _fxViewFactory.GetFX<ItemBlastFxView>();
            var itemConfigContainer = _gridConfigContainer.GetConfig<ItemConfigContainerSO>();
            if (!itemConfigContainer.Configs.TryGet(item.ItemType, out var itemConfig)) return true;
            itemBlastFx.Initialize(itemConfig.BlastColor);
            return false;
        }

        private bool TryGetObstacleBlastFx(ObstacleObject obstacle, out ObstacleBlastFxView obstacleBlastFx)
        {
            obstacleBlastFx = _fxViewFactory.GetFX<ObstacleBlastFxView>();
            var obstacleConfigContainer = _gridConfigContainer.GetConfig<ObstacleConfigContainerSO>();
            if (!obstacleConfigContainer.Configs.TryGet(obstacle.ObstacleType, out var obstacleConfig)) return true;
            obstacleBlastFx.Initialize(obstacleConfig.ShatteredSprites);
            return false;
        }

        private async UniTask PlayBlastFxAsync(BlastFxView blastFxView, Vector3 pos, Transform parent)
        {
            blastFxView.transform.SetParent(parent, false);
            blastFxView.transform.position = pos;
            await blastFxView.PlayAsync();
            OnBlastFxComplete(blastFxView);
        }

        private void OnBlastFxComplete(BlastFxView blastFxView) => _fxViewFactory.ReleaseFX(blastFxView);
    }
}