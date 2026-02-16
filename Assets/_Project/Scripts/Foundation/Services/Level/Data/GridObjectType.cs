using System;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Item
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
    }
}