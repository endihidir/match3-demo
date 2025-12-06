using System;
using Core.Item;
using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "ObstacleConfig", menuName = "Match3/ItemConfigs/ObstacleConfig", order = -1)]
    public class ObstacleItemConfig : EnumItemConfig<ObstacleType, ObstacleConfigData>
    {
        [field: SerializeField] private ObstacleEffectConfig ObstacleEffectConfig { get; set; }
        public Sprite GetSprite(ObstacleType itemType) => Configs[itemType].sprite;
        public ObstacleEffectConfig GetEffectConfig() => ObstacleEffectConfig;
        public ObstacleEffectConfig GetEffectConfig(ObstacleType itemType) => Configs[itemType].obstacleEffectConfig;
    }
    
    [Serializable]
    public struct ObstacleConfigData
    {
        public Sprite sprite;
        public ObstacleEffectConfig obstacleEffectConfig;
    }
}