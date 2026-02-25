using System;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Grid.Item
{
    [Serializable]
    public struct GridObjectType
    {
        [field: SerializeField, ReadOnly] 
        public GridItemKind ItemKind { get; private set; }
        
        [field: SerializeField, ReadOnly] 
        public int TypeId { get; private set; }

        public GridObjectType(GridItemKind itemKind, int typeId) : this()
        {
            ItemKind = itemKind;
            TypeId = typeId;
        }
        
        public bool Equals(GridObjectType other) => ItemKind == other.ItemKind && TypeId == other.TypeId;
        public override bool Equals(object obj) => obj is GridObjectType other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(ItemKind, TypeId);
        public static bool operator ==(GridObjectType a, GridObjectType b) => a.Equals(b);
        public static bool operator !=(GridObjectType a, GridObjectType b) => !a.Equals(b);

        public override string ToString() => $"{ItemKind}_{TypeId}";
    }
}