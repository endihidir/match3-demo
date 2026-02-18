using Game.Grid.Item;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "ObstacleConfigContainer", menuName = "Match3/ItemConfigs/ObstacleConfigContainer", order = -1)]
    public class ObstacleConfigContainerSO : BaseGridObjectConfigContinerSO
    {
        [field: SerializeField] 
        public EnumConfigMap<ObstacleType, ObstacleDataSO> Configs { get; private set; }

        [Button]
        protected void EnsureAllKeysExist() => Configs.EnsureAllKeysExist();
    }
}