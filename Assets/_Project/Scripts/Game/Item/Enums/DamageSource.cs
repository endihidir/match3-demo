using System;

namespace Core.Item
{
    [Flags]
    public enum DamageSource
    {
        None = 0,
        Match = 1 << 0,
        Booster = 1 << 1,
    }
}