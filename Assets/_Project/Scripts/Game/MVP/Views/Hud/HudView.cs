using System.Linq;
using AYellowpaper.SerializedCollections;
using Core.Item;
using Core.Utils;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

namespace Core.UI
{
    public class HudView : MonoBehaviour, IHudView
    {
        [field: SerializeField] private Transform GoalsHolder { get; set; }
        [field: SerializeField] public Transform GoalFxHolder { get; set; }
        [field: SerializeField] private TextMeshProUGUI MoveCountTxt { get; set; }
        [field: SerializeField, ReadOnly] private SerializedDictionary<ObstacleType, GoalSlotView> SlotByType { get; set; }
        
        public void Initialize(GoalSlotView[] goalSlotViews, int moveCount)
        {
            SlotByType = new SerializedDictionary<ObstacleType, GoalSlotView>(goalSlotViews.ToDictionary(x => x.ObstacleType));
            
            foreach (var goalSlotView in goalSlotViews)
            {
                goalSlotView.transform.SetParent(GoalsHolder, false);
            }
            
            SetMoveCount(moveCount);
        }
        
        public void SetMoveCount(int moveCount) => MoveCountTxt.SetText(moveCount.ToString());

        public bool TryGetGoalSlotView(ObstacleType obstacleType, out GoalSlotView goalSlotView)
        {
            if (SlotByType.TryGetValue(obstacleType, out goalSlotView)) return true;
            
            EditorLogger.LogError($"{obstacleType} slot view not found!");
            
            return false;
        }
        
        public void DecreaseGoalCount(ObstacleType obstacleType, bool useBounceAnim = true)
        {
            if (TryGetGoalSlotView(obstacleType, out var goalSlotView)) 
                goalSlotView.DecreaseGoalCount(useBounceAnim);
        }
    }
}