using Core.Config;
using DG.Tweening;
using UnityEngine;

namespace Core.Item
{
    public class ItemAnimation : MonoBehaviour
    {
        [field: SerializeField] private ItemAnimationSettings Settings { get; set; }
        [field: SerializeField] private Transform ItemHolder { get; set; }
        
        public bool IsFallInProgress => (_shiftTween != null && _shiftTween.IsActive() && !_shiftTween.IsComplete()) ||
                                        (_slideTween != null && _slideTween.IsActive() && !_slideTween.IsComplete());

        private Tween _shakeTween, _moveTween, _pingPongTween, _shiftTween, _slideTween, _springTween;

        private Vector3 _itemHolderDefaultPos;
        
        private void Awake()
        {
            _itemHolderDefaultPos = ItemHolder.localPosition;
        }
        public void InitAnimations()
        {
            CreateShakeTween();
            CreateSpringTween();
        }

        private void CreateShakeTween()
        {
            var duration = Settings.ShakeDuration / 3f;

            if(_shakeTween != null) return;
            
            _shakeTween?.Kill(); 
            
            _shakeTween = DOTween.Sequence()
                .SetAutoKill(false)
                .Append(ItemHolder.transform.DOLocalRotate(Vector3.forward * Settings.ShakeRotAngle, duration))
                .Append(ItemHolder.transform.DOLocalRotate(Vector3.back * Settings.ShakeRotAngle, duration))
                .Append(ItemHolder.transform.DOLocalRotate(Vector3.zero, duration))
                .OnComplete(() => ItemHolder.transform.localRotation = Quaternion.identity)
                .Pause()
                .SetUpdate(Settings.UseUnscaledTime);
        }

        private void CreateSpringTween()
        {
            if(_springTween != null) return;
            
            _springTween?.Kill();
            
            _springTween = DOTween.Sequence()
                .SetAutoKill(false)
                .Append(ItemHolder.transform.DOScale(Settings.SpringScale, Settings.SpringDuration).SetEase(Ease.OutQuad))
                .Join(ItemHolder.transform.DOLocalMoveY(_itemHolderDefaultPos.y - Settings.SpringYMove, Settings.SpringDuration).SetEase(Ease.OutQuad))
                .Append(ItemHolder.transform.DOScale(Vector3.one, Settings.SpringDuration).SetEase(Ease.InQuad))
                .Join(ItemHolder.transform.DOLocalMoveY(_itemHolderDefaultPos.y, Settings.SpringDuration).SetEase(Ease.InQuad))
                .Pause()
                .SetUpdate(Settings.UseUnscaledTime);
        }

        public void Shake() => _shakeTween?.Restart();
        private void Spring() => _springTween?.Restart();

        public Tween ShiftTo(Vector3 worldPos, float cellDistance, float delay = 0f)
        {
            KillInputTweens();
            
            _shiftTween?.Kill(true);

            var distanceMultiplier = Settings.ShiftDistanceMultiplier;
            var duration = Settings.BaseShiftDuration + (cellDistance * distanceMultiplier);

            _shiftTween = transform.DOMove(worldPos, duration)
                .SetEase(Ease.InQuad)
                .SetDelay(Settings.BaseShiftDelay + delay)
                .OnComplete(Spring)
                .SetUpdate(Settings.UseUnscaledTime);

            return _shiftTween;
        }
        
        public float GetShiftTimelineDuration(float cellDistance)
        {
            var totalTime = Settings.BaseShiftDelay + Settings.BaseShiftDuration + cellDistance * Settings.ShiftDistanceMultiplier;
            var result = Mathf.Max(0.05f, totalTime - Settings.ShiftEarlyStartSeconds);
            return .05f;
        }
        
        public Tween SlideAlongPath(Vector3[] points, int length, float[] cellDistances, float delay = 0f)
        {
            KillInputTweens();
            
            _slideTween?.Kill(true);

            var seq = DOTween.Sequence()
                .SetDelay(Settings.BaseSlideDelay + delay)
                .SetUpdate(Settings.UseUnscaledTime);

            var distanceMultiplier = Settings.SlideDistanceMultiplier;
            for (int i = 0; i < length; i++)
                seq.Append(transform.DOMove(points[i], Settings.BaseSlideDuration + (cellDistances[i] * distanceMultiplier)).SetEase(Ease.InQuad));

            _slideTween = seq.OnComplete(Spring);
            return _slideTween;
        }
        
        public float GetSlideTimelineDuration(float[] cellDistances, int length)
        {
            var totalTime = Settings.BaseSlideDelay;
            
            for (int i = 0; i < length; i++)
                totalTime += Settings.BaseSlideDuration + cellDistances[i] * Settings.SlideDistanceMultiplier;

            var result = Mathf.Max(0.05f, totalTime - Settings.SlideEarlyStartSeconds);
          
            return .05f;
        }

        public Tween PingPongMove(Vector3 defaultPos, Vector3 targetPos)
        {
            _pingPongTween?.Kill(true);

            _pingPongTween = DOTween.Sequence()
                                .Append(transform.DOMove(targetPos, Settings.BasePingPongDuration).SetEase(Ease.Linear))
                                .Append(transform.DOMove(defaultPos, Settings.BasePingPongDuration).SetEase(Ease.Linear))
                                .SetUpdate(Settings.UseUnscaledTime);
            
            return _pingPongTween;
        }

        public Tween MoveTo(Vector3 worldPos, float durationMultiplier = 1f, Ease ease = Ease.Linear)
        {
            _moveTween?.Kill(true);
            
            _moveTween = transform.DOMove(worldPos, Settings.BaseMoveDuration * durationMultiplier)
                                  .SetEase(ease)
                                  .SetUpdate(Settings.UseUnscaledTime);

            return _moveTween;
        }

        private void KillInputTweens()
        {
            _moveTween?.Kill(true);
            _pingPongTween?.Kill(true);
        }
        
        public void Dispose()
        {
            _slideTween?.Kill();
            _shiftTween?.Kill();
            _moveTween?.Kill();
            _pingPongTween?.Kill();
        }

        private void OnDestroy()
        {
            _shakeTween?.Kill();
            _springTween?.Kill();
            Dispose();
        }
    }
}