using NaughtyAttributes;
using UnityEngine;

namespace Core.Item
{
    public class ItemObject : BaseItemObject
    {
        [field: SerializeField, ReadOnly] public ItemType ItemType { get; private set; }
        protected override void OnInitialize(int typeId)
        {
            ItemType = (ItemType)typeId;
        }
        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            ItemType = ItemType.None;
        }
    }
}