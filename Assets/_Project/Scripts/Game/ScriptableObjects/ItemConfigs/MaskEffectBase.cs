using System;
using UnityEngine;

namespace Core.Config
{
    [Serializable]
    public abstract class MaskEffectBase : BoosterEffectBase
    {
        [field: SerializeField] public int DamageAmount { get; private set; } = 1;
    }
    
    [Serializable]
    public abstract class RocketEffectBase : MaskEffectBase
    {
        [field: SerializeField] public int LineCount { get; private set; } = 1;
    }

    [Serializable]
    public class RocketHorizontalEffect : RocketEffectBase
    {
    }

    [Serializable]
    public class RocketVerticalEffect : RocketEffectBase
    {
        
    }

    [Serializable]
    public class BombEffect : MaskEffectBase
    {
        [field: SerializeField] public int Radius { get; private set; } = 3;
    }

    [Serializable]
    public class FullGridRemoveEffect : MaskEffectBase
    {
        
    }
}