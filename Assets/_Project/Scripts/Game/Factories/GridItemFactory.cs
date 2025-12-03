using Core.Config;
using Core.Pool;
using UnityEngine;

namespace Core.Item.Factories
{
    public interface IGridItemFactory
    {
        GridItemObject GetItem(GridItemKind itemKind, int typeId, Vector2Int gridPos);
        GridItemObject GetItem(ItemType type, Vector2Int gridPos) => GetItem(GridItemKind.Regular, (int)type, gridPos);
        GridItemObject GetItem(BoosterType type, Vector2Int gridPos) => GetItem(GridItemKind.Booster, (int)type, gridPos);
        GridItemObject GetItem(ObstacleType type, Vector2Int gridPos) => GetItem(GridItemKind.Obstacle, (int)type, gridPos);
        void ReleaseItem(GridItemObject item);
        void ReleaseItem(Transform item);
        void ReleaseItem(GameObject item) => ReleaseItem(item.transform);
        void ReleaseAllItems();
        void RemoveItemPool();
    }
    
    public sealed class GridItemFactory : IGridItemFactory
    {
        private readonly IObjectPoolService _objectPoolService;
        private readonly IItemEffectFactory _itemEffectFactory;
        private readonly IItemSpriteProvider _itemSpriteProvider;
        
        public GridItemFactory(IObjectPoolService objectPoolService, IItemEffectFactory itemEffectFactory, ItemConfigContainer itemConfigContainer)
        {
            _objectPoolService = objectPoolService;
            _itemEffectFactory = itemEffectFactory;
            _itemSpriteProvider = itemConfigContainer;
        }
        
        public GridItemObject GetItem(GridItemKind itemKind, int typeId, Vector2Int gridPos)
        {
            var itemObject = _objectPoolService.GetObject<GridItemObject>();

            var itemBehaviour = itemObject.Behaviour;
    
            if (itemBehaviour == null)
            {
                var behaviourData = new ItemBehaviourData()
                {
                    itemObject = itemObject,
                    itemEffectFactory = _itemEffectFactory,
                    itemSpriteProvider = _itemSpriteProvider
                };
                
                itemBehaviour = new GridItemBehaviour(behaviourData);
                
                itemObject.BindBehaviour(itemBehaviour);
            }
            
            itemBehaviour.Initialize(gridPos, itemKind, typeId);

            return itemObject;
        }

        public void ReleaseItem(GridItemObject item) => _objectPoolService.ReturnObject(item);
        public void ReleaseItem(Transform item) => _objectPoolService.ReturnObject(item);
        public void ReleaseAllItems() => _objectPoolService.ReturnAllObjectsOfType<GridItemObject>();
        public void RemoveItemPool() => _objectPoolService.RemovePool<GridItemObject>();
    }
}