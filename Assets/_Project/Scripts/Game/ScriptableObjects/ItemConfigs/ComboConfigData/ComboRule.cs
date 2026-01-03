using System;
using UnityEngine;

namespace Core.Config
{
    [Serializable]
    public struct ComboRule
    {
        [field: SerializeField] public BoosterComboKey Key { get; private set; }
        [field: SerializeReference] public BoosterActionBase[] Actions { get; private set; }
    }
}