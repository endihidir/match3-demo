using Core.Item;

namespace Core.Handlers
{
    public struct BoosterImpactRecord
    {
        public bool Remove { get; private set; }
        public bool HasDamage => DamageSource > 0;
        public int DamageAmount { get; private set; }
        public DamageSource DamageSource { get; private set; }
        
        private int _lastDamageGroupId;
        
        public void MarkRemove() => Remove = true;
        public void UnMarkRemove() => Remove = false;
        public void AddDamage(int groupId, int damageAmount, DamageSource damageSource)
        {
            if (_lastDamageGroupId == groupId) return;
            _lastDamageGroupId = groupId;
            DamageAmount += damageAmount;
            DamageSource |= damageSource;
        }
        
        public void ResetDamage()
        {
            DamageAmount = 0;
            DamageSource = DamageSource.None;
            _lastDamageGroupId = 0;
        }

        public void Dispose()
        {
            UnMarkRemove();
            ResetDamage();
        }
    }
}