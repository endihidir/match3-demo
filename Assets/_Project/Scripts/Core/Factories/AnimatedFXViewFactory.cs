using Core.UI;
using UnityEngine;

namespace Core.Item.Factories
{
    public sealed class AnimatedFXViewFactory : IAnimatedFXViewFactory
    {
        private readonly IFXViewFactory _fxFactory;

        public AnimatedFXViewFactory(IFXViewFactory fxFactory) => _fxFactory = fxFactory;
        
        public T GetAnimatedFX<T>(Transform parent, Vector3 pos, Sprite sprite, Vector2 size, bool show = true) where T : BaseAnimatedFXView
        {
            var fx = _fxFactory.GetFX<T>(show);
            fx.transform.SetParent(parent, false);
            fx.transform.localScale = Vector3.one;
            fx.transform.position = pos;
            fx.SetSprite(sprite);
            fx.SetSize(size);
            return fx;
        }

        public void ReleaseAnimatedFX(BaseAnimatedFXView animatedFX) => _fxFactory.ReleaseFX(animatedFX);
        public void ReleaseAnimatedFX(Transform animatedFX) => _fxFactory.ReleaseFX(animatedFX);
        public void ReleaseAnimatedFXByType<T>() where T : BaseAnimatedFXView => _fxFactory.ReleaseFXByType<T>();
        public void RemoveAnimatedFXPoolByType<T>() where T : BaseAnimatedFXView => _fxFactory.RemoveFXPoolByType<T>();
    }
}