using Core.Config;
using Core.Configs;
using Core.Pool;

namespace Core.Item.Factories
{
    public interface IItemAnimationFactory
    {
        IItemAnimation Get(AnimationEntity data);
        void Release(IItemAnimation item);
        void Clear();
    }
    
    public class ItemAnimationFactory : StackObjectPool<IItemAnimation, AnimationEntity>, IItemAnimationFactory
    {
        private readonly ItemConfigContainer _itemConfigContainer;
        public ItemAnimationFactory(GameConfigContainer gameConfigContainer)
        {
            _itemConfigContainer = gameConfigContainer.ItemConfigContainer;
            Initialize(64);
        }

        protected override IItemAnimation CreateInstance(AnimationEntity data) => new ItemAnimation(data.owner);

        protected override void OnGet(IItemAnimation item)
        {
            base.OnGet(item);
            item.Initialize(_itemConfigContainer.DefaultEffectSettings);
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
    }

    public struct AnimationEntity
    {
        public IItemObjectReader owner;
    }
}