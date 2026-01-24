using Core.Item;
using UnityEngine;

namespace Core.Config
{
    //[CreateAssetMenu(fileName = "GridConfigContainer", menuName = "Match3/GridConfigContainer", order = 0)]
    public class GridConfigContainerSO : ScriptableObject
    { 
        [field: SerializeField] public FillItemDecisionSettingsSO FillItemDecisionSettings { get; private set; }
        [field: SerializeField] private BaseItemConfigContainerSO[] ConfigContainers { get; set; }
        
        public BaseItemDataSO GetConfigData(GridObjectType objectType) => objectType.ItemKind switch
        {
            GridItemKind.Regular => GetConfig<ItemConfigContainerSO>().Configs.Get((ItemType)objectType.TypeId),
            GridItemKind.Booster => GetConfig<BoosterConfigContainerSO>().Configs.Get((BoosterType)objectType.TypeId),
            GridItemKind.Obstacle => GetConfig<ObstacleConfigContainerSO>().Configs.Get((ObstacleType)objectType.TypeId),
            _ => null
        };
        
        public T GetConfig<T>() where T : BaseItemConfigContainerSO
        {
            foreach (var baseItemConfig in ConfigContainers)
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
