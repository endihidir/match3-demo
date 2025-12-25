namespace Core.Item
{
    public enum DamageResult
    {
        Ignored,
        Damaged,
        Destroyed
    }

    public interface IDamageableItem
    {
        public DamageResult TakeDamage(int damage, DamageSource source);
    }
}