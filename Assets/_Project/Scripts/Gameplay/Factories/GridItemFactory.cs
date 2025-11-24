using System;
using Core.Config;
using Core.Pool;
using UnityEngine;

namespace Core.Item.Factories
{
    public interface IGridItemFactory
    {
        GridItemObject GetItem(Enum itemType, Vector2Int gridPos);
        void HideItem(GridItemObject popUp);
        void HideAllItems();
        void RemoveItemPool();
    }
    
    public class GridItemFactory : IGridItemFactory
    {
        private readonly IObjectPoolService _objectPoolService;
        private readonly ItemConfigContainer _itemConfigContainer;
        
        public GridItemFactory(IObjectPoolService objectPoolService, ItemConfigContainer itemConfigContainer)
        {
            _objectPoolService = objectPoolService;
            _itemConfigContainer = itemConfigContainer;
        }
        
        public GridItemObject GetItem(Enum itemType, Vector2Int gridPos)
        {
            var itemObject = _objectPoolService.GetObject<GridItemObject>();

            var itemBehaviour = itemObject.Behaviour;
    
            if (itemBehaviour == null)
            {
                itemBehaviour = new GridItemBehaviour(itemObject.transform, itemObject.SpriteRenderer, _itemConfigContainer);
                itemObject.BindBehaviour(itemBehaviour);
            }

            itemBehaviour.ApplyItemType(itemType);
            
            itemBehaviour.SetGridPos(gridPos);
            

            return itemObject;
        }

        public void HideItem(GridItemObject popUp) => _objectPoolService.ReturnToPool(popUp);

        public void HideAllItems() => _objectPoolService.HideAllObjectsOfType<GridItemObject>();

        public void RemoveItemPool() => _objectPoolService.RemovePool<GridItemObject>();
    }
}
