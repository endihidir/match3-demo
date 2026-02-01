using Core.UI;
using UnityEngine;

namespace Core.Item.Factories
{
    public interface IAnimatedFXViewFactory
    {
        public T GetImageFX<T>(Transform parent, Vector3 pos, Sprite sprite, Vector2 size, bool show = true) where T : BaseImageFXView;
        public void ReleaseImageFX(BaseImageFXView animatedFX);
        public void ReleaseImageFX(Transform animatedFX);
        public void ReleaseImageFXByType<T>() where T : BaseImageFXView;
        public void RemoveImageFXPoolByType<T>() where T : BaseImageFXView;
    }
}