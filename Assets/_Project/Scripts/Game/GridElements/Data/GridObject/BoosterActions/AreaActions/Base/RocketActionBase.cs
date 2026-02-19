using System;
using UnityEngine;

namespace Game.Configs
{
    [Serializable]
    public abstract class RocketActionBase : AreaActionBase
    {
        [field: SerializeField] public int LineCount { get; private set; } = 1;
    }
}