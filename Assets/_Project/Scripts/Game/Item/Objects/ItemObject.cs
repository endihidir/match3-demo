using Core.Config;
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

        public override void ApplyData(BaseItemConfigData baseItemConfigData)
        {
            base.ApplyData(baseItemConfigData);

            if (baseItemConfigData is ItemConfigData itemConfigData)
            {
                
            }
        }

        public override string ToString() => $"X: {Coordinate.x}, Y: {Coordinate.y}, Type: {ItemType}";
    }
}