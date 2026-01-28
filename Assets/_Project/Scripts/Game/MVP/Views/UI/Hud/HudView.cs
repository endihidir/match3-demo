using NaughtyAttributes;
using TMPro;
using UnityEngine;

namespace Core.UI
{
    public class HudView : MonoBehaviour, IHudView
    {
        [field: SerializeField] private Transform GoalsHolder { get; set; }
        [field: SerializeField] private TextMeshProUGUI MoveCountTxt { get; set; }
        [field: SerializeField, ReadOnly] public GoalSlotView[] GoalSlotViews { get; private set; }
        
        public void Initialize(GoalSlotView[] goalSlotViews, int moveCount)
        {
            GoalSlotViews = goalSlotViews;
            
            foreach (var goalSlotView in GoalSlotViews)
            {
                goalSlotView.transform.SetParent(GoalsHolder, false);
            }
            
            SetMoveCount(moveCount);
        }
        
        public void SetMoveCount(int moveCount)
        {
            MoveCountTxt.SetText(moveCount.ToString());
        }
    }
}