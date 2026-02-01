using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public abstract class BaseImageFXView : BaseFxView
    {
        [field: SerializeField, Required] public Image Image { get; private set; }
        public void SetSprite(Sprite sprite) => Image.sprite = sprite;
        public void SetSize(Vector2 size) => Image.rectTransform.sizeDelta = size;
    }
}