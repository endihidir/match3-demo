using System.Linq;
using Core.Pool.Services;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "PoolAsset", menuName = "Match3/Pool/PoolAsset")]
    public class PooledAssetConfig : ScriptableObject
    {
        [field: SerializeField] public bool IsLazy { get; private set; } = true;
        [field: SerializeField] public int PoolSize {get; private set;}
        
        [field: SerializeField, Required, ValidateInput(nameof(HasDerivedPooledObject), "It must have a component derived from PooledObject component")]
        public GameObject PoolObject { get; private set; }
        
        private bool HasDerivedPooledObject(GameObject go)
        {
            if (!go) return true;

            var components = go.GetComponents<PooledObject>();

            return components.Any(comp => comp.GetType() != typeof(PooledObject));
        }
    }
}