using Core.Configs;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Item
{
    public class ObstacleObject : BaseGridObject, IDamageableGridObject
    {
        [field: SerializeField, ReadOnly] public ObstacleType ObstacleType { get; private set; }
        [field: SerializeField, ReadOnly] public int Life { get; private set; }
        [field: SerializeField, ReadOnly] public GridDamageSource AllowedDamageSources { get; private set; }
        [field: SerializeField, ReadOnly] public bool IsCollectible { get; private set; }
        [field: SerializeField, ReadOnly] public Sprite[] BrokenSprites { get; private set; }

        protected override void OnInitialize()
        {
            ObstacleType = (ObstacleType)TypeId;
        }

        public override void ApplyData(BaseGridObjectDataSO baseGridObjectDataSo)
        {
            base.ApplyData(baseGridObjectDataSo);

            if (baseGridObjectDataSo is ObstacleDataSO obstacleConfigData)
            {
                Life = obstacleConfigData.GetLife();
                IsCollectible = obstacleConfigData.IsCollectible;
                AllowedDamageSources = obstacleConfigData.GridDamageSource;
                BrokenSprites = obstacleConfigData.CrackedSprites;
            }
        }

        public GridDamageResult TakeDamage(int damage, GridDamageSource source)
        {
            if ((AllowedDamageSources & source) == 0 || Life <= 0) return GridDamageResult.Ignored;
            Life -= damage;
            Life = Mathf.Max(0, Life);
            SetBrokenSprite(Life);
            return Life <= 0 ? GridDamageResult.Destroyed : GridDamageResult.Damaged;
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            ObstacleType = (ObstacleType)TypeId;
            Life = 0;
            UpdateIdentity();
        }

        private void SetBrokenSprite(int remainingLife)
        {
            if (BrokenSprites.Length < 1) return;
            var index = Mathf.Max(0, remainingLife - 1);
            SpriteRenderer.sprite = BrokenSprites[index];
        }

        public override string ToString() => $"Type: {ObstacleType}";
    }
}