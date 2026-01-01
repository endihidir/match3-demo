using System;
using UnityEngine;

namespace Core.Config
{
    [Serializable]
    public class OrbAction : TargetingActionBase
    {
        [field: SerializeField] public int PickCount { get; private set; }
    }
}