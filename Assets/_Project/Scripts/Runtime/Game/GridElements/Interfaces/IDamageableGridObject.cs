using Core.Interfaces;

namespace Game.Grid.Item
{
    public interface IDamageableGridObject : IDamageable<GridDamageSource, GridDamageResult>
    {
        ObstacleType ObstacleType { get; }
        bool IsCollectible { get; }
    }
}