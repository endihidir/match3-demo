using Game.Configs;
using DG.Tweening;
using UnityEngine;

namespace Game.Grid.Item
{
    public class GridObjectAnimation : MonoBehaviour
    {
        [field: SerializeField] private GridObjectAnimationConfigSO Config { get; set; }
        [field: SerializeField] private Transform ItemHolder { get; set; }
        
        public bool IsFallInProgress => (_shiftTween != null && _shiftTween.IsActive() && !_shiftTween.IsComplete()) ||
                                        (_slideTween != null && _slideTween.IsActive() && !_slideTween.IsComplete());

        private Tween _shakeTween, _moveTween, _pingPongTween, _shiftTween, _slideTween, _springTween;

        private Vector3 _itemHolderDefaultPos;
        
        private void Awake()
        {
            _itemHolderDefaultPos = ItemHolder.localPosition;
        }
        public void CacheAnimations()
        {
            CacheShakeTween();
            CacheSpringTween();
        }

        private void CacheShakeTween()
        {
            var duration = Config.ShakeDuration / 3f;

            if(_shakeTween != null) return;
            
            _shakeTween?.Kill(); 
            
            _shakeTween = DOTween.Sequence()
                .SetAutoKill(false)
                .Append(ItemHolder.transform.DOLocalRotate(Vector3.forward * Config.ShakeRotAngle, duration))
                .Append(ItemHolder.transform.DOLocalRotate(Vector3.back * Config.ShakeRotAngle, duration))
                .Append(ItemHolder.transform.DOLocalRotate(Vector3.zero, duration))
                .OnComplete(() => ItemHolder.transform.localRotation = Quaternion.identity)
                .Pause()
                .SetUpdate(Config.UseUnscaledTime);
        }

        private void CacheSpringTween()
        {
            if(_springTween != null) return;
            
            _springTween?.Kill();
            
            _springTween = DOTween.Sequence()
                .SetAutoKill(false)
                .Append(ItemHolder.transform.DOScale(Config.SpringScale, Config.SpringDuration).SetEase(Ease.OutQuad))
                .Join(ItemHolder.transform.DOLocalMoveY(_itemHolderDefaultPos.y - Config.SpringYMove, Config.SpringDuration).SetEase(Ease.OutQuad))
                .Append(ItemHolder.transform.DOScale(Vector3.one, Config.SpringDuration).SetEase(Ease.InQuad))
                .Join(ItemHolder.transform.DOLocalMoveY(_itemHolderDefaultPos.y, Config.SpringDuration).SetEase(Ease.InQuad))
                .Pause()
                .SetUpdate(Config.UseUnscaledTime);
        }

        public void Shake() => _shakeTween?.Restart();
        private void Spring() => _springTween?.Restart();

        public Tween ShiftTo(Vector3 worldPos, float cellDistance, float delay = 0f)
        {
            KillMovementTweens();
            
            _shiftTween?.Kill();

            var distanceMultiplier = Config.ShiftDistanceMultiplier;
            var duration = Config.BaseShiftDuration + (cellDistance * distanceMultiplier);

            _shiftTween = transform.DOMove(worldPos, duration)
                .SetEase(Ease.InQuad)
                .SetDelay(Config.BaseShiftDelay + delay)
                .OnComplete(Spring)
                .SetUpdate(Config.UseUnscaledTime);

            return _shiftTween;
        }
        
        public float GetShiftDelay() => Config.ShiftDelay;

        public Tween SlideAlongPath(Vector3[] points, int length, float[] cellDistances, float delay = 0f)
        {
            KillMovementTweens();
            
            _slideTween?.Kill();

            var seq = DOTween.Sequence()
                .SetDelay(Config.BaseSlideDelay + delay)
                .SetUpdate(Config.UseUnscaledTime);

            var distanceMultiplier = Config.SlideDistanceMultiplier;
            for (int i = 0; i < length; i++)
                seq.Append(transform.DOMove(points[i], Config.BaseSlideDuration + (cellDistances[i] * distanceMultiplier)).SetEase(Ease.InQuad));

            _slideTween = seq.OnComplete(Spring);
            return _slideTween;
        }
        
        public float GetSlideDelay() => Config.SlideDelay;

        public Tween PingPongMove(Vector3 defaultPos, Vector3 targetPos)
        {
            _pingPongTween?.Kill(true);

            _pingPongTween = DOTween.Sequence()
                                .Append(transform.DOMove(targetPos, Config.BasePingPongDuration).SetEase(Ease.Linear))
                                .Append(transform.DOMove(defaultPos, Config.BasePingPongDuration).SetEase(Ease.Linear))
                                .SetUpdate(Config.UseUnscaledTime);
            
            return _pingPongTween;
        }

        public Tween MoveTo(Vector3 worldPos, float durationMultiplier = 1f, Ease ease = Ease.Linear)
        {
            _moveTween?.Kill(true);
            
            _moveTween = transform.DOMove(worldPos, Config.BaseMoveDuration * durationMultiplier)
                                  .SetEase(ease)
                                  .SetUpdate(Config.UseUnscaledTime);

            return _moveTween;
        }

        private void OnDestroy()
        {
            KillCachedTweens();
            Dispose();
        }

        public void Dispose()
        {
            KillPlacementTweens();
            KillMovementTweens();
        }

        private void KillPlacementTweens()
        {
            _slideTween?.Kill(true);
            _shiftTween?.Kill(true);
        }
        
        private void KillMovementTweens()
        {
            _moveTween?.Kill(true);
            _pingPongTween?.Kill(true);
        }
        private void KillCachedTweens()
        {
            _shakeTween?.Kill();
            _springTween?.Kill();
        }
    }
}