using Core.Item;
using UnityEngine;

namespace Core.Handlers
{
    public struct CellResolveData
    {
        public bool Remove;
        public int Damage;
        public DamageSource Source;

        public int ObstacleDamage;
        public DamageSource ObstacleSource;

        public void MarkRemove(DamageSource source)
        {
            Remove = true;
            Source |= source;
        }

        public void AddDamage(int damage, DamageSource source)
        {
            Remove = true;
            Damage = Mathf.Max(Damage, damage);
            Source |= source;
        }

        public void AddObstacleOnlyDamage(int damage, DamageSource source)
        {
            ObstacleDamage = Mathf.Max(ObstacleDamage, damage);
            ObstacleSource |= source;
        }
    }
}