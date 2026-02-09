using System;

namespace Core.Interfaces
{
    public interface IDamageable<TDamageSource, out TDamageResult> where TDamageSource : Enum where TDamageResult : Enum
    {
        int Life { get; }
        TDamageSource AllowedGridDamageSources { get; }
        TDamageResult TakeDamage(int damage, TDamageSource source);
    }
}
