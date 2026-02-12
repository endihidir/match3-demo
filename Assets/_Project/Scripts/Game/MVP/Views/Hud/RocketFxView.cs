using System;
using Core.Modules;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.UI
{
    public abstract class RocketFxView : BoosterFxView
    {
        [field: SerializeField] protected float SizeMultiplier { get; set; } = .75f;
        [field: SerializeField] protected float Speed { get; set; } = 20f; 
        [field: SerializeField] protected float ScreenPadding { get; set; } = 0.2f;
        [field: SerializeField] protected Ease Ease { get; set; } = Ease.Linear;
        [field: SerializeField] protected MoveAnimationModule PositiveSideMove { get; set; }
        [field: SerializeField] protected MoveAnimationModule NegativeSideMove { get; set; }
        [field: SerializeField] protected SpriteRenderer PositiveSideSpriteRenderer { get; set; }
        [field: SerializeField] protected SpriteRenderer NegativeSideSpriteRenderer { get; set; }
        
        protected Vector3 _negativeSideTargetPos, _positiveSideTargetPos;
        
        protected Camera _cam;

        public abstract void Initialize(Vector3 startPos, float cellSize);
        
        public override async UniTask Play(Action onComplete = null)
        {
            Stop();
            
            var positiveDuration = GetDistance(PositiveSideMove.Transform.position, _positiveSideTargetPos) / Speed;
            var negativeDuration = GetDistance(NegativeSideMove.Transform.position, _negativeSideTargetPos) / Speed;
    
            var positiveTween = PositiveSideMove.MoveTo(_positiveSideTargetPos, positiveDuration, ease: Ease);
            var negativeTween = NegativeSideMove.MoveTo(_negativeSideTargetPos, negativeDuration, ease: Ease);
    
            await UniTask.WhenAll(positiveTween.ToUniTask(), negativeTween.ToUniTask());
    
            onComplete?.Invoke();
        }
        
        protected abstract float GetDistance(Vector3 from, Vector3 to);

        public override void Stop()
        {
            ResetAnimation(PositiveSideMove);
            ResetAnimation(NegativeSideMove);
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            Stop();
        }

        private static void ResetAnimation(MoveAnimationModule animationModule)
        {
            animationModule.Dispose();
            animationModule.GetTransform().localPosition = Vector3.zero;
        }
    }
}