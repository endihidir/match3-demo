using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class ImageFXView : BaseAnimatedFXView
    {
        [field: SerializeField, Required] public Image Image { get; private set; }
        
        public override void SetSprite(Sprite sprite) => Image.sprite = sprite;
        public override void SetSize(Vector2 size) => Image.rectTransform.sizeDelta = size;
        
        public override BaseAnimatedFXView SetSize(Vector2 size, float duration, float delay = 0f, Ease ease = Ease.Linear)
        {
            _sizeTween?.Kill();
            
            _sizeTween = Image.rectTransform.DOSizeDelta(size, duration)
                                            .SetEase(ease)
                                            .SetDelay(delay)
                                            .SetUpdate(UseUnscaledTime);
            return this;
        }
    }
}