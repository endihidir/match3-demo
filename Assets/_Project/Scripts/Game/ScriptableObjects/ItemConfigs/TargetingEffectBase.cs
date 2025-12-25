using System;
using UnityEngine;

namespace Core.Config
{
    [Serializable]
    public abstract class TargetingActionBase : BoosterActionBase
    { 
        [field: SerializeReference] public BoosterActionBase[] Payloads { get; private set; }
        [field: SerializeField] public TargetSelectionMode SelectionMode { get; private set; }
        [field: SerializeField,] public int DamageAmount { get; private set; }
    }
    
    [Serializable]
    public class FlyAction : TargetingActionBase
    {
        [field: SerializeField] public int StartRange { get; private set; } = 1;
        [field: SerializeField] public int StartDamage { get; private set; } = 1;
        [field: SerializeField] public int DropCount { get; private set; } = 1;
    }
    
    [Serializable]
    public class OrbAction : TargetingActionBase
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