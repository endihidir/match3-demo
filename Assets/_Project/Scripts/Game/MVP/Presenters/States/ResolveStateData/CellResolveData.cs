using Core.Item;
using UnityEngine;

namespace Core.Handlers
{
    public struct CellResolveData
    {
        public bool Remove { get; private set; }
        
        public int ObstacleDamage { get; private set; }
        public DamageSource ObstacleSource { get; private set; }

        public void MarkRemove() => Remove = true;
        public void ClearRemove() => Remove = false;

        public void AddDamage(int damage, DamageSource source)
        {
            ObstacleDamage = Mathf.Max(ObstacleDamage, damage);
            ObstacleSource |= source;
        }
        
        public void ClearDamage()
        {
            ObstacleDamage = 0;
            ObstacleSource = DamageSource.None;
        }
    }
}