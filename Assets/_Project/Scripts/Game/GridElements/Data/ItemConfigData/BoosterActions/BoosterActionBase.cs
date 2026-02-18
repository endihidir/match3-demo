using System;
using UnityEngine;

namespace Game.Configs
{
    [Serializable]
    public abstract class BoosterActionBase
    {
        [field: SerializeField] public int DamageAmount { get; private set; } = 1;
    }
}