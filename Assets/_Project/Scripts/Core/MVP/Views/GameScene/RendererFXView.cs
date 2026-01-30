using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace Core.UI
{
    public abstract class RendererFXView : BaseAnimatedFXView
    {
        [field: SerializeField, Required] public SpriteRenderer Renderer { get; private set; }
        
        public override void SetSprite(Sprite sprite) => Renderer.sprite = sprite;
        
        public override void SetSize(Vector2 size) => Renderer.size = size;
        
        public override BaseAnimatedFXView SetSize(Vector2 size, float duration, float delay = 0f, Ease ease = Ease.Linear)
        {
            _sizeTween?.Kill();

            var defaultSize = Renderer.size;
            
            _sizeTween = DOVirtual.Vector2(defaultSize, size, duration, s=> Renderer.size = s)
                                  .SetEase(ease)
                                  .SetDelay(delay)
                                  .SetUpdate(UseUnscaledTime);
            
            return this;
        }
    }
}