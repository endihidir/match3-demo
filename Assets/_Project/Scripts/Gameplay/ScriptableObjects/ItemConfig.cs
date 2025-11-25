using System;
using Core.Item;
using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "ItemConfig", menuName = "Match3/ItemConfigs/ItemConfig", order = -1)]
    public class ItemConfig : EnumItemConfig<ItemType, ItemConfigData>
    {
        public Sprite GetSprite(ItemType itemType) => Configs[itemType].sprite;
        public RegularEffectConfig GetEffectConfig(ItemType itemType) => Configs[itemType].regularEffectConfig;
    }
       
    [Serializable]
    public struct ItemConfigData
    {
        public Sprite sprite;
        public RegularEffectConfig regularEffectConfig;
    }
}