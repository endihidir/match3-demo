using System;
using Core.Utils;
using DG.Tweening;
using UnityEngine;

namespace Core.Item
{
    public class ItemAnimation : MonoBehaviour
    {
        [field: SerializeField] private bool UseUnscaledTime { get; set; } = true;
        [field: SerializeField] private Transform ItemHolder { get; set; }
        
        public bool IsShiftInProgress => _shiftTween.IsActive();
        
        private Tween _shakeTween, _moveTween, _shiftTween, _springTween;

        private const float BaseShiftDuration = 0.15f;
        private const float BaseMoveDuration = 0.15f;
        private const float TotalShakeDuration = 0.25f;
        private const float ShakeRotAngle = 10f;

        private Vector3 _itemHolderDefaultPos;
        private void Awake()
        {
            _itemHolderDefaultPos = ItemHolder.localPosition;
        }

        public void Shake()
        {
            _shakeTween?.Kill(true);
            
            var duration = TotalShakeDuration / 3f;

            _shakeTween = DOTween.Sequence()
                .Append(ItemHolder.transform.DORotate(Vector3.forward * ShakeRotAngle, duration))
                .Append(ItemHolder.transform.DORotate(Vector3.back * ShakeRotAngle, duration))
                .Append(ItemHolder.transform.DORotate(Vector3.zero, duration))
                .OnComplete(() => ItemHolder.transform.localRotation = Quaternion.identity)
                .SetUpdate(UseUnscaledTime);
        }

        private void Spring()
        {
            var springDuration = 0.02f;
            var releaseDuration = 0.03f;
            _springTween.Kill(true);
            
            _springTween = DOTween.Sequence()
                .Append(ItemHolder.transform.DOScale(new Vector3(1f, 0.9f, 1f), springDuration).SetEase(Ease.InOutQuad))
                .Join(ItemHolder.transform.DOLocalMoveY(_itemHolderDefaultPos.y - 0.1f, springDuration).SetEase(Ease.InOutQuad))
                .Append(ItemHolder.transform.DOScale(Vector3.one, releaseDuration).SetEase(Ease.InOutQuad))
                .Join(ItemHolder.transform.DOLocalMoveY(_itemHolderDefaultPos.y, releaseDuration).SetEase(Ease.InOutQuad))
                .SetUpdate(UseUnscaledTime);
        }

        public Tween Shift(Vector3 worldPos, float durationMultiplier = 1f, float delay = 0f)
        {
            _shiftTween?.Kill();
            
            var duration = BaseShiftDuration * durationMultiplier;

            _shiftTween = transform.DOMove(worldPos, duration)
                .SetEase(Ease.InQuad)
                .SetDelay(delay)
                .OnComplete(Spring)
                .SetUpdate(UseUnscaledTime);

            return _shiftTween;
        }
        
        public Tween ShiftPath(Vector3[] worldPoints, float durationMultiplier = 1f, float delay = 0f, float cellSize = 1f)
        {
            _shiftTween?.Kill();
        
            if (worldPoints == null || worldPoints.Length == 0)
                return null;
        
            var current = transform.position;
            var totalDuration = 0f;
            
            for (int i = 0; i < worldPoints.Length; i++)
            {
                var next = worldPoints[i];
                var distCells = Mathf.Abs(current.y - next.y) / cellSize;
                var segMul = .5f + distCells * durationMultiplier;
                totalDuration += BaseShiftDuration * segMul;
                current = next;
            }
        
            _shiftTween = transform.DOPath(worldPoints, totalDuration, PathType.Linear, PathMode.Ignore)
                .SetEase(Ease.InQuad)
                .SetDelay(delay)
                .OnComplete(Spring)
                .SetUpdate(UseUnscaledTime);
        
            return _shiftTween;
        }

        public Tween PingPongMove(Vector3 targetPos)
        {
            _moveTween?.Kill();
                
            var defaultPos = transform.position;

            _moveTween = DOTween.Sequence()
                                .Append(transform.DOMove(targetPos, BaseMoveDuration).SetEase(Ease.Linear))
                                .Append(transform.DOMove(defaultPos, BaseMoveDuration).SetEase(Ease.Linear))
                                .SetUpdate(UseUnscaledTime);
            
            return _moveTween;
        }

        public Tween Move(Vector3 worldPos)
        {
            _moveTween?.Kill();
            
            _moveTween = transform.DOMove(worldPos, BaseMoveDuration)
                                  .SetEase(Ease.Linear)
                                  .SetUpdate(UseUnscaledTime);

            return _moveTween;
        }
        
        public void Dispose()
        {
            _shiftTween?.Kill();
            _moveTween?.Kill();
            _shakeTween?.Kill();
            _springTween?.Kill();
        }

        private void OnDestroy() => Dispose();
    }
}