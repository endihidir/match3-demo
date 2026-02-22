using NaughtyAttributes;
using UnityEngine;

namespace Game.Grid.Item
{
    public class ItemObject : BaseGridObject
    {
        [field: SerializeField, ReadOnly] public ItemType ItemType { get; private set; }
        protected override void OnInitialize()
        {
            ItemType = (ItemType)TypeId;
            UpdateIdentity();
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