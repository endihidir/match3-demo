using Core.Config;
using Core.Configs;
using Core.Level;
using Core.Pool;

namespace Core.Item.Factories
{
    public interface IItemAnimationFactory
    {
        IItemAnimation Get(ItemAnimationData data);
        void Release(IItemAnimation item);
        void Clear();
    }
    
    public class ItemAnimationFactory : SinglePool<IItemAnimation, ItemAnimationData>, IItemAnimationFactory
    {
        private readonly ItemConfigContainer _itemConfigContainer;
        public ItemAnimationFactory(GameConfigContainer gameConfigContainer)
        {
            _itemConfigContainer = gameConfigContainer.ItemConfigContainer;
            Initialize();
        }

        protected override IItemAnimation CreateInstance(ItemAnimationData itemAnimationData) => new ItemAnimation(itemAnimationData);

        protected override void OnGet(IItemAnimation item)
        {
            base.OnGet(item);
            var config = GetConfig(item.GridObjectTypeData);
            item.Initialize(config);
        }

        protected override void OnRelease(IItemAnimation item)
        {
            base.OnRelease(item);
            item.Dispose();
        }

        protected override void OnDestroy(IItemAnimation item)
        {
            base.OnDestroy(item);
            item.Dispose();
        }

        private ItemAnimationConfig GetConfig(GridObjectTypeData gridObjectTypeData)
        {
            var animationConfig = _itemConfigContainer.GetAnimationConfig(gridObjectTypeData.gridItemKind, gridObjectTypeData.typeId);
            return animationConfig ? animationConfig : _itemConfigContainer.DefaultAnimationConfigs;
        }
    }

    public struct ItemAnimationData
    {
        public IItemObjectReader itemObjectReader;
        public GridObjectTypeData gridObjectType;
    }
}