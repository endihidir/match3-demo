using System;

namespace Core.Item
{
    [Flags]
    public enum DamageSource
    {
        None = 0,
        Item = 1,
        Booster = 2
    }
}