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
        public override void Deactivate()
        {
            base.Deactivate();
            ItemType = (ItemType)TypeId;
            UpdateIdentity();
        }

        public override string ToString() => $"Type: {ItemType}";
    }
}