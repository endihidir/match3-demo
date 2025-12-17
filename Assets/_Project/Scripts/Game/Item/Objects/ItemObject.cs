using NaughtyAttributes;
using UnityEngine;

namespace Core.Item
{
    public class ItemObject : BaseItemObject
    {
        [field: SerializeField, ReadOnly] public ItemType ItemType { get; private set; }
        protected override void OnInitialize()
        {
            ItemType = (ItemType)TypeId;
        }
        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            ItemType = (ItemType)TypeId;
            UpdateIdentity();
        }

        public override string ToString() => $"Type: {ItemType}";
    }
}