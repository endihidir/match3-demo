using System;
using UnityEngine;

namespace Core.Config
{
    [Serializable]
    public abstract class TargetingEffectBase : BoosterEffectBase
    { 
        [field: SerializeReference] public BoosterEffectBase[] Payload { get; private set; }
        [field: SerializeField] public TargetSelectionMode SelectionMode { get; private set; }
        [field: SerializeField,] public int DamageAmount { get; private set; }
    }
    
    [Serializable]
    public class FlyEffect : TargetingEffectBase
    {
        [field: SerializeField] public int StartRange { get; private set; } = 1;
        [field: SerializeField] public int StartDamage { get; private set; } = 1;
        [field: SerializeField] public int DropCount { get; private set; } = 1;
    }
    
    [Serializable]
    public class OrbEffect : TargetingEffectBase
    {
        [field: SerializeField] public int PickCount { get; private set; }
    }
    
    public enum TargetSelectionMode
    {
        RandomCells,
        RandomRegularItems,
        RandomObstacle
    }
}