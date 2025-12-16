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

        protected override void OnInitialize(int typeId)
        {
            ObstacleType = (ObstacleType)typeId;
        }

        public override void ApplyData(BaseItemConfigData baseItemConfigData)
        {
            base.ApplyData(baseItemConfigData);
            
            if (baseItemConfigData is ObstacleConfigData obstacleConfigData)
            {
                Life = obstacleConfigData.Life;
            }
        }
        public void TakeDamage(int damage, Action onLifeFinished)
        {
            if (Life <= 0) return;
            Life -= damage;
            Life = Mathf.Max(0, Life);
            onLifeFinished?.Invoke();
        }
        
        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            ObstacleType = ObstacleType.None;
            Life = 0;
        }
    }

    public interface IDamageableItem
    {
        public void TakeDamage(int damage, Action onLifeFinished);
    }
}