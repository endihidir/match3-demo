using Core.Item;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "ObstacleConfig", menuName = "Match3/ItemConfigs/ObstacleConfig", order = -1)]
    public class ObstacleConfigSO : BaseItemConfigSO
    {
        [field: SerializeField] 
        public EnumConfigMap<ObstacleType, ObstacleDataSO> Configs { get; private set; }

        [Button]
        protected void EnsureAllKeysExist() => Configs.EnsureAllKeysExist();
    }
}