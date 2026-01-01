using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "BoosterData", menuName = "Match3/ItemConfigs/Data/BoosterData", order = -1)]
    public class BoosterDataSO : BaseItemDataSO
    {
        [field: SerializeReference] public BoosterActionBase BoosterAction { get; private set; }
    }
}