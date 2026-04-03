using Cysharp.Threading.Tasks;
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

        public void Shake()
        {
            var duration = Config.ShakeDuration / 3f;
            
            _shakeTween?.Kill(true); 
            
            _shakeTween = DOTween.Sequence()
                .Append(ItemHolder.transform.DOLocalRotate(Vector3.forward * Config.ShakeRotAngle, duration))
                .Append(ItemHolder.transform.DOLocalRotate(Vector3.back * Config.ShakeRotAngle, duration))
                .Append(ItemHolder.transform.DOLocalRotate(Vector3.zero, duration))
                .OnComplete(() => ItemHolder.transform.localRotation = Quaternion.identity)
                .SetUpdate(Config.UseUnscaledTime);
        }

        private void Spring()
        {
            _springTween?.Kill();
            
            _springTween = DOTween.Sequence()
                .Append(ItemHolder.transform.DOScale(Config.SpringScale, Config.SpringDuration).SetEase(Ease.OutQuad))
                .Join(ItemHolder.transform.DOLocalMoveY(_itemHolderDefaultPos.y - Config.SpringYMoveOffset, Config.SpringDuration).SetEase(Ease.OutQuad))
                .Append(ItemHolder.transform.DOScale(Vector3.one, Config.SpringDuration).SetEase(Ease.InQuad))
                .Join(ItemHolder.transform.DOLocalMoveY(_itemHolderDefaultPos.y, Config.SpringDuration).SetEase(Ease.InQuad))
                .SetUpdate(Config.UseUnscaledTime);
        }

        public Tween ShiftTo(Vector3 worldPos, float cellDistance, float delay = 0f)
        {
            KillMovementTweens();
            
            _shiftTween?.Kill();

            var distanceMultiplier = Config.ShiftDistanceMultiplier;
            var duration = Config.ShiftDuration + (cellDistance * distanceMultiplier);

            _shiftTween = transform.DOMove(worldPos, duration)
                .SetEase(Ease.InQuad)
                .SetDelay(Config.StartShiftDelay + delay)
                .SetUpdate(Config.UseUnscaledTime);
            
            SpringAsync(_shiftTween).Forget();

            return _shiftTween;
        }
        
        public float GetShiftDelay() => Config.ShiftDelay;

        public Tween SlideAlongPath(Vector3[] points, int length, float[] cellDistances, float delay = 0f)
        {
            KillMovementTweens();
            
            _slideTween?.Kill();

            var seq = DOTween.Sequence()
                .SetDelay(Config.StartSlideDelay + delay)
                .SetUpdate(Config.UseUnscaledTime);

            var distanceMultiplier = Config.SlideDistanceMultiplier;
            for (int i = 0; i < length; i++)
                seq.Append(transform.DOMove(points[i], Config.SlideDuration + (cellDistances[i] * distanceMultiplier)).SetEase(Ease.InQuad));

            _slideTween = seq;
            
            SpringAsync(_slideTween).Forget();
            
            return _slideTween;
        }

        private async UniTask SpringAsync(Tween tween)
        {
            await tween;
            Spring();
        }
        
        public float GetSlideDelay() => Config.SlideDelay;

        public Tween PingPongMove(Vector3 startPos, Vector3 targetPos)
        {
            _pingPongTween?.Kill(true);

            _pingPongTween = DOTween.Sequence()
                                .Append(transform.DOMove(targetPos, Config.PingPongDuration).SetEase(Ease.Linear))
                                .Append(transform.DOMove(startPos, Config.PingPongDuration).SetEase(Ease.Linear))
                                .SetUpdate(Config.UseUnscaledTime);
            
            return _pingPongTween;
        }

        public Tween MoveTo(Vector3 worldPos, float durationMultiplier = 1f, Ease ease = Ease.Linear)
        {
            _moveTween?.Kill(true);
            
            _moveTween = transform.DOMove(worldPos, Config.MoveDuration * durationMultiplier)
                                  .SetEase(ease)
                                  .SetUpdate(Config.UseUnscaledTime);

            return _moveTween;
        }

        private void OnDestroy()
        {
            KillIdleTweens();
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
        private void KillIdleTweens()
        {
            _shakeTween?.Kill();
            _springTween?.Kill();
        }
    }
}