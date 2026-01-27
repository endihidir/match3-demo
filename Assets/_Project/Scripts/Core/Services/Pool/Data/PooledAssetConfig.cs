using NaughtyAttributes;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(fileName = "PoolAsset", menuName = "Match3/Pool/PoolAsset")]
    public class PooledAssetConfig : ScriptableObject
    {
        [field: SerializeField] public int PoolSize {get; private set;}
        
        [Required]
        public GameObject poolObject;
        
        public bool isLazy;
    }
}