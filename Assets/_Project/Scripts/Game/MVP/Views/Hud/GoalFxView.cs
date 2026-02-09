using Core.Views;
using UnityEngine;

namespace Core.UI
{
    public class GoalFxView : BaseFxView
    {
        [field: SerializeField] public MoveAnimationModule MoveAnimation { get; private set; }
        [field: SerializeField] public SizeAnimationModule SizeAnimation { get; private set; }
        [field: SerializeField] public ImageFxModule ImageFxModule { get; private set; }

        public void Initialize(Transform parent, Vector3 pos, Sprite sprite, Vector2 size)
        {
            transform.SetParent(parent, false);
            transform.position = pos;
            ImageFxModule.SetSprite(sprite);
            ImageFxModule.SetSize(size);
        }
    }
}