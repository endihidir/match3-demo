using Game.Grid.Item;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "ItemConfigContainer", menuName = "Game/Gameplay/Grid/Containers/ItemConfigContainer")]
    public class ItemConfigContainerSO : BaseGridObjectConfigContinerSO
    {
        [field: SerializeField] 
        public EnumConfigMap<ItemType, ItemDataSO> Configs { get; private set; }
        
        [Button]
        protected void EnsureAllKeysExist() => Configs.EnsureAllKeysExist();
    }
}