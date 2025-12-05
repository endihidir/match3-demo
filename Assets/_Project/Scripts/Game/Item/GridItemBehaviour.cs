using Core.Config;
using Core.Item.Factories;
using Core.Level;
using UnityEngine;

namespace Core.Item
{
    public interface IGridItemBehaviour : IItemTypeReader, IItemTypeWriter, IItemObjectReader, IItemObjectWriter
    {
        IBaseItemEffect ItemEffect { get; }
        void Initialize(Vector2Int gridPos, GridItemKind itemKind, int typeId);
        void Initialize(Vector2Int gridPos, GridObjectTypeData typeData) => Initialize(gridPos, typeData.gridItemKind, typeData.typeId);
        void Dispose();
    }
    
    public class GridItemBehaviour : IGridItemBehaviour
    {
        public Transform Transform { get; private set; }
        public SpriteRenderer SpriteRenderer { get; private set; }
        public GridItemKind ItemKind { get; private set; }
        public int TypeId { get; private set; }
        public Vector2Int GridPos { get; private set; }
        public IBaseItemEffect ItemEffect { get; private set; }
        
        private IItemSpriteProvider ItemSpriteProvider { get; }
        private IItemEffectFactory ItemEffectFactory { get; }

        public GridItemBehaviour(ItemBehaviourData itemBehaviourData)
        {
            Transform = itemBehaviourData.itemObject.Transform;
            SpriteRenderer = itemBehaviourData.itemObject.SpriteRenderer;
            ItemSpriteProvider = itemBehaviourData.itemSpriteProvider;
            ItemEffectFactory = itemBehaviourData.itemEffectFactory;
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
            SpriteRenderer.sprite = ItemSpriteProvider.GetSprite(ItemKind, TypeId);
            
            ItemEffectFactory.ReleaseEffect(ItemEffect);
            ItemEffect = ItemEffectFactory.GetEffect(this, ItemKind, TypeId);
        }

        public void Dispose()
        {
            GridPos = default;
            ItemKind = GridItemKind.None;
            TypeId = 0;
            SpriteRenderer.sprite = null;
            ItemEffectFactory.ReleaseEffect(ItemEffect);
        }
    }

    public struct ItemBehaviourData
    {
        public IGridItemObject itemObject;
        public IItemEffectFactory itemEffectFactory;
        public IItemSpriteProvider itemSpriteProvider;
    }
}