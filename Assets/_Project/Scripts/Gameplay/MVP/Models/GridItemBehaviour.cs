using System;
using Core.Config;
using UnityEngine;

namespace Core.Item
{
    public interface IItemConfig
    {
        ItemConfigContainer ItemConfigContainer { get; }
    }
    public interface IItemType
    {
        Enum GridItemType { get; }
        void ApplyItemType(Enum type);
    }

    public interface IItemAnimation
    {
        IBaseItemEffect ItemEffect { get; }
    }

    public interface IItemObject
    {
        SpriteRenderer SpriteRenderer { get; }
        Vector2Int GridPos { get; }
        
        Transform Transform { get; }
        void SetGridPos(Vector2Int gridPos);
    }

    public interface IGridItemBehaviour : IItemType, IItemObject, IItemConfig, IItemAnimation
    {
        
    }
    
    public class GridItemBehaviour : IGridItemBehaviour
    {
        public ItemConfigContainer ItemConfigContainer { get; private set; }
        public SpriteRenderer SpriteRenderer { get; private set; }
        public Transform Transform { get; private set; }
        
        public Enum GridItemType { get; private set; }
        public Vector2Int GridPos { get; private set; }
        
        public IBaseItemEffect ItemEffect { get; private set; }

        public GridItemBehaviour(Transform transform, SpriteRenderer spriteRenderer, ItemConfigContainer itemConfigContainer)
        {
            Transform = transform;
            SpriteRenderer = spriteRenderer;
            ItemConfigContainer = itemConfigContainer;
        }
        
        public void SetGridPos(Vector2Int gridPos) => GridPos = gridPos;
        public void ApplyItemType(Enum type)
        {
            GridItemType = type;

            SetSprite(GridItemType);
            
            CreateEffect(GridItemType);
        }
        
        public T GetItemAnimation<T>() where T : IBaseItemEffect => (T)ItemEffect;

        private void CreateEffect(Enum type) => ItemEffect = type switch
        {
            ItemType itemType => new ItemEffect(this, itemType, ItemConfigContainer.GetConfig<ItemConfig>()),
            ObstacleType obstacleType => new ObstacleEffect(this, obstacleType, ItemConfigContainer.GetConfig<ObstacleConfig>()),
            BoosterType boosterType => new BoosterEffect(this, boosterType, ItemConfigContainer.GetConfig<BoosterConfig>()),
            _ => null
        };

        private void SetSprite(Enum itemType) => SpriteRenderer.sprite = ItemConfigContainer.GetSprite(itemType);
    }

    public enum ItemType
    {
        None = 0,
        Red = 1,
        Blue = 2,
        Green = 3,
        Yellow = 4
    }
    
    public enum BoosterType
    {
        None = 0,
        RocketHorizontal = 1,
        RocketVertical = 2,
        Bomb = 3,
    }

    public enum ObstacleType
    {
        None = 0,
        Box = 1,
        Vase = 2
    }
}

