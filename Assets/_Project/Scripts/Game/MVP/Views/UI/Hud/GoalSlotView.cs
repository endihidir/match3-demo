using Core.Item;
using DG.Tweening;
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
        [field: SerializeField, ReadOnly] private int GoalCount { get; set; }
        [field: SerializeField, ReadOnly] public bool IsCollectible { get; private set; }
        
        private Tween _bounceTween;
        
        public void SetCollectible(bool isCollectible) => IsCollectible = isCollectible;
        public void SetType(ObstacleType obstacleType) => ObstacleType = obstacleType;
        public void SetIcon(Sprite springIcon) => GoalIcon.sprite = springIcon;
        public Sprite GetIcon() => GoalIcon.sprite;
        public Vector2 GetSize() => GoalIcon.rectTransform.rect.size;
        
        public void SetGoalCount(int goalCount)
        {
            GoalCount = goalCount;
            GoalCountTxt?.SetText(GoalCount.ToString());
        }

        public void DecreaseGoalCount()
        {
            if (GoalCount > 0)
            {
                PlayBounce();
            }
            
            GoalCount = Mathf.Max(GoalCount - 1, 0);
            
            GoalCountTxt.SetText(GoalCount.ToString());
        }
        
        private void PlayBounce(float duration = 0.15f, float up = 1.2f)
        {
            _bounceTween.Kill(true);
            
            _bounceTween = DOTween.Sequence()
                .Append(GoalIcon.transform.DOScale(1f * up, duration * 0.7f).SetEase(Ease.OutQuad))
                .Append(GoalIcon.transform.DOScale(1f,duration * 0.3f).SetEase(Ease.OutBack));
        }

        private void OnDestroy() => _bounceTween.Kill();
    }
}