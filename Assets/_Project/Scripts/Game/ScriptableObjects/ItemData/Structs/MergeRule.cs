using System;
using UnityEngine;

namespace Core.Config
{
    [Serializable]
    public struct MergeRule
    {
        [field: SerializeField] public BoosterMergeKey Key { get; private set; }
        [field: SerializeReference] public BoosterActionBase[] Actions { get; private set; }
    }
}