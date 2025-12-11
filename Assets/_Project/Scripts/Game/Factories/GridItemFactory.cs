using Core.Config;
using Core.Configs;
using Core.Level;
using Core.Pool;
using UnityEngine;

namespace Core.Item.Factories
{
    public interface IGridItemFactory
    {
        GridItemObject GetItem(GridObjectTypeData typeData, Vector2Int coordinate, Vector2 cellSize);
        GridItemObject GetItem(GridItemKind itemKind, int typeId, Vector2Int coordinate, Vector2 cellSize) => GetItem(new GridObjectTypeData(itemKind, typeId), coordinate, cellSize);
        GridItemObject GetItem(ItemType type, Vector2Int gridPos, Vector2 size) => GetItem(GridItemKind.Regular, (int)type, gridPos, size);
        GridItemObject GetItem(BoosterType type, Vector2Int gridPos, Vector2 size) => GetItem(GridItemKind.Booster, (int)type, gridPos, size);
        GridItemObject GetItem(ObstacleType type, Vector2Int gridPos, Vector2 size) => GetItem(GridItemKind.Obstacle, (int)type, gridPos, size);
        void ReleaseItem(GridItemObject item);
        void ReleaseItem(Transform item);
        void ReleaseItem(GameObject item) => ReleaseItem(item.transform);
        void ReleaseAllItems();
        void RemoveItemPool();
    }
    
    public class GridItemFactory : IGridItemFactory
    {
        private readonly IObjectPoolService _objectPoolService;
        private readonly IItemAnimationFactory _itemAnimationFactory;
        private readonly IItemVisualConfig _itemVisualConfig;

        public GridItemFactory(IObjectPoolService objectPoolService, IItemAnimationFactory itemAnimationFactory, GameConfigContainer gameConfigContainer)
        {
            _objectPoolService = objectPoolService;
            _itemAnimationFactory = itemAnimationFactory;
            _itemVisualConfig = gameConfigContainer.ItemConfigContainer;
        }
        
        public GridItemObject GetItem(GridObjectTypeData typeData, Vector2Int coordinate, Vector2 cellSize)
        {
            var itemObject = _objectPoolService.GetObject<GridItemObject>();
            
            itemObject.ResetState();
            
            var animationData = new ItemAnimationData
            {
                itemObjectReader = itemObject,
                gridObjectType = typeData
            };
            
            var animation = _itemAnimationFactory.Get(animationData);
            
            itemObject.Initialize(typeData.gridItemKind, typeData.typeId, coordinate, cellSize)
                      .ApplyVisual(_itemVisualConfig)
                      .BindAnimation(animation);
   
            return itemObject;
        }

        public void ReleaseItem(GridItemObject item) => _objectPoolService.ReturnObject(item);
        public void ReleaseItem(Transform item) => _objectPoolService.ReturnObject(item);
        public void ReleaseAllItems() => _objectPoolService.ReturnAllObjectsOfType<GridItemObject>();
        public void RemoveItemPool() => _objectPoolService.RemovePool<GridItemObject>();
    }
}