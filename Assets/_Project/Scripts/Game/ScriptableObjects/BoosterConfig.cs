using System;
using Core.Item;
using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "BoosterConfig", menuName = "Match3/ItemConfigs/BoosterConfig", order = -1)]
    public class BoosterItemConfig : EnumItemConfig<BoosterType, BoosterConfigData>
    {
      
    }
    
    [Serializable]
    public class BoosterConfigData : BaseItemConfigData
    {
        
    }
}