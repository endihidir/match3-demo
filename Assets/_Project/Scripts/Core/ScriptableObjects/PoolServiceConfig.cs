using UnityEngine;

namespace Core.Configs
{
    //[CreateAssetMenu(fileName = "PoolManagerConfig", menuName = "Match3/Pool/PoolManagerConfig", order = 0)]
    public class PoolServiceConfig : ScriptableObject
    { 
        [Header("POOL DATA")] 
        
        public PooledAssetConfig[] poolDataConfigs;
    }
}