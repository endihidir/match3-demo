namespace Core.Item
{
    public interface IDamageableItem
    {
        int Life { get; }
        bool IsCollectible { get; }
        ObstacleType ObstacleType { get; }
        DamageSource AllowedDamageSources { get; }
        DamageResult TakeDamage(int damage, DamageSource source);
    }
    
    public enum DamageResult
    {
        Ignored,
        Damaged,
        Destroyed
    }
}