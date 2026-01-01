using System;
using Core.Item;
using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "ObstacleConfig", menuName = "Match3/ItemConfigs/ObstacleConfig", order = -1)]
    public class ObstacleConfigSO : EnumItemConfigSO<ObstacleType, ObstacleDataSO>
    {
        
    }
}