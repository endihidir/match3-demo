namespace Core.Item
{
    public interface IDamageableItem
    {
        int Life { get; }
        DamageResult TakeDamage(int damage, DamageSource source);
    }
    
    public enum DamageResult
    {
        Ignored,
        Damaged,
        Destroyed
    }
}