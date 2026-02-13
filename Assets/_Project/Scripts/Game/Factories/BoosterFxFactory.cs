using Core.UI;
using UnityEngine;

namespace Core.Item.Factories
{
    public class BoosterFxFactory : IBoosterFxFactory
    {
        private readonly IFXViewFactory _fxViewFactory;
        
        public BoosterFxFactory(IFXViewFactory fxViewFactory)
        {
            _fxViewFactory = fxViewFactory;
        }

        public BombFxView GetBombFx(Vector3 pos, int radius, bool activate = true)
        {
            var fx = _fxViewFactory.GetFX<BombFxView>(activate);
            fx.transform.position = pos;
            fx.ApplyData(radius);
            return fx;
        }

        public T GetRocketFx<T>(Vector3 pos, float cellSize, float animSpeed, bool activate = true) where T : RocketFxView
        {
            var fx = _fxViewFactory.GetFX<T>(activate);
            fx.transform.position = pos;
            fx.CalculateRocketsSize(cellSize);
            fx.CalculateTargetPositions();
            fx.ApplyData(animSpeed);
            return fx;
        }

        public void ReleaseBooster(BoosterFxView blastFxView) => _fxViewFactory.ReleaseFX(blastFxView);
        public void ReleaseSlot(Transform blastFxView) => _fxViewFactory.ReleaseFX(blastFxView);
        public void ReleaseBoosterByType<T>() where T : BoosterFxView => _fxViewFactory.ReleaseFXByType<T>();
        public void RemovePoolsByType<T>() where T : BoosterFxView => _fxViewFactory.RemoveFXPoolByType<T>();
    }
}