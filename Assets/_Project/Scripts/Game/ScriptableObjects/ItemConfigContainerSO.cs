using Core.Item;
using UnityEngine;

namespace Core.Config
{
    //[CreateAssetMenu(fileName = "ItemConfigContainer", menuName = "Match3/ItemConfigContainer", order = 0)]
    public class ItemConfigContainerSO : ScriptableObject
    { 
        [field: SerializeField] private BaseItemConfigSO[] ItemConfigs { get; set; }
        [field: SerializeField] public BoosterComboConfigSO BoosterComboConfigSo { get; private set; }

        public T GetConfig<T>() where T : BaseItemConfigSO
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

        public BaseItemDataSO GetConfigData(GridObjectType ıd) => ıd.ItemKind switch
        {
            GridItemKind.Regular => GetConfig<ItemConfigSO>().GetData((ItemType)ıd.TypeId),
            GridItemKind.Booster => GetConfig<BoosterConfigSO>().GetData((BoosterType)ıd.TypeId),
            GridItemKind.Obstacle => GetConfig<ObstacleConfigSO>().GetData((ObstacleType)ıd.TypeId),
            _ => null
        };

        public BoosterDataSO GetBoosterData(BoosterType boosterType)
        {
            var config = GetConfig<BoosterConfigSO>();
            return config ? config.GetData(boosterType) : null;
        }
    }
}
