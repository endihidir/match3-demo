using Core.Interfaces;

namespace Core.Item
{
    public interface IDamageableGridObject : IDamageable<GridDamageSource, GridDamageResult>
    {
        ObstacleType ObstacleType { get; }
        bool IsCollectible { get; }
    }
}