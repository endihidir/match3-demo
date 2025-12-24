using System;
using Core.Config;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Item
{
    public class ObstacleObject : BaseItemObject, IDamageableItem
    {
        [field: SerializeField, ReadOnly] public ObstacleType ObstacleType { get; private set; }
        [field: SerializeField, ReadOnly] public int Life { get; private set; }

        protected override void OnInitialize()
        {
            ObstacleType = (ObstacleType)TypeId;
        }

        public override void ApplyData(BaseItemConfigData baseItemConfigData)
        {
            base.ApplyData(baseItemConfigData);

            if (baseItemConfigData is ObstacleConfigData obstacleConfigData)
            {
                Life = obstacleConfigData.Life;
            }
        }

        public void TakeDamage(int damage, DamageSource source, Action onLifeFinished)
        {
            if (!CanTakeDamageFrom(source)) return;

            if (Life <= 0) return;

            Life -= damage;
            Life = Mathf.Max(0, Life);

            if (Life <= 0)
                onLifeFinished?.Invoke();
        }

        private bool CanTakeDamageFrom(DamageSource source)
        {
            return source switch
            {
                DamageSource.RegularMatch => CanTakeRegularMatchDamage(),
                DamageSource.Booster => true,
                _ => true
            };
        }

        private bool CanTakeRegularMatchDamage()
        {
            return ObstacleType switch
            {
                _ => true
            };
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            ObstacleType = (ObstacleType)TypeId;
            Life = 0;
            UpdateIdentity();
        }

        public override string ToString() => $"Type: {ObstacleType}";
    }

    public interface IDamageableItem
    {
        public void TakeDamage(int damage, DamageSource source, Action onLifeFinished);
    }

    public enum DamageSource
    {
        RegularMatch,
        Booster
    }
}
