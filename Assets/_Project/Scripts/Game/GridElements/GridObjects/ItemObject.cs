using NaughtyAttributes;
using UnityEngine;

namespace Core.Item
{
    public class ItemObject : BaseGridObject
    {
        [field: SerializeField, ReadOnly] public ItemType ItemType { get; private set; }
        protected override void OnInitialize()
        {
            ItemType = (ItemType)TypeId;
        }
        protected override void OnDespawned()
        {
            base.OnDespawned();
            ItemType = (ItemType)TypeId;
            UpdateIdentity();
        }

        public override string ToString() => $"Type: {ItemType}";
    }
}