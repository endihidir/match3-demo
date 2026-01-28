namespace Core.Item
{
    public interface IDamageableItem
    {
        public ObstacleType ObstacleType { get; }
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