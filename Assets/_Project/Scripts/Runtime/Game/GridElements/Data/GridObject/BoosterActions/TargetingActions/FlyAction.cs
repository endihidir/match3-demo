using System;
using UnityEngine;

namespace Game.Configs
{
    [Serializable]
    public class FlyAction : TargetingActionBase
    {
        [field: SerializeField] public int StartRange { get; private set; } = 1;
        [field: SerializeField] public int StartDamage { get; private set; } = 1;
        [field: SerializeField] public int DropCount { get; private set; } = 1;
    }
}