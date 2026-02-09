using Core.Interfaces;

namespace Core.Item
{
    public interface IDamageableGridObject : IDamageable<GridDamageSource, GridDamageResult>
    {
        bool IsCollectible { get; }
    }
}