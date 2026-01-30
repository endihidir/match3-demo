namespace Core.Item
{
    public interface IDamageableObstacle
    {
        ObstacleType ObstacleType { get; }
        int Life { get; }
        bool IsCollectible { get; }
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