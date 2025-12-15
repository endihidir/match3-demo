using System;
using Core.Config;
using Core.Pool;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Item
{
    public abstract class BaseItemObject : PooledObject, IItemObject, IItemType
    {
        [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
        [field: SerializeField] public ItemAnimation ItemAnimation { get; private set; }
        [field: SerializeField, ReadOnly] public Vector2Int Coordinate { get; private set; }
        [field: SerializeField, ReadOnly] public Vector2 CellSize { get; private set; }
        [field: SerializeField, ReadOnly] public bool IsStatic { get; private set; }
        public GridObjectTypeData GridObjectType { get; private set; }
        public Transform Transform => transform;
        
        private Vector2 _cellSizeMultiplier;
        private bool _isEmpty;
        
        public bool IsEmpty
        {
            get => _isEmpty;
            set
            {
                _isEmpty = value;
                if (_isEmpty) IsStatic = false;
                SpriteRenderer.enabled = !_isEmpty;
            }
        }
        
        public BaseItemObject Initialize(GridObjectTypeData gridObjectTypeData, Vector2Int coordinate)
        {
            GridObjectType = gridObjectTypeData;
            Coordinate = coordinate;
            OnInitialize(GridObjectType.TypeId);
            name = ToString();
            IsEmpty = GridObjectType.ItemKind is GridItemKind.None || gridObjectTypeData.TypeId == 0;
            return this;
        }
        protected abstract void OnInitialize(int typeId);
        
        public virtual void ApplyData(BaseItemConfigData baseItemConfigData)
        {
            var sprite = baseItemConfigData.icon;
            SpriteRenderer.sprite = sprite;
            _cellSizeMultiplier = baseItemConfigData.spriteSizeMultiplier;
        }
        public void SetPosition(Vector3 position) => Transform.position = position;
        public void SetCellSize(Vector2 cellSize)
        {
            CellSize = cellSize * _cellSizeMultiplier;
            SpriteRenderer.size = CellSize;
        }
        public void SetParent(Transform parent) => Transform.SetParent(parent);
        public void UpdateTypeData(GridItemKind gridItemKind, int typeId) => GridObjectType = new GridObjectTypeData(gridItemKind, typeId);
        public void UpdateCoordinate(Vector2Int coordinate) => Coordinate = coordinate;
        public void ResetState()
        {
            IsEmpty = true;
            Coordinate = default;
            GridObjectType = default;
            CellSize = default;
            name = ToString();
        }
        public override void Deactivate(float duration = 0, float delay = 0, Action onComplete = null)
        {
            base.Deactivate(duration, delay, onComplete);
            ResetState();
        }
        private void OnDestroy() => ResetState();
    }
}