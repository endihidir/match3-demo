using Core.Item;

namespace Core.Handlers
{
    public struct CellImpactMarkData
    {
        public bool Remove { get; private set; }
        public bool HasDamage => DamageSource > 0;
        public int DamageAmount { get; private set; }
        public DamageSource DamageSource { get; private set; }
        
        public void MarkRemove() => Remove = true;
        public void UnMarkRemove() => Remove = false;
        public void MarkDamage(int damageAmount, DamageSource damageSource)
        {
            DamageAmount = damageAmount;
            DamageSource |= damageSource;
        }
        public void UnmarkDamage()
        {
            DamageAmount = 0;
            DamageSource = DamageSource.None;
        }

        public void Dispose()
        {
            UnMarkRemove();
            UnmarkDamage();
        }
    }
}