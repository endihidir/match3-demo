using System;
using UnityEngine;

namespace Game.Configs
{
    [Serializable]
    public class ComboRule
    {
        [field: SerializeField] public BoosterComboKey Key { get; private set; }
        [field: SerializeReference] public BoosterActionBase[] Actions { get; private set; }
    }
}