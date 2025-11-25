using System;
using System.Collections.Generic;
using Core.Config;

namespace Core.Item.Factories
{
    public interface IItemEffectFactory
    {
        IBaseItemEffect GetEffect(IItemObject owner, GridItemKind kind, int typeId);
        IBaseItemEffect GetEffect(IItemObject owner, ItemType type) => GetEffect(owner, GridItemKind.Regular,(int)type);
        IBaseItemEffect GetEffect(IItemObject owner, ObstacleType type) => GetEffect(owner, GridItemKind.Obstacle,(int)type);
        IBaseItemEffect GetEffect(IItemObject owner, BoosterType type) => GetEffect(owner, GridItemKind.Booster,(int)type);
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

        public IBaseItemEffect GetEffect(IItemObject owner, GridItemKind kind, int typeId)
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
            
            var effectData = new EffectData
            {
                owner = owner,
                itemConfig = config,
                typeId = typeId,
                defaultSettings = _itemConfigContainer.DefaultEffectSettings
            };
            
            effect.Initialize(effectData);
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

    public struct EffectData
    {
        public IItemObject owner;
        public BaseItemConfig itemConfig;
        public int typeId;
        public ItemEffectSettingsConfig defaultSettings;
    }
}