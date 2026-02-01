using Core.Views;
using UnityEngine;

namespace Core.UI
{
    public class GoalFxView : BaseImageFXView
    {
        [field: SerializeField] public MoveAnimationView MoveAnimation { get; private set; }
        [field: SerializeField] public SizeAnimationView SizeAnimation { get; private set; }
    }
}