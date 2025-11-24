using System;
using Core.Item;
using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "BoosterConfig", menuName = "Match3/ItemConfigs/BoosterConfig", order = -1)]
    public class BoosterConfig : BaseEnumConfig<BoosterType, BoosterConfigData>
    {
        public Sprite GetSprite(BoosterType itemType) => Configs[itemType].sprite;
        public BoosterEffectConfig GetEffectConfig(BoosterType itemType) => Configs[itemType].boosterEffectConfig;
    }
    
    [Serializable]
    public struct BoosterConfigData
    {
        public Sprite sprite;
        public BoosterEffectConfig boosterEffectConfig;
    }
}