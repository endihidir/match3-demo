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
        void Initialize(Vector2Int gridPos, GridItemKind itemKind, int typeId);
        void Initialize(Vector2Int gridPos, GridObjectTypeData typeData) => Initialize(gridPos, typeData.gridItemKind, typeData.typeId);
        void Dispose();
    }
    
    [Serializable]
    public class GridItemState : IGridItemState
    {
        [field: SerializeField, ReadOnly, AllowNesting] public Vector2Int GridPos { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public GridItemKind ItemKind { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public int TypeId { get; private set; }
        
        public Transform Transform { get; private set; }
        public SpriteRenderer SpriteRenderer { get; private set; }
        public IItemAnimation ItemAnimation { get; private set; }
        
        private IItemSpriteProvider ItemSpriteProvider { get; }
        private IItemAnimationFactory ItemAnimationFactory { get; }

        private GridObjectTypeData _typeData;

        public GridItemState(ItemBehaviourData itemBehaviourData)
        {
            Transform = itemBehaviourData.itemObject.Transform;
            SpriteRenderer = itemBehaviourData.itemObject.SpriteRenderer;
            ItemSpriteProvider = itemBehaviourData.itemSpriteProvider;
            ItemAnimationFactory = itemBehaviourData.itemAnimationFactory;
        }
        
        public void Initialize(Vector2Int gridPos, GridItemKind itemKind, int typeId)
        {
            SetGridPos(gridPos);
            ApplyItem(itemKind, typeId);
        }
        
        public void SetGridPos(Vector2Int gridPos) => GridPos = gridPos;
        public void ApplyItem(GridItemKind itemKind,  int typeId)
        {
            ItemKind = itemKind;
            TypeId = typeId;
            _typeData = new GridObjectTypeData(ItemKind, typeId);
            
            var sprite = ItemSpriteProvider.GetSprite(itemKind, typeId);
            SetSprite(sprite);
      
            ItemAnimationFactory.Release(ItemAnimation);
            var animationEntity = new AnimationEntity { objectReader = this, gridObjectType = _typeData };
            ItemAnimation = ItemAnimationFactory.Get(animationEntity);
        }
        
        public void Dispose()
        {
            GridPos = default;
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

        public void SetSize(Vector2 size)
        {
            if(!SpriteRenderer) return;
            SpriteRenderer.size = size;
        }
    }

    public struct ItemBehaviourData
    {
        public IGridItemObject itemObject;
        public IItemAnimationFactory itemAnimationFactory;
        public IItemSpriteProvider itemSpriteProvider;
    }
}