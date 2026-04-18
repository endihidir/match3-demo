using Core.Interfaces;

namespace Game.Grid.Item
{
    public interface IDamageableGridObject : IDamageable<GridDamageSource, GridDamageResult>
    {
        GridDamageResult ApplyDamageLogic(int damage, GridDamageSource source);
        void ApplyDamageVisual(int remainingLife);
    }
}