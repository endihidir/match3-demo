using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "ObstacleConfigData", menuName = "Match3/ItemConfigs/Data/ObstacleConfigData", order = -1)]
    public class ObstacleConfigData : BaseItemConfigData
    {
        [field: SerializeField] public int Life { get; private set; }
    }
}