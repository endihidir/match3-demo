using Core.Config;
using Core.Configs;
using Core.UI;
using UnityEngine;

namespace Core.Item.Factories
{
    public class BlastFxFactory : IBlastFxFactory
    {
        private readonly IFXViewFactory _fxViewFactory;
        private readonly GridConfigContainerSO _gridConfigContainer;

        public BlastFxFactory(IFXViewFactory fxViewFactory, GameplayConfigContainer gameplayConfigContainer)
        {
            _fxViewFactory = fxViewFactory;
            _gridConfigContainer = gameplayConfigContainer.GridConfigContainer;
        }

        public ItemBlastFxView GetItemBlast(ItemType itemType, bool activate = true)
        {
            var fx = _fxViewFactory.GetFX<ItemBlastFxView>(activate);
            var itemConfigContainer = _gridConfigContainer.GetConfig<ItemConfigContainerSO>();
            if (!itemConfigContainer.Configs.TryGet(itemType, out var itemConfig)) return fx;
            fx.Initialize(itemConfig.BlastColor);
            return fx;
        }

        public ObstacleBlastFxView GetObstacleBlast(ObstacleType obstacleType, bool activate = true)
        {
            var fx = _fxViewFactory.GetFX<ObstacleBlastFxView>(activate);
            var obstacleConfigContainer = _gridConfigContainer.GetConfig<ObstacleConfigContainerSO>();
            if (!obstacleConfigContainer.Configs.TryGet(obstacleType, out var obstacleConfig)) return fx;
            fx.Initialize(obstacleConfig.ShatteredSprites);
            return fx;
        }
        
        public void ReleaseBlast(BlastFxView blastFxView) => _fxViewFactory.ReleaseFX(blastFxView);
        public void ReleaseSlot(Transform blastFxView) => _fxViewFactory.ReleaseFX(blastFxView);
        public void ReleaseBlastsByType<T>() where T : BlastFxView => _fxViewFactory.ReleaseFXByType<T>();
        public void RemovePoolsByType<T>() where T : BlastFxView => _fxViewFactory.RemoveFXPoolByType<T>();
    }
}