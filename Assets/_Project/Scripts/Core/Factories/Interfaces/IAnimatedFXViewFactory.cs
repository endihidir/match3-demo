using Core.UI;
using UnityEngine;

namespace Core.Item.Factories
{
    public interface IAnimatedFXViewFactory
    {
        public T GetAnimatedFX<T>(Transform parent, Vector3 pos, Sprite sprite, Vector2 size, bool show = true) where T : BaseAnimatedFXView;
        public void ReleaseAnimatedFX(BaseAnimatedFXView animatedFX);
        public void ReleaseAnimatedFX(Transform animatedFX);
        public void ReleaseAnimatedFXByType<T>() where T : BaseAnimatedFXView;
        public void RemoveAnimatedFXPoolByType<T>() where T : BaseAnimatedFXView;
    }
}