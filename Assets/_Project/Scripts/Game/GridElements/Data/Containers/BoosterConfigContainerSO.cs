using Core.Item;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(fileName = "BoosterConfigContainer", menuName = "Match3/ItemConfigs/BoosterConfigContainer", order = -1)]
    public class BoosterConfigContainerSO : BaseItemConfigContainerSO
    {
        [field: SerializeField] public BoosterComboConfigSO BoosterComboConfigSo { get; private set; }
        [field: SerializeField] public EnumConfigMap<BoosterType, BoosterDataSO> Configs { get; private set; }
        
        [Button]
        protected void EnsureAllKeysExist() => Configs.EnsureAllKeysExist();
    }
}