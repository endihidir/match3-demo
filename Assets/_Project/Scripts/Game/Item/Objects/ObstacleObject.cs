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
        [field: SerializeField, ReadOnly] public DamageSource DamageSource { get; private set; }

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
                DamageSource = obstacleConfigData.DamageSource;
            }
        }

        public void TakeDamage(int damage, DamageSource source, Action onLifeFinished)
        {
            if (!DamageSource.HasFlag(source)) return;

            if (Life <= 0) return;

            Life -= damage;
            Life = Mathf.Max(0, Life);

            if (Life <= 0)
                onLifeFinished?.Invoke();
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
}