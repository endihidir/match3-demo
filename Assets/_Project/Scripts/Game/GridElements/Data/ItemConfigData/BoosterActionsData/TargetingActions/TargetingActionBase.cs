using System;
using UnityEngine;

namespace Core.Configs
{
    [Serializable]
    public abstract class TargetingActionBase : BoosterActionBase
    { 
        [field: SerializeReference] public BoosterActionBase[] Payloads { get; private set; }
        [field: SerializeField] public TargetSelectionMode SelectionMode { get; private set; }
        [field: SerializeField,] public int DamageAmount { get; private set; }
    }
}