using System;
using UnityEngine;

namespace Core.Config
{
    [Serializable]
    public abstract class RocketActionBase : AreaActionBase
    {
        [field: SerializeField] public int LineCount { get; private set; } = 1;
    }
}