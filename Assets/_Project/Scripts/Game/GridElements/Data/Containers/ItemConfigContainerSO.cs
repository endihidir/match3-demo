using Core.Item;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(fileName = "ItemConfigContainer", menuName = "Match3/ItemConfigs/ItemConfigContainer", order = -1)]
    public class ItemConfigContainerSO : BaseItemConfigContainerSO
    {
        [field: SerializeField] 
        public EnumConfigMap<ItemType, ItemDataSO> Configs { get; private set; }
        
        [Button]
        protected void EnsureAllKeysExist() => Configs.EnsureAllKeysExist();
    }
}