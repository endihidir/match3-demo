using System;
using UnityEngine;

namespace Core.Configs
{
    [Serializable]
    public abstract class BoosterActionBase
    {
        [field: SerializeField] public int DamageAmount { get; private set; }
    }
}