using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(fileName = "BoosterData", menuName = "Match3/ItemConfigs/Data/BoosterData", order = -1)]
    public class BoosterDataSO : BaseItemDataSO
    {
        [field: SerializeField] public float AnimationSpeed { get; private set; }
        [field: SerializeReference] public BoosterActionBase BoosterAction { get; private set; }
    }
}