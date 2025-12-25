using System;
using UnityEngine;

namespace Core.Config
{
    [Serializable]
    public abstract class AreaActionBase : BoosterActionBase
    {
        [field: SerializeField] public int DamageAmount { get; private set; } = 1;
    }
    
    [Serializable]
    public abstract class RocketActionBase : AreaActionBase
    {
        [field: SerializeField] public int LineCount { get; private set; } = 1;
    }

    [Serializable]
    public class RocketHorizontalAction : RocketActionBase
    {
    }

    [Serializable]
    public class RocketVerticalAction : RocketActionBase
    {
        
    }

    [Serializable]
    public class BombAction : AreaActionBase
    {
        [field: SerializeField] public int Radius { get; private set; } = 3;
    }

    [Serializable]
    public class FullGridRemoveAction : AreaActionBase
    {
        
    }
}