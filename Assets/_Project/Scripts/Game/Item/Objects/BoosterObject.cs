using Core.Config;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Item
{
    public class BoosterObject : BaseGridObject, IBoosterActionSource
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

        public override void ApplyData(BaseItemDataSO baseItemDataSo)
        {
            base.ApplyData(baseItemDataSo);

            if (baseItemDataSo is BoosterDataSO boosterConfigData)
            {
                BoosterAction = boosterConfigData.BoosterAction;
            }
        }
        public bool TryBuildAction(Vector2Int origin, out PendingBoosterAction boosterAction)
        {
            if (BoosterAction == null)
            {
                boosterAction = default;
                return false;
            }

            boosterAction = new PendingBoosterAction(origin, BoosterAction);

            return true;
        }

        public override string ToString() => $"Type: {BoosterType}";
    }
}