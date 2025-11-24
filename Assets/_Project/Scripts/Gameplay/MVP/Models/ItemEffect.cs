using Core.Config;

namespace Core.Item
{
    public class ItemEffect : BaseItemEffect<ItemConfig>
    {
        private readonly ItemType _itemType;
        
        private readonly ItemEffectConfig _effectConfig;
        public ItemEffect(IItemObject itemObject, ItemType itemType, ItemConfig itemConfig) : base(itemObject, itemConfig)
        {
            _itemType = itemType;
            
            _effectConfig = ItemConfig.GetEffectConfig(_itemType);
        }
        
        protected override ShiftSettingsConfig GetShiftSettings() => _effectConfig.shiftSettingsConfig;
        protected override ShakeSettingsConfig GetShakeSettings() => _effectConfig.shakeSettingsConfig;
    }
}