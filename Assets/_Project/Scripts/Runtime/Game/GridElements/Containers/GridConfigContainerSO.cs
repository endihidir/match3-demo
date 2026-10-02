using Game.Grid.Item;
using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "GridConfigContainer", menuName = "Game/Gameplay/Containers/GridConfigContainer")]
    public sealed class GridConfigContainerSO : ScriptableObject
    { 
        [field: SerializeField] public FillSpawnDecisionConfigSO FillSpawnDecisionConfig { get; private set; }
        [field: SerializeField] public GridObjectAnimationConfigSO AnimationConfig { get; private set; }
        [field: SerializeField] private BaseGridObjectConfigContinerSO[] GridItemConfigContainers { get; set; }
        
        public BaseGridObjectDataSO GetConfigData(GridObjectType objectType) => objectType.ObjectKind switch
        {
            GridObjectKind.Regular => GetConfig<ItemConfigContainerSO>().Configs.Get((ItemType)objectType.TypeId),
            GridObjectKind.Booster => GetConfig<BoosterConfigContainerSO>().Configs.Get((BoosterType)objectType.TypeId),
            GridObjectKind.Obstacle => GetConfig<ObstacleConfigContainerSO>().Configs.Get((ObstacleType)objectType.TypeId),
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
