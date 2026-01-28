using Core.Item;
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
        
        public void SetType(ObstacleType obstacleType) => ObstacleType = obstacleType;
        public void SetIcon(Sprite springIcon) => GoalIcon.sprite = springIcon;
        public void SetGoalCount(int goalCount) => GoalCountTxt?.SetText(goalCount.ToString());
    }
}