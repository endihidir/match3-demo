using System;
using Core.Config;
using Core.Item.Factories;
using Core.Level;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Item
{
    public interface IGridItemState : IItemTypeReader, IItemTypeWriter, IItemObjectReader, IItemObjectWriter
    {
        IItemAnimation ItemAnimation { get; }
        void Initialize(Vector2Int gridPos, Vector2 cellSize, GridItemKind itemKind, int typeId);
        void Initialize(Vector2Int gridPos, Vector2 cellSize, GridObjectTypeData typeData) => Initialize(gridPos, cellSize, typeData.gridItemKind, typeData.typeId);
        void Dispose();
    }
    
    [Serializable]
    public class GridItemState : IGridItemState
    {
        [field: SerializeField, ReadOnly, AllowNesting] public Vector2Int Coordinate { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public GridItemKind ItemKind { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public int TypeId { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public Vector2 CellSize { get; private set; }
        
        public Transform Transform { get; private set; }
        public SpriteRenderer SpriteRenderer { get; private set; }
        public IItemAnimation ItemAnimation { get; private set; }
        
        private IItemVisualProvider ItemVisualProvider { get; }
        private IItemAnimationFactory ItemAnimationFactory { get; }
        
        public GridItemState(ItemBehaviourData itemBehaviourData)
        {
            Transform = itemBehaviourData.itemObject.Transform;
            SpriteRenderer = itemBehaviourData.itemObject.SpriteRenderer;
            ItemVisualProvider = itemBehaviourData.itemVisualProvider;
            ItemAnimationFactory = itemBehaviourData.itemAnimationFactory;
        }
        
        public void Initialize(Vector2Int gridPos, Vector2 cellSize, GridItemKind itemKind, int typeId)
        {
            SetCoordinate(gridPos);
            SetCellSize(cellSize);
            ApplyItem(itemKind, typeId);
        }
        
        public void SetCoordinate(Vector2Int coordinate) => Coordinate = coordinate;
        public void SetCellSize(Vector2 size) => CellSize = size;
        public void ApplyItem(GridItemKind itemKind,  int typeId)
        {
            ItemKind = itemKind;
            TypeId = typeId;
            var typeData = new GridObjectTypeData(ItemKind, TypeId);
            
            var sprite = ItemVisualProvider.GetSprite(itemKind, typeId);
            SetSprite(sprite);
            var sizeMultiplier = ItemVisualProvider.GetSizeMultiplier(itemKind, typeId);
            SetSpriteSize(sizeMultiplier);
      
            ItemAnimationFactory.Release(ItemAnimation);
            var animationEntity = new AnimationEntity { objectReader = this, gridObjectType = typeData };
            ItemAnimation = ItemAnimationFactory.Get(animationEntity);
        }
        
        public void Dispose()
        {
            Coordinate = default;
            ItemKind = GridItemKind.None;
            TypeId = 0;
            SetSprite(null);
            ItemAnimationFactory?.Release(ItemAnimation);
        }
        
        private void SetSprite(Sprite sprite, int sortOrder = 5)
        {
            if(!SpriteRenderer) return;
            SpriteRenderer.sprite = sprite;
            SpriteRenderer.sortingOrder = sortOrder;
        }

        private void SetSpriteSize(float sizeMultiplier)
        {
            if(!SpriteRenderer) return;
            SpriteRenderer.size = CellSize * sizeMultiplier;
        }
    }

    public struct ItemBehaviourData
    {
        public IGridItemObject itemObject;
        public IItemAnimationFactory itemAnimationFactory;
        public IItemVisualProvider itemVisualProvider;
    }
}