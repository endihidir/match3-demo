using System;
using UnityEngine;

namespace Game.Configs
{
    [Serializable]
    public abstract class TargetingActionBase : BoosterActionBase
    { 
        [field: SerializeReference] public BoosterActionBase[] Payloads { get; private set; }
        [field: SerializeField] public TargetSelectionMode SelectionMode { get; private set; }
    }
}