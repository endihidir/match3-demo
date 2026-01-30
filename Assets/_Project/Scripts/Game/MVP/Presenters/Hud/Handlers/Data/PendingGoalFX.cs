using Core.Item;
using Core.UI;

namespace Core.Presenters
{
    public readonly struct PendingGoalFX
    {
        public readonly ObstacleType ObstacleType;
        public readonly GoalFxView FxView;
    
        public PendingGoalFX(ObstacleType obstacleType, GoalFxView fxView)
        {
            ObstacleType = obstacleType;
            FxView = fxView;
        }
    }
}