using Core.Item;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Config
{
    //[CreateAssetMenu(fileName = "ItemConfigContainer", menuName = "Match3/ItemConfigContainer", order = 0)]
    public class ItemConfigContainer : ScriptableObject, IItemVisualConfig
    {
        [field: SerializeField, Required] public ItemAnimationConfig DefaultAnimationConfigs { get; private set; }
        [field: SerializeField] private BaseItemConfig[] ItemConfigs { get; set; }

        public T GetConfig<T>() where T : BaseItemConfig
        {
            foreach (var baseItemConfig in ItemConfigs)
            {
                if (baseItemConfig is T config) 
                {
                    return config;
                }
            }
            
            return null;
        }
        
        public Sprite GetSprite(GridObjectTypeData typeData) => typeData.ItemKind switch
        {
            GridItemKind.Regular => GetConfig<ItemConfig>().GetSprite((ItemType)typeData.TypeId),
            GridItemKind.Booster => GetConfig<BoosterItemConfig>().GetSprite((BoosterType)typeData.TypeId),
            GridItemKind.Obstacle => GetConfig<ObstacleItemConfig>().GetSprite((ObstacleType)typeData.TypeId),
            _ => null
        };

        public float GetSizeMultiplier(GridObjectTypeData typeData) => typeData.ItemKind switch
        {
            GridItemKind.Regular => GetConfig<ItemConfig>().GetSizeMultiplier((ItemType)typeData.TypeId),
            GridItemKind.Booster => GetConfig<BoosterItemConfig>().GetSizeMultiplier((BoosterType)typeData.TypeId),
            GridItemKind.Obstacle => GetConfig<ObstacleItemConfig>().GetSizeMultiplier((ObstacleType)typeData.TypeId),
            _ => 1f
        };

        public ItemAnimationConfig GetAnimationConfig(GridObjectTypeData typeData) => typeData.ItemKind switch
        {
            GridItemKind.Regular => GetConfig<ItemConfig>().GetAnimationConfig((ItemType)typeData.TypeId),
            GridItemKind.Booster => GetConfig<BoosterItemConfig>().GetAnimationConfig((BoosterType)typeData.TypeId),
            GridItemKind.Obstacle => GetConfig<ObstacleItemConfig>().GetAnimationConfig((ObstacleType)typeData.TypeId),
            _ => null
        };
    }
    
    public interface IItemVisualConfig
    {
        Sprite GetSprite(GridObjectTypeData typeData);
        float GetSizeMultiplier(GridObjectTypeData typeData);
    }
}
