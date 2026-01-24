using Core.Item;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "BoosterConfig", menuName = "Match3/ItemConfigs/BoosterConfig", order = -1)]
    public class BoosterConfigSO : BaseItemConfigSO
    {
        [field: SerializeField] 
        public EnumConfigMap<BoosterType, BoosterDataSO> Configs { get; private set; }
        
        [Button]
        protected void EnsureAllKeysExist() => Configs.EnsureAllKeysExist();
    }
}