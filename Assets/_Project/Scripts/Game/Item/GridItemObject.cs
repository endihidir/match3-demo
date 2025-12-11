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
        public Transform Transform => transform;
        public IItemAnimation Animation { get; private set; }
        
        public GridItemObject Initialize(GridItemKind gridItemKind, int typeId, Vector2Int coordinate, Vector2 cellSize)
        {
            ItemKind = gridItemKind;
            TypeId = typeId;
            Coordinate = coordinate;
            CellSize = cellSize;
            return this;
        }
        public GridItemObject BindAnimation(IItemAnimation anim)
        {
            Animation = anim;
            return this;
        }
        public GridItemObject ApplyVisual(IItemVisualConfig visualConfig)
        {
            if (!SpriteRenderer) return this;
            var sprite = visualConfig.GetSprite(ItemKind, TypeId);
            SpriteRenderer.sprite = sprite;
            var sizeMultiplier = visualConfig.GetSizeMultiplier(ItemKind, TypeId);
            SpriteRenderer.size = CellSize * sizeMultiplier;
            return this;
        }
        
        public void UpdateType(GridItemKind gridItemKind, int typeId)
        {
            ItemKind = gridItemKind;
            TypeId = typeId;
        }
        public void UpdateCoordinate(Vector2Int coordinate) => Coordinate = coordinate;

        public void ResetState()
        {
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