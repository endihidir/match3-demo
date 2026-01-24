using Core.Item;
using UnityEngine;

namespace Core.Config
{
    //[CreateAssetMenu(fileName = "ItemConfigContainer", menuName = "Match3/ItemConfigContainer", order = 0)]
    public class ItemConfigContainerSO : ScriptableObject
    { 
        [field: SerializeField] private BaseItemConfigSO[] ItemConfigs { get; set; }
        [field: SerializeField] public BoosterComboConfigSO BoosterComboConfigSo { get; private set; }
        
        public BaseItemDataSO GetConfigData(GridObjectType objectType) => objectType.ItemKind switch
        {
            GridItemKind.Regular => GetConfig<ItemConfigSO>().Configs.Get((ItemType)objectType.TypeId),
            GridItemKind.Booster => GetConfig<BoosterConfigSO>().Configs.Get((BoosterType)objectType.TypeId),
            GridItemKind.Obstacle => GetConfig<ObstacleConfigSO>().Configs.Get((ObstacleType)objectType.TypeId),
            _ => null
        };
        private T GetConfig<T>() where T : BaseItemConfigSO
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
    }
}
