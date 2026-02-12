using Core.Configs;
using Core.Item;
using Core.Modules;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class GoalSlotView : BaseSlotView
    {
        [field: SerializeField, ReadOnly] public ObstacleType ObstacleType { get; private set; }
        [field: SerializeField] private Image GoalIcon { get; set; }
        [field: SerializeField] private TextMeshProUGUI GoalCountTxt { get; set; }
        [field: SerializeField] private BounceAnimationModule BounceAnimationModule { get; set; }
        [field: SerializeField, ReadOnly] private int GoalCount { get; set; }

        public void Initialize(ObstacleType obstacleType)
        {
            ObstacleType = obstacleType;
        }

        public void ApplyData(ObstacleDataSO obstacleData)
        {
            GoalIcon.sprite = obstacleData.icon;
        }
        
        public Vector2 GetIconSize() => GoalIcon.rectTransform.rect.size;
        
        public void SetGoalCount(int goalCount)
        {
            GoalCount = goalCount;
            GoalCountTxt?.SetText(GoalCount.ToString());
        }

        public void DecreaseGoalCount(bool useBounceAnim = true)
        {
            if (GoalCount > 0 && useBounceAnim)
            {
                BounceAnimationModule?.PlayBounce();
            }
            
            GoalCount = Mathf.Max(GoalCount - 1, 0);
            GoalCountTxt.SetText(GoalCount.ToString());
        }
    }
}