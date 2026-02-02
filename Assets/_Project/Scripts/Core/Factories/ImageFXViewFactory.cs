using Core.UI;
using UnityEngine;

namespace Core.Item.Factories
{
    public sealed class ImageFXViewFactory : IAnimatedFXViewFactory
    {
        private readonly IFXViewFactory _fxFactory;

        public ImageFXViewFactory(IFXViewFactory fxFactory) => _fxFactory = fxFactory;
        
        public T GetImageFX<T>(Transform parent, Vector3 pos, Sprite sprite, Vector2 size, bool show = true) where T : BaseImageFXView
        {
            var fx = _fxFactory.GetFX<T>(show);
            fx.transform.SetParent(parent, false);
            fx.transform.position = pos;
            fx.SetSprite(sprite);
            fx.SetSize(size);
            return fx;
        }

        public void ReleaseImageFX(BaseImageFXView animatedFX) => _fxFactory.ReleaseFX(animatedFX);
        public void ReleaseImageFX(Transform animatedFX) => _fxFactory.ReleaseFX(animatedFX);
        public void ReleaseImageFXByType<T>() where T : BaseImageFXView => _fxFactory.ReleaseFXByType<T>();
        public void RemoveImageFXPoolByType<T>() where T : BaseImageFXView => _fxFactory.RemoveFXPoolByType<T>();
    }
}