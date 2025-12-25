using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "BoosterConfigData", menuName = "Match3/ItemConfigs/Data/BoosterConfigData", order = -1)]
    public class BoosterConfigData : BaseItemConfigData
    {
        [field: FormerlySerializedAs("<BoosterEffect>k__BackingField")] [field: SerializeReference] public BoosterActionBase BoosterAction { get; private set; }
    }
}