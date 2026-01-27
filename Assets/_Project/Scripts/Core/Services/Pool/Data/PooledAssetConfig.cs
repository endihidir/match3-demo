using Core.Pool;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(fileName = "PoolAsset", menuName = "Match3/Pool/PoolAsset")]
    public class PooledAssetConfig : ScriptableObject
    {
        [field: SerializeField] public bool IsLazy { get; private set; } = true;
        [field: SerializeField] public int PoolSize {get; private set;}
        
        [field: SerializeField, Required, ValidateInput(nameof(HasPooledObject), "PoolObject must have a PooledObject component")]
        public GameObject PoolObject { get; private set; }
        
        private bool HasPooledObject(GameObject go) => go && go.TryGetComponent<PooledObject>(out _);
    }
}