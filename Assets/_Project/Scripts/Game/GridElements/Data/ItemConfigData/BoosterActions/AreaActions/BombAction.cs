using System;
using UnityEngine;

namespace Core.Configs
{
    [Serializable]
    public class BombAction : AreaActionBase
    {
        [field: SerializeField] public int Radius { get; private set; } = 3;
    }
}