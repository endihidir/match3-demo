using Core.Modules;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Views
{
    public abstract class RocketFxView : BoosterFxView
    {
        [field: SerializeField, ReadOnly] protected float AnimSpeed { get; set; }
        [field: SerializeField] protected Ease Ease { get; set; } = Ease.Linear;
        [field: SerializeField] protected MoveAnimationModule PositiveSideMove { get; set; }
        [field: SerializeField] protected MoveAnimationModule NegativeSideMove { get; set; }
        [field: SerializeField] protected SpriteRenderer PositiveSideSpriteRenderer { get; set; }
        [field: SerializeField] protected SpriteRenderer NegativeSideSpriteRenderer { get; set; }
        
        protected Vector3 _negativeSideTargetPos, _positiveSideTargetPos;
        
        public void ApplyData(float animSpeed) => AnimSpeed = animSpeed;
        public abstract void UpdateRocketVisuals(float cellSize, float sizeMultiplier = .75f);
        public abstract void UpdateTargetPositions(Camera cam, Vector3 pos, float screenPadding = .2f);
        public override async UniTask PlayAsync()
        {
            Stop();
            
            var positiveDuration = GetDistance(PositiveSideMove.Transform.position, _positiveSideTargetPos) / AnimSpeed;
            var negativeDuration = GetDistance(NegativeSideMove.Transform.position, _negativeSideTargetPos) / AnimSpeed;
    
            var positiveTween = PositiveSideMove.MoveTo(_positiveSideTargetPos, positiveDuration, ease: Ease);
            var negativeTween = NegativeSideMove.MoveTo(_negativeSideTargetPos, negativeDuration, ease: Ease);
    
            await UniTask.WhenAll(positiveTween.ToUniTask(), negativeTween.ToUniTask());
        }
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
        protected abstract float GetDistance(Vector3 from, Vector3 to);
    }
}