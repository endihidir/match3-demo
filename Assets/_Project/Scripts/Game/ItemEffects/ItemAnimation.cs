using Core.Config;
using Core.Extensions;
using Core.Item.Factories;
using Core.Level;
using Core.Models;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Core.Item
{
    public interface IItemAnimation
    {
        bool IsInProgress { get; }
        GridObjectTypeData GridObjectTypeData { get; }
        void Initialize(ItemAnimationConfig defaultItemSettings);
        void Shake();
        Tween Shift(IGridModel<IGridItemState> gridModel);
        UniTask ShiftAsync(IGridModel<IGridItemState> gridModel);
        void ForceComplete();
        void Dispose();
    }
    
    public class ItemAnimation : IItemAnimation
    {
        private readonly IItemObjectReader _objectReader;
        
        private ItemAnimationConfig _animationConfig;
        private Tween _shakeTween, _shiftTween;
        
        public GridObjectTypeData GridObjectTypeData { get; }
        public bool IsInProgress => _shakeTween.IsActive() || _shiftTween.IsActive();
        public ItemAnimation(ItemAnimationData ıtemAnimationData)
        {
            _objectReader = ıtemAnimationData.objectReader;
            GridObjectTypeData = ıtemAnimationData.gridObjectType;
        }

        public void Initialize(ItemAnimationConfig itemAnimationConfig) => _animationConfig = itemAnimationConfig;

        public virtual void Shake()
        {
            var shakeSettings = _animationConfig.ShakeSettings;

            _shakeTween?.Kill();

            _shakeTween = _objectReader.Transform.DOShakePosition(shakeSettings.duration, shakeSettings.strength)
                                                 .SetUpdate(_animationConfig.UseUnscaledTime);
        }

        public virtual Tween Shift(IGridModel<IGridItemState> gridModel)
        {
            var worldPos = gridModel.GridToWorld(_objectReader.Coordinate);
            
            var shiftSettings = _animationConfig.ShiftSettings;

            _shiftTween?.Kill();

            _shiftTween = _objectReader.Transform.DOMove(worldPos, shiftSettings.duration)
                                                 .SetUpdate(_animationConfig.UseUnscaledTime);

            return _shiftTween;
        }

        public virtual async UniTask ShiftAsync(IGridModel<IGridItemState> gridModel)
        {
            var tween = Shift(gridModel);
            
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