using System;
using Core.Item;
using UnityEngine;

namespace Core.Config
{
    //[CreateAssetMenu(fileName = "ItemConfigContainer", menuName = "Match3/ItemConfigContainer", order = 0)]
    public class ItemConfigContainer : ScriptableObject
    {
        [field: SerializeField] private BaseItemConfig[] ItemConfigs { get; set; }

        public T GetConfig<T>() where T : BaseItemConfig
        {
            foreach (var baseItemConfig in ItemConfigs)
            {
                if (baseItemConfig is T config) 
                {
                    return config;
                }
            }
            
            return null;
        }
        
        public Sprite GetSprite(Enum type) => type switch
        {
            ItemType itemType => GetConfig<ItemConfig>().GetSprite(itemType),
            BoosterType boosterType => GetConfig<BoosterConfig>().GetSprite(boosterType),
            ObstacleType obstacleType => GetConfig<ObstacleConfig>().GetSprite(obstacleType),
            _ => null
        };
    }
}
