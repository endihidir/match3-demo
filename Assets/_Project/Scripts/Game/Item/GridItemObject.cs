using System;
using Core.Config;
using Core.Pool;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Item
{
    public interface IGridItemObject : IItemObjectReader, IItemTypeReader
    {
        IItemAnimation Animation { get; }
        void SetPosition(Vector3 position);
        void SetCellSize(float cellSize);
        void UpdateType(GridItemKind gridItemKind, int typeId);
        void UpdateCoordinate(Vector2Int coordinate);
    }
    
    public class GridItemObject : PooledObject, IGridItemObject
    {
        [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
        [field: SerializeField, ReadOnly] public GridItemKind ItemKind { get; private set; }
        [field: SerializeField, ReadOnly] public Vector2Int Coordinate { get; private set; }
        [field: SerializeField, ReadOnly] public int TypeId { get; private set; }
        [field: SerializeField, ReadOnly] public Vector2 CellSize { get; private set; }
        [field: SerializeField, ReadOnly] public bool IsStatic { get; private set; }
        public Transform Transform => transform;
        public IItemAnimation Animation { get; private set; }
        private bool _isEmpty;
        
        public bool IsEmpty
        {
            get => _isEmpty;
            set
            {
                _isEmpty = value;
                IsStatic = !_isEmpty;
                SpriteRenderer.enabled = !_isEmpty;
            }
        }

        private float _cellSizeMultiplier;
        
        public GridItemObject Initialize(GridItemKind gridItemKind, int typeId, Vector2Int coordinate)
        {
            ItemKind = gridItemKind;
            TypeId = typeId;
            Coordinate = coordinate;
            IsEmpty = gridItemKind is GridItemKind.None || typeId == 0;
            return this;
        }
        public GridItemObject BindAnimation(IItemAnimation anim)
        {
            Animation = anim;
            return this;
        }
        
        public GridItemObject ApplyData(IItemVisualConfig visualConfig)
        {
            if (!SpriteRenderer) return this;
            var sprite = visualConfig.GetSprite(ItemKind, TypeId);
            SpriteRenderer.sprite = sprite;
            _cellSizeMultiplier = visualConfig.GetSizeMultiplier(ItemKind, TypeId);
            return this;
        }

        public void SetPosition(Vector3 position) => Transform.position = position;

        public void SetCellSize(float cellSize)
        {
            if (!SpriteRenderer) return;
            CellSize = Vector2.one * (cellSize * _cellSizeMultiplier);
            SpriteRenderer.size = CellSize;
        }

        public void UpdateType(GridItemKind gridItemKind, int typeId)
        {
            ItemKind = gridItemKind;
            TypeId = typeId;
        }
        public void UpdateCoordinate(Vector2Int coordinate) => Coordinate = coordinate;

        public void ResetState()
        {
            IsEmpty = true;
            Animation?.Dispose();
            Animation = null;
            Coordinate = default;
        }

        public override void Deactivate(float duration = 0, float delay = 0, Action onComplete = null)
        {
            base.Deactivate(duration, delay, onComplete);
            ResetState();
        }
        
        private void OnDestroy() => ResetState();
    }
}