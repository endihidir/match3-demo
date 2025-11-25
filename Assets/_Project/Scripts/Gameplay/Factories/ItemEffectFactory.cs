using System;
using System.Collections.Generic;
using Core.Config;

namespace Core.Item.Factories
{
    public interface IItemEffectFactory
    {
        IBaseItemEffect GetEffect(GridItemKind kind, int typeId, IItemObject owner);

        IBaseItemEffect GetEffect(ItemType type, IItemObject owner) => GetEffect(GridItemKind.Regular,(int)type, owner);
        IBaseItemEffect GetEffect(ObstacleType type, IItemObject owner) => GetEffect(GridItemKind.Obstacle,(int)type, owner);
        IBaseItemEffect GetEffect(BoosterType type, IItemObject owner) => GetEffect(GridItemKind.Booster,(int)type, owner);
        void ReleaseEffect(IBaseItemEffect effect);
    }
    
    public class ItemEffectFactory : IItemEffectFactory
    {
        private readonly ItemConfigContainer _itemConfigContainer;
        private readonly Dictionary<Type, Stack<IBaseItemEffect>> _pool = new();

        public ItemEffectFactory(ItemConfigContainer itemConfigContainer)
        {
            _itemConfigContainer = itemConfigContainer;
        }

        public IBaseItemEffect GetEffect(GridItemKind kind, int typeId, IItemObject owner)
        {
            IBaseItemEffect effect = kind switch
            {
                GridItemKind.Regular  => GetFromPool<RegularEffect>(),
                GridItemKind.Booster  => GetFromPool<BoosterEffect>(),
                GridItemKind.Obstacle => GetFromPool<ObstacleEffect>(),
                _  => null
            };

            if (effect == null) return null;

            var config = GetConfigForKind(kind);
            effect.Initialize(owner, config, typeId, _itemConfigContainer.DefaultEffectSettings);
            return effect;
        }

        public void ReleaseEffect(IBaseItemEffect effect)
        {
            if (effect == null) return;

            effect.Dispose();

            var type = effect.GetType();
            
            if (!_pool.TryGetValue(type, out var stack))
            {
                stack = new Stack<IBaseItemEffect>();
                _pool[type] = stack;
            }
            
            stack.Push(effect);
        }

        private T GetFromPool<T>() where T : IBaseItemEffect, new()
        {
            var type = typeof(T);

            if (_pool.TryGetValue(type, out var stack) && stack.Count > 0) 
                return (T)stack.Pop();

            return new T();
        }

        private BaseItemConfig GetConfigForKind(GridItemKind kind) => kind switch
        {
            GridItemKind.Regular  => _itemConfigContainer.GetConfig<ItemConfig>(),
            GridItemKind.Booster  => _itemConfigContainer.GetConfig<BoosterItemConfig>(),
            GridItemKind.Obstacle => _itemConfigContainer.GetConfig<ObstacleItemConfig>(),
            _ => null
        };
    }
}