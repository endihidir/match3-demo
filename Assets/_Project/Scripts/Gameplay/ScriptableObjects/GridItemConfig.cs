using Core.Item;
using UnityEngine;

namespace Core.Config
{
    //[CreateAssetMenu(fileName = "ItemConfigContainer", menuName = "Match3/ItemConfigContainer", order = 0)]
    public class ItemConfigContainer : ScriptableObject
    {
        [field: SerializeField] private BaseItemConfig[] ItemConfigs { get; set; }
        [field: SerializeField] public ItemEffectSettingsConfig DefaultEffectSettings { get; private set; }

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
    }
}
