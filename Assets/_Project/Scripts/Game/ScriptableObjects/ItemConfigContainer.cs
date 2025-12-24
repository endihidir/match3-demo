using Core.Item;
using UnityEngine;

namespace Core.Config
{
    //[CreateAssetMenu(fileName = "ItemConfigContainer", menuName = "Match3/ItemConfigContainer", order = 0)]
    public class ItemConfigContainer : ScriptableObject
    { 
        [field: SerializeField] private BaseItemConfig[] ItemConfigs { get; set; }
        [field: SerializeField] public BoosterMergeConfig BoosterMergeConfig { get; private set; }

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

        public BaseItemConfigData GetConfigData(GridObjectTypeData typeData) => typeData.ItemKind switch
        {
            GridItemKind.Regular => GetConfig<ItemConfig>().GetData((ItemType)typeData.TypeId),
            GridItemKind.Booster => GetConfig<BoosterItemConfig>().GetData((BoosterType)typeData.TypeId),
            GridItemKind.Obstacle => GetConfig<ObstacleItemConfig>().GetData((ObstacleType)typeData.TypeId),
            _ => null
        };

        public BoosterConfigData GetBoosterData(BoosterType boosterType)
        {
            var config = GetConfig<BoosterItemConfig>();
            return config ? config.GetData(boosterType) : null;
        }
    }
}
