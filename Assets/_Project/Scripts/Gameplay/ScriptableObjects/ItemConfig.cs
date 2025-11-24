using System;
using Core.Item;
using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "ItemConfig", menuName = "Match3/ItemConfigs/ItemConfig", order = -1)]
    public class ItemConfig : BaseEnumConfig<ItemType, ItemConfigData>
    {
        public Sprite GetSprite(ItemType itemType) => Configs[itemType].sprite;
        public ItemEffectConfig GetEffectConfig(ItemType itemType) => Configs[itemType].itemEffectConfig;
    }
       
    [Serializable]
    public struct ItemConfigData
    {
        public Sprite sprite;
        public ItemEffectConfig itemEffectConfig;
    }
}