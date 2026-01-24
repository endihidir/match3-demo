using Core.Item;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "ItemConfig", menuName = "Match3/ItemConfigs/ItemConfig", order = -1)]
    public class ItemConfigSO : BaseItemConfigSO
    {
        [field: SerializeField] 
        public EnumConfigMap<ItemType, ItemDataSO> Configs { get; private set; }
        
        [Button]
        protected void EnsureAllKeysExist() => Configs.EnsureAllKeysExist();
    }
}