using System;

namespace Game.Grid.Item
{
    [Flags]
    public enum GridDamageSource
    {
        None = 0,
        Match = 1 << 0,
        Booster = 1 << 1,
    }
}