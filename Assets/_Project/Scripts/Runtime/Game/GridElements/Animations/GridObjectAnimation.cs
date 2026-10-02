using System.Collections.Generic;
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
        
        public bool IsFallInProgress
        {
            get
            {
                for (int i = 0; i < _placementTweens.Count; i++)
                {
                    if (IsRunning(_placementTweens[i])) return true;
                }

                return false;
            }
        }

        public int PlacementVersion => _placementVersion;

        private readonly List<Tween> _placementTweens = new(4);
        private Tween _shakeTween, _moveTween, _pingPongTween, _springTween;
        private int _placementVersion;

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

        public Tween PlayPlacement(Vector3[] points, float[] durations, float[] waits, float[] easeSlopes, int count, float startDelay)
        {
            KillMovementTweens();
            RemoveFinishedPlacements();

            var version = ++_placementVersion;

            var sequence = DOTween.Sequence()
                .SetDelay(startDelay)
                .SetUpdate(Config.UseUnscaledTime);

            for (int i = 0; i < count; i++)
            {
                if (waits[i] > 0f)
                    sequence.AppendInterval(waits[i]);

                sequence.Append(transform.DOMove(points[i], durations[i]).SetEase(QuadraticEase.Get(easeSlopes[i])));
            }

            _placementTweens.Add(sequence);

            SpringAsync(sequence, version).Forget();

            return sequence;
        }

        private async UniTask SpringAsync(Tween tween, int version)
        {
            await tween;

            if (version != _placementVersion) return;

            Spring();
        }

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
            _placementVersion++;
            KillPlacementTweens();
            KillMovementTweens();
            _springTween?.Kill(true);
        }

        private void KillPlacementTweens()
        {
            for (int i = 0; i < _placementTweens.Count; i++)
                _placementTweens[i]?.Kill(true);

            _placementTweens.Clear();
        }

        private void RemoveFinishedPlacements()
        {
            for (int i = _placementTweens.Count - 1; i >= 0; i--)
            {
                if (!IsRunning(_placementTweens[i]))
                    _placementTweens.RemoveAt(i);
            }
        }

        private static bool IsRunning(Tween tween) => tween != null && tween.IsActive() && !tween.IsComplete();
        
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