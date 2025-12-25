namespace Core.Item
{
    public interface IDamageableItem
    {
        public DamageResult TakeDamage(int damage, DamageSource source);
    }
    
    public enum DamageResult
    {
        Ignored,
        Damaged,
        Destroyed
    }
}