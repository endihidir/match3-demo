using Core.Config;
using DG.Tweening;
using UnityEngine;

namespace Core.Item
{
    public class ItemAnimation : MonoBehaviour
    {
        [field: SerializeField] private ItemAnimationSettings Settings { get; set; }
        [field: SerializeField] private Transform ItemHolder { get; set; }
        
        public bool IsShiftInProgress => _shiftTween.IsActive();
        
        private Tween _shakeTween, _moveTween, _shiftTween, _springTween;

        private Vector3 _itemHolderDefaultPos;
        
        private void Awake()
        {
            _itemHolderDefaultPos = ItemHolder.localPosition;
        }
        private void Start()
        {
            CreateShakeTween();
            CreateSpringTween();
        }

        private void CreateShakeTween()
        {
            var duration = Settings.ShakeDuration / 3f;

            _shakeTween = DOTween.Sequence()
                .Append(ItemHolder.transform.DORotate(Vector3.forward * Settings.ShakeRotAngle, duration))
                .Append(ItemHolder.transform.DORotate(Vector3.back * Settings.ShakeRotAngle, duration))
                .Append(ItemHolder.transform.DORotate(Vector3.zero, duration))
                .OnComplete(() => ItemHolder.transform.localRotation = Quaternion.identity)
                .SetAutoKill(false)
                .Pause()
                .SetUpdate(Settings.UseUnscaledTime);
        }

        private void CreateSpringTween()
        {
            _springTween = DOTween.Sequence()
                .Append(ItemHolder.transform.DOScale(Settings.SpringScale, Settings.SpringDuration).SetEase(Ease.OutQuad))
                .Join(ItemHolder.transform.DOLocalMoveY(_itemHolderDefaultPos.y - Settings.SpringYMove, Settings.SpringDuration).SetEase(Ease.OutQuad))
                .Append(ItemHolder.transform.DOScale(Vector3.one, Settings.SpringDuration).SetEase(Ease.InQuad))
                .Join(ItemHolder.transform.DOLocalMoveY(_itemHolderDefaultPos.y, Settings.SpringDuration).SetEase(Ease.InQuad))
                .SetAutoKill(false)
                .Pause()
                .SetUpdate(Settings.UseUnscaledTime);
        }

        public void Shake()
        {
            if (_shakeTween == null || !_shakeTween.IsActive())
                CreateShakeTween();
            
            _shakeTween?.Restart();
        }

        private void Spring()
        {
            if (_springTween == null || !_springTween.IsActive())
                CreateSpringTween();
            
            _springTween?.Restart();
        }

        public Tween Shift(Vector3 worldPos, float durationMultiplier = 1f, float delay = 0f)
        {
            _shiftTween?.Kill();
            
            var duration = Settings.BaseShiftDuration * durationMultiplier;

            _shiftTween = transform.DOMove(worldPos, duration)
                .SetEase(Ease.InQuad)
                .SetDelay(Settings.BaseShiftDelay + delay)
                .OnComplete(Spring)
                .SetUpdate(Settings.UseUnscaledTime);

            return _shiftTween;
        }
        
        public Tween ShiftPath(Vector3[] worldPoints, float durationMultiplier = 1f, float delay = 0f)
        {
            _shiftTween?.Kill();
        
            _shiftTween = transform.DOPath(worldPoints, Settings.BaseShiftDuration * durationMultiplier, PathType.Linear, PathMode.Ignore)
                .SetEase(Ease.InQuad)
                .SetDelay(Settings.BaseShiftDelay + delay)
                .OnComplete(Spring)
                .SetUpdate(Settings.UseUnscaledTime);
        
            return _shiftTween;
        }

        public Tween PingPongMove(Vector3 targetPos)
        {
            _moveTween?.Kill();
                
            var defaultPos = transform.position;

            _moveTween = DOTween.Sequence()
                                .Append(transform.DOMove(targetPos, Settings.BaseMoveDuration).SetEase(Ease.Linear))
                                .Append(transform.DOMove(defaultPos, Settings.BaseMoveDuration).SetEase(Ease.Linear))
                                .SetUpdate(Settings.UseUnscaledTime);
            
            return _moveTween;
        }

        public Tween Move(Vector3 worldPos)
        {
            _moveTween?.Kill();
            
            _moveTween = transform.DOMove(worldPos, Settings.BaseMoveDuration)
                                  .SetEase(Ease.Linear)
                                  .SetUpdate(Settings.UseUnscaledTime);

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