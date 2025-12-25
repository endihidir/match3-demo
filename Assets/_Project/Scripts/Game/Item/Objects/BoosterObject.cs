using Core.Config;
using Core.Handlers;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Item
{
    public class BoosterObject : BaseItemObject, ITriggerEffectSource
    {
        [field: SerializeField, ReadOnly] public BoosterType BoosterType { get; private set; }
        [field: SerializeField, ReadOnly] public BoosterEffectBase BoosterEffect { get; private set; }

        protected override void OnInitialize()
        {
            BoosterType = (BoosterType)TypeId;
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            BoosterType = (BoosterType)TypeId;
            UpdateIdentity();
        }

        public override void ApplyData(BaseItemConfigData baseItemConfigData)
        {
            base.ApplyData(baseItemConfigData);

            if (baseItemConfigData is BoosterConfigData boosterConfigData)
            {
                BoosterEffect = boosterConfigData.BoosterEffect;
            }
        }
        public bool TryBuildEffect(Vector2Int origin, out PendingEffect effect)
        {
            if (BoosterEffect == null)
            {
                effect = default;
                return false;
            }

            effect = new PendingEffect(origin, BoosterEffect);

            return true;
        }

        public override string ToString() => $"Type: {BoosterType}";
    }
}