using Game.Grid.Item;
using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "GridConfigContainer", menuName = "Game/Gameplay/Containers/GridConfigContainer")]
    public sealed class GridConfigContainerSO : ScriptableObject
    { 
        [field: SerializeField] public FillSpawnDecisionConfigSO FillSpawnDecisionConfig { get; private set; }
        [field: SerializeField] private BaseGridObjectConfigContinerSO[] GridItemConfigContainers { get; set; }
        
        public BaseGridObjectDataSO GetConfigData(GridObjectType objectType) => objectType.ItemKind switch
        {
            GridItemKind.Regular => GetConfig<ItemConfigContainerSO>().Configs.Get((ItemType)objectType.TypeId),
            GridItemKind.Booster => GetConfig<BoosterConfigContainerSO>().Configs.Get((BoosterType)objectType.TypeId),
            GridItemKind.Obstacle => GetConfig<ObstacleConfigContainerSO>().Configs.Get((ObstacleType)objectType.TypeId),
            _ => null
        };
        
        public T GetConfig<T>() where T : BaseGridObjectConfigContinerSO
        {
            foreach (var baseItemConfig in GridItemConfigContainers)
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
