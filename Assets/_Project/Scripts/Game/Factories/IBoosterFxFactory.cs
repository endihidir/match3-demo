using Core.UI;
using UnityEngine;

namespace Core.Item.Factories
{
    public interface IBoosterFxFactory
    {
        public BombFxView GetBombFx(Vector3 pos, int radius, bool activate = true);
        public T GetRocketFx<T>(Vector3 pos, float size, bool activate = true) where T : RocketFxView;
        public void ReleaseBooster(BoosterFxView blastFxView);
        public void ReleaseSlot(Transform blastFxView);
        public void ReleaseBoosterByType<T>() where T : BoosterFxView;
        public void RemovePoolsByType<T>() where T : BoosterFxView;
    }
}