using System;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Grid.Item
{
    [Serializable]
    public struct GridObjectType
    {
        [field: SerializeField, ReadOnly] 
        public GridObjectKind ObjectKind { get; private set; }
        
        [field: SerializeField, ReadOnly] 
        public int TypeId { get; private set; }

        public GridObjectType(GridObjectKind objectKind, int typeId) : this()
        {
            ObjectKind = objectKind;
            TypeId = typeId;
        }
        
        public bool Equals(GridObjectType other) => ObjectKind == other.ObjectKind && TypeId == other.TypeId;
        public override bool Equals(object obj) => obj is GridObjectType other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(ObjectKind, TypeId);
        public static bool operator ==(GridObjectType a, GridObjectType b) => a.Equals(b);
        public static bool operator !=(GridObjectType a, GridObjectType b) => !a.Equals(b);
        public override string ToString() => $"{ObjectKind}_{TypeId}";
    }
}