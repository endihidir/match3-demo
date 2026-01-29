using Core.Item;
using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "ObstacleData", menuName = "Match3/ItemConfigs/Data/ObstacleData", order = -1)]
    public class ObstacleDataSO : BaseItemDataSO
    {
        [field: SerializeField] public int Life { get; private set; }
        [field: SerializeField] public DamageSource DamageSource { get; private set; }
        [field: SerializeField] public bool IsCollectible { get; private set; }
    }
}