using System;

namespace Core.Interfaces
{
    public interface IDamageable<TDamageSource, out TDamageResult> where TDamageSource : Enum where TDamageResult : Enum
    {
        int Life { get; }
        TDamageSource AllowedDamageSources { get; }
        TDamageResult TakeDamage(int damage, TDamageSource source);
    }
}
