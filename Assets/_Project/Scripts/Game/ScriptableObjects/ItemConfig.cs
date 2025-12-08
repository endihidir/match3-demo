using System;
using Core.Item;
using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "ItemConfig", menuName = "Match3/ItemConfigs/ItemConfig", order = -1)]
    public class ItemConfig : EnumItemConfig<ItemType, ItemConfigData>
    {
        
    }
       
    [Serializable]
    public class ItemConfigData : BaseItemConfigData
    {
        
    }
}