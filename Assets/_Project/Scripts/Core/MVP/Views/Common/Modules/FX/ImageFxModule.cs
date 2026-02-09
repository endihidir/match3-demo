using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class ImageFxModule : MonoBehaviour
    {
        [field: SerializeField, Required] public Image Icon { get; set; }
        
        public void SetSprite(Sprite sprite) => Icon.sprite = sprite;
        public void SetSize(Vector2 size) => Icon.rectTransform.sizeDelta = size;
        
        public void SetColor(Color color) => Icon.color = color;
        
        public void SetRaycastTarget(bool value) => Icon.raycastTarget = value;
        
        public void SetAlpha(float alpha)
        {
            var color = Icon.color;
            color.a = alpha;
            Icon.color = color;
        }
    }
}