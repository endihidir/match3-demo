using System;

namespace Core.Item
{
    public interface IDamageableItem
    {
        public void TakeDamage(int damage, DamageSource source, Action onLifeFinished);
    }
}