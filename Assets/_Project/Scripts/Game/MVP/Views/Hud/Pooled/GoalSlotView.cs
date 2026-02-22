using Game.Configs;
using Game.Grid.Item;
using Core.Modules;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Views
{
    public class GoalSlotView : BaseSlotView
    {
        [field: SerializeField, ReadOnly] public ObstacleType ObstacleType { get; private set; }
        [field: SerializeField] private Image GoalIcon { get; set; }
        [field: SerializeField] private TextMeshProUGUI GoalCountTxt { get; set; }
        [field: SerializeField] private BounceAnimationModule BounceAnimationModule { get; set; }

        public void Initialize(ObstacleType obstacleType) => ObstacleType = obstacleType;
        public void ApplyData(ObstacleDataSO obstacleData) => GoalIcon.sprite = obstacleData.Icon;
        public Vector2 GetIconSize() => GoalIcon.rectTransform.rect.size;
        public void SetGoalCount(int goalCount) => GoalCountTxt.SetText(goalCount.ToString());
        public void DecrementGoalCount(int amount = 1, bool useBounceAnim = true)
        {
            if (!int.TryParse(GoalCountTxt.text, out var current)) return;
            
            if (useBounceAnim) BounceAnimationModule?.PlayBounce();

            var result = Mathf.Max(0, current - amount).ToString();
            
            GoalCountTxt.SetText(result);
        }
    }
}