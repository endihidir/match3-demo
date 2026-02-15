using System;
using TMPro;
using UnityEngine;

namespace Core.UI
{
    public class HudView : MonoBehaviour, IHudView
    {
        [field: SerializeField] public Transform GoalsHolder { get; private set; }
        [field: SerializeField] public Transform GoalFxHolder { get; private set; }
        [field: SerializeField] private TextMeshProUGUI MoveCountTxt { get; set; }
        public event Action OnInitialize;
        
        public void Initialize(int goalCount, int moveCount)
        {
            ResizeLayoutGroup(goalCount);
            
            SetMoveCount(moveCount);
            
            OnInitialize?.Invoke();
        }

        private void ResizeLayoutGroup(int goalCount)
        {
            // TODO: Resize Layout Group
        }
        
        public void SetMoveCount(int moveCount) => MoveCountTxt.SetText(moveCount.ToString());
    }
}