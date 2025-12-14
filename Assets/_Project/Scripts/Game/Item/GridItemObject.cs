using System;
using Core.Config;
using Core.Pool;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Item
{
    public class GridItemObject : PooledObject, IGridItemObject
    {
        [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
        [field: SerializeField, ReadOnly] public GridObjectTypeData GridObjectTypeData { get; private set; }
        [field: SerializeField, ReadOnly] public Vector2Int Coordinate { get; private set; }
        [field: SerializeField, ReadOnly] public Vector2 CellSize { get; private set; }
        [field: SerializeField, ReadOnly] public bool IsStatic { get; private set; }
        public Transform Transform => transform;
        public IItemAnimation Animation { get; private set; }
        
        private float _cellSizeMultiplier;
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
        
        public GridItemObject Initialize(GridObjectTypeData gridObjectTypeData, Vector2Int coordinate)
        {
            GridObjectTypeData = gridObjectTypeData;
            Coordinate = coordinate;
            name = ToString();
            IsEmpty = GridObjectTypeData.ItemKind is GridItemKind.None || gridObjectTypeData.TypeId == 0;
            return this;
        }
        public GridItemObject ApplyData(IItemVisualConfig visualConfig)
        {
            if (!SpriteRenderer) return this;
            var sprite = visualConfig.GetSprite(GridObjectTypeData);
            SpriteRenderer.sprite = sprite;
            _cellSizeMultiplier = visualConfig.GetSizeMultiplier(GridObjectTypeData);
            return this;
        }
        public void BindAnimation(IItemAnimation anim) => Animation = anim;
        public void SetPosition(Vector3 position) => Transform.position = position;
        public void SetCellSize(float cellSize)
        {
            if (!SpriteRenderer) return;
            CellSize = Vector2.one * (cellSize * _cellSizeMultiplier);
            SpriteRenderer.size = CellSize;
        }
        public void SetParent(Transform parent) => Transform.SetParent(parent);
        public void UpdateTypeData(GridItemKind gridItemKind, int typeId) => GridObjectTypeData = new GridObjectTypeData(gridItemKind, typeId);
        public void UpdateCoordinate(Vector2Int coordinate) => Coordinate = coordinate;
        public void ResetState()
        {
            IsEmpty = true;
            Animation = null;
            Coordinate = default;
            GridObjectTypeData = default;
            CellSize = default;
            Animation?.Dispose();
            name = ToString();
        }
        public override void Deactivate(float duration = 0, float delay = 0, Action onComplete = null)
        {
            base.Deactivate(duration, delay, onComplete);
            ResetState();
        }
        private void OnDestroy() => ResetState();
        public override string ToString() => $"X: {Coordinate.x}, Y: {Coordinate.y}, Type: {GridObjectTypeData.Type}";
    }
}