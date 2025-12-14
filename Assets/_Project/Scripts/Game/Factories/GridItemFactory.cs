using Core.Config;
using Core.Configs;
using Core.Pool;
using UnityEngine;

namespace Core.Item.Factories
{
    public interface IGridItemFactory
    {
        GridItemObject GetItem(GridObjectTypeData gridObjectTypeData, Vector2Int coordinate);
        GridItemObject GetItem(GridItemKind itemKind, int typeId, Vector2Int coordinate) => GetItem(new GridObjectTypeData(itemKind, typeId), coordinate);
        GridItemObject GetItem(ItemType type, Vector2Int gridPos) => GetItem(GridItemKind.Regular, (int)type, gridPos);
        GridItemObject GetItem(BoosterType type, Vector2Int gridPos) => GetItem(GridItemKind.Booster, (int)type, gridPos);
        GridItemObject GetItem(ObstacleType type, Vector2Int gridPos) => GetItem(GridItemKind.Obstacle, (int)type, gridPos);
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
        
        public GridItemObject GetItem(GridObjectTypeData gridObjectTypeData, Vector2Int coordinate)
        {
            var itemObject = _objectPoolService.GetObject<GridItemObject>();
            
            itemObject.ResetState();
            
            var animationData = new ItemAnimationData
            {
                itemObjectReader = itemObject,
                gridObjectType = gridObjectTypeData
            };
            
            var animation = _itemAnimationFactory.Get(animationData);
            
            itemObject.Initialize(gridObjectTypeData, coordinate)
                      .ApplyData(_itemVisualConfig)
                      .BindAnimation(animation);
   
            return itemObject;
        }

        public void ReleaseItem(GridItemObject item) => _objectPoolService.ReturnObject(item);
        public void ReleaseItem(Transform item) => _objectPoolService.ReturnObject(item);
        public void ReleaseAllItems() => _objectPoolService.ReturnAllObjectsOfType<GridItemObject>();
        public void RemoveItemPool() => _objectPoolService.RemovePool<GridItemObject>();
    }
}