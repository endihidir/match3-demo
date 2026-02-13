using Core.UI;
using UnityEngine;

namespace Core.Item.Factories
{
    public interface IBoosterFxFactory
    {
        BombFxView GetBombFx(Vector3 pos, int radius, bool activate = true);
        T GetRocketFx<T>(Vector3 pos, float cellSize, float animSpeed, bool activate = true) where T : RocketFxView;
        void ReleaseBooster(BoosterFxView blastFxView);
        void ReleaseSlot(Transform blastFxView);
        void ReleaseBoosterByType<T>() where T : BoosterFxView;
        void RemovePoolsByType<T>() where T : BoosterFxView;
    }
}