using Core.Item;
using Core.UI;

namespace Core.Presenters
{
    public readonly struct PendingGoalFX
    {
        public readonly ObstacleType ObstacleType;
        public readonly ImageFXView FxView;
    
        public PendingGoalFX(ObstacleType obstacleType, ImageFXView fxView)
        {
            ObstacleType = obstacleType;
            FxView = fxView;
        }
    }
}