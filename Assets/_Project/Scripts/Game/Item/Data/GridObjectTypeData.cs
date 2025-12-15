using System;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Item
{
    [Serializable]
    public struct GridObjectTypeData
    {
        [field: SerializeField, ReadOnly] 
        public GridItemKind ItemKind { get; private set; }
        
        [field: SerializeField, ReadOnly, AllowNesting] 
        public string Type { get; private set; }
        
        [field: SerializeField, ReadOnly] 
        public int TypeId { get; private set; }

        public GridObjectTypeData(GridItemKind itemKind, int typeId) : this()
        {
            ItemKind = itemKind;
            TypeId = typeId;
            Type = GetTypeName(ItemKind,  TypeId);
        }

        private string GetTypeName(GridItemKind itemKind, int typeId) => itemKind switch
        {
            GridItemKind.Regular => ((ItemType)typeId).ToString(),
            GridItemKind.Booster => ((BoosterType)typeId).ToString(),
            GridItemKind.Obstacle => ((ObstacleType)typeId).ToString(),
            _ => "None"
        };

        public override string ToString()
        {
            return $"{ItemKind}: {Type}: {TypeId}";
        }
    }
}