using Core.Item;
using UnityEngine;

namespace Core.Handlers
{
    public struct CellResolveData
    {
        public bool Remove { get; private set; }
        public DamageSource Source { get; private set; }
        
        public int ObstacleDamage { get; private set; }
        public DamageSource ObstacleSource { get; private set; }

        public void MarkRemove(DamageSource source)
        {
            Remove = true;
            Source |= source;
        }
        
        public void ClearRemoveFlag(DamageSource source)
        {
            Remove = false;
            Source &= ~source;
        }

        public void AddObstacleDamage(int damage, DamageSource source)
        {
            ObstacleDamage = Mathf.Max(ObstacleDamage, damage);
            ObstacleSource |= source;
        }
    }
}