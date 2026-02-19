using Game.Grid.Item;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "ObstacleConfigContainer", menuName = "Game/Gameplay/Grid/Containers/ObstacleConfigContainer")]
    public class ObstacleConfigContainerSO : BaseGridObjectConfigContinerSO
    {
        [field: SerializeField] 
        public EnumConfigMap<ObstacleType, ObstacleDataSO> Configs { get; private set; }

        [Button]
        protected void EnsureAllKeysExist() => Configs.EnsureAllKeysExist();
    }
}