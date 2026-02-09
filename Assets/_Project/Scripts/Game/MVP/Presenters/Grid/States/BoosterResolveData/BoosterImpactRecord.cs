using Core.Item;

namespace Core.Handlers
{
    public struct BoosterImpactRecord
    {
        public bool Remove { get; private set; }
        public bool HasDamage => GridDamageSource > 0;
        public int DamageAmount { get; private set; }
        public GridDamageSource GridDamageSource { get; private set; }
        
        private int _lastDamageGroupId;
        
        public void MarkRemove() => Remove = true;
        public void UnMarkRemove() => Remove = false;
        public void AddDamage(int groupId, int damageAmount, GridDamageSource gridDamageSource)
        {
            if (_lastDamageGroupId == groupId) return;
            _lastDamageGroupId = groupId;
            DamageAmount += damageAmount;
            GridDamageSource |= gridDamageSource;
        }
        
        public void ResetDamage()
        {
            DamageAmount = 0;
            GridDamageSource = GridDamageSource.None;
            _lastDamageGroupId = 0;
        }

        public void Dispose()
        {
            UnMarkRemove();
            ResetDamage();
        }
    }
}