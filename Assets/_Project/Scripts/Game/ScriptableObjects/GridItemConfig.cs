using Core.Item;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Config
{
    //[CreateAssetMenu(fileName = "ItemConfigContainer", menuName = "Match3/ItemConfigContainer", order = 0)]
    public class ItemConfigContainer : ScriptableObject, IItemVisualProvider
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
        
        public Sprite GetSprite(GridItemKind kind, int typeId) => kind switch
        {
            GridItemKind.Regular => GetConfig<ItemConfig>().GetSprite((ItemType)typeId),
            GridItemKind.Booster => GetConfig<BoosterItemConfig>().GetSprite((BoosterType)typeId),
            GridItemKind.Obstacle => GetConfig<ObstacleItemConfig>().GetSprite((ObstacleType)typeId),
            _ => null
        };

        public float GetSizeMultiplier(GridItemKind kind, int typeId) => kind switch
        {
            GridItemKind.Regular => GetConfig<ItemConfig>().GetSizeMultiplier((ItemType)typeId),
            GridItemKind.Booster => GetConfig<BoosterItemConfig>().GetSizeMultiplier((BoosterType)typeId),
            GridItemKind.Obstacle => GetConfig<ObstacleItemConfig>().GetSizeMultiplier((ObstacleType)typeId),
            _ => 1f
        };

        public ItemAnimationConfig GetAnimationConfig(GridItemKind kind, int typeId) => kind switch
        {
            GridItemKind.Regular => GetConfig<ItemConfig>().GetAnimationConfig((ItemType)typeId),
            GridItemKind.Booster => GetConfig<BoosterItemConfig>().GetAnimationConfig((BoosterType)typeId),
            GridItemKind.Obstacle => GetConfig<ObstacleItemConfig>().GetAnimationConfig((ObstacleType)typeId),
            _ => null
        };
    }
    
    public interface IItemVisualProvider
    {
        Sprite GetSprite(GridItemKind kind, int typeId);
        float GetSizeMultiplier(GridItemKind kind, int typeId);
    }
}
