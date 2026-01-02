using Core.Item;
using UnityEngine;

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
        public void AddDamage(int damageAmount, DamageSource damageSource)
        {
            DamageAmount += damageAmount;
            DamageSource |= damageSource;
        }
        
        public void ResetDamage()
        {
            DamageAmount = 0;
            DamageSource = DamageSource.None;
        }

        public void Dispose()
        {
            UnMarkRemove();
            ResetDamage();
        }
    }
}