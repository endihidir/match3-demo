using NaughtyAttributes;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(fileName = "PoolAsset", menuName = "Match3/Pool/PoolAsset")]
    public class PooledAssetConfig : ScriptableObject
    {
        [field: HideIf(nameof(isUnique))] 
        [field: SerializeField] private int PoolSize {get; set;}
        
        [Required]
        public GameObject poolObject;
        
        public bool isLazy;
        
        public bool isUnique;
        
        public int GetSize()=> isUnique ? 1 : PoolSize;
    }
}