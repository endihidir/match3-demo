using Core.Config;
using Core.Handlers;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Item
{
    public class BoosterObject : BaseGridObject, ITriggerEffectSource
    {
        [field: SerializeField, ReadOnly] public BoosterType BoosterType { get; private set; }
        [field: SerializeReference, ReadOnly] public BoosterActionBase BoosterAction { get; private set; }

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
                BoosterAction = boosterConfigData.BoosterAction;
            }
        }
        public bool TryBuildEffect(Vector2Int origin, out PendingEffect effect)
        {
            if (BoosterAction == null)
            {
                effect = default;
                return false;
            }

            effect = new PendingEffect(origin, BoosterAction);

            return true;
        }

        public override string ToString() => $"Type: {BoosterType}";
    }
}