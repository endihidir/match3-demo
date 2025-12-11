using Core.Config;
using Core.Extensions;
using Core.Item.Factories;
using Core.Level;
using Core.Models;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Item
{
    public interface IItemAnimation
    {
        bool IsInProgress { get; }
        GridObjectTypeData GridObjectTypeData { get; }
        void Initialize(ItemAnimationConfig defaultItemSettings);
        void Shake();
        Tween Shift(Vector3 worldPos);
        UniTask ShiftAsync(Vector3 worldPos);
        void ForceComplete();
        void Dispose();
    }
    
    public class ItemAnimation : IItemAnimation
    {
        private readonly IItemObjectReader _itemObjectReader;
        
        private ItemAnimationConfig _animationConfig;
        private Tween _shakeTween, _shiftTween;
        
        public GridObjectTypeData GridObjectTypeData { get; }
        public bool IsInProgress => _shakeTween.IsActive() || _shiftTween.IsActive();
        public ItemAnimation(ItemAnimationData itemAnimationData)
        {
            _itemObjectReader = itemAnimationData.itemObjectReader;
            GridObjectTypeData = itemAnimationData.gridObjectType;
        }

        public void Initialize(ItemAnimationConfig itemAnimationConfig) => _animationConfig = itemAnimationConfig;

        public virtual void Shake()
        {
            var shakeSettings = _animationConfig.ShakeSettings;

            _shakeTween?.Kill();

            _shakeTween = _itemObjectReader.Transform.DOShakePosition(shakeSettings.duration, shakeSettings.strength)
                                                 .SetUpdate(_animationConfig.UseUnscaledTime);
        }

        public virtual Tween Shift(Vector3 worldPos)
        {
            var shiftSettings = _animationConfig.ShiftSettings;

            _shiftTween?.Kill();

            _shiftTween = _itemObjectReader.Transform.DOMove(worldPos, shiftSettings.duration)
                                                 .SetUpdate(_animationConfig.UseUnscaledTime);

            return _shiftTween;
        }

        public virtual async UniTask ShiftAsync(Vector3 worldPos)
        {
            var tween = Shift(worldPos);
            
            await tween.AsyncWaitForCompletion();
        }

        public virtual void ForceComplete()
        {
            _shakeTween?.Complete();
            _shiftTween?.Complete();
        }

        public virtual void Dispose()
        {
            _shakeTween?.Kill();
            _shiftTween?.Kill();
        }
    }
}