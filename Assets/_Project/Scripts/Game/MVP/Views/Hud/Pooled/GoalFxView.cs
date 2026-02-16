using Core.Modules;
using UnityEngine;

namespace Core.UI
{
    public class GoalFxView : BaseFxView
    {
        [field: SerializeField] public MoveAnimationModule MoveAnimation { get; private set; }
        [field: SerializeField] public SizeAnimationModule SizeAnimation { get; private set; }
        [field: SerializeField] public ImageFxModule ImageFxModule { get; private set; }
    }
}