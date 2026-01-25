using Core.Config;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Item
{
    public class ObstacleObject : BaseGridObject, IDamageableItem
    {
        [field: SerializeField, ReadOnly] public ObstacleType ObstacleType { get; private set; }
        [field: SerializeField, ReadOnly] public int Life { get; private set; }
        [field: SerializeField, ReadOnly] public DamageSource AllowDamageSources { get; private set; }

        protected override void OnInitialize()
        {
            ObstacleType = (ObstacleType)TypeId;
        }

        public override void ApplyData(BaseItemDataSO baseItemDataSo)
        {
            base.ApplyData(baseItemDataSo);

            if (baseItemDataSo is ObstacleDataSO obstacleConfigData)
            {
                Life = obstacleConfigData.Life;
                AllowDamageSources = obstacleConfigData.DamageSource;
            }
        }

        public DamageResult TakeDamage(int damage, DamageSource source)
        {
            if ((AllowDamageSources & source) == 0 || Life <= 0) return DamageResult.Ignored;
            Life -= damage;
            Life = Mathf.Max(0, Life);
            return Life <= 0 ? DamageResult.Destroyed : DamageResult.Damaged;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            ObstacleType = (ObstacleType)TypeId;
            Life = 0;
            UpdateIdentity();
        }

        public override string ToString() => $"Type: {ObstacleType}";
    }
}