using Core.Config;
using Core.Item.Factories;
using UnityEngine;

namespace Core.Item
{
    public interface IItemType
    {
        GridItemKind ItemKind { get; }
        int TypeId { get; }
        void ApplyItem(GridItemKind itemKind, int typeId);
        void ApplyItem(ItemType type) => ApplyItem(GridItemKind.Regular, (int)type);
        void ApplyItem(BoosterType type) => ApplyItem(GridItemKind.Booster, (int)type);
        void ApplyItem(ObstacleType type) => ApplyItem(GridItemKind.Obstacle, (int)type);
    }

    public interface IItemObject
    {
        Vector2Int GridPos { get; }
        SpriteRenderer SpriteRenderer { get; }
        Transform Transform { get; }
        IBaseItemEffect ItemEffect { get; }
        void SetGridPos(Vector2Int gridPos);
    }

    public interface IGridItemBehaviour : IItemType, IItemObject
    {
        void Initialize(Vector2Int gridPos, GridItemKind itemKind, int typeId);
        void Dispose();
    }
    
    public class GridItemBehaviour : IGridItemBehaviour
    {
        public SpriteRenderer SpriteRenderer { get; private set; }
        public Transform Transform { get; private set; }
        
        public GridItemKind ItemKind { get; private set; }
        public int TypeId { get; private set; }
        public Vector2Int GridPos { get; private set; }
        public IBaseItemEffect ItemEffect { get; private set; }

        private IItemSpriteProvider ItemSpriteProvider { get; }
        private IItemEffectFactory ItemEffectFactory { get; }

        public GridItemBehaviour(ItemBehaviourData itemBehaviourData)
        {
            Transform = itemBehaviourData.objectView.Transform;
            SpriteRenderer = itemBehaviourData.objectView.SpriteRenderer;
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
            ItemEffect = ItemEffectFactory.GetEffect(this, ItemKind, TypeId);
        }

        public void Dispose()
        {
            ItemKind = GridItemKind.None;
            TypeId = 0;
            SpriteRenderer.sprite = null;
            ItemEffectFactory.ReleaseEffect(ItemEffect);
        }
    }

    public struct ItemBehaviourData
    {
        public IGridItemObjectView objectView;
        public IItemEffectFactory itemEffectFactory;
        public IItemSpriteProvider itemSpriteProvider;
    }
}