using System;
using UnityEngine;

namespace Core.Configs
{
    [Serializable]
    public class OrbAction : TargetingActionBase
    {
        [field: SerializeField] public int PickCount { get; private set; }
    }
}