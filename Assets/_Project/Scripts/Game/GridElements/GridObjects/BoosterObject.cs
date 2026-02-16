using Core.Configs;
using Core.Handlers;
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

        public override void ApplyData(BaseGridObjectDataSO baseGridObjectDataSo)
        {
            base.ApplyData(baseGridObjectDataSo);

            if (baseGridObjectDataSo is BoosterDataSO boosterConfigData)
            {
                BoosterAction = boosterConfigData.BoosterAction;
            }
        }
        
        public bool TryBuildAction(Vector2Int origin, out BoosterActionContext boosterActionContext)
        {
            if (BoosterAction == null)
            {
                boosterActionContext = default;
                return false;
            }

            boosterActionContext = new BoosterActionContext(origin, BoosterAction);

            return true;
        }

        public override string ToString() => $"Type: {BoosterType}";
    }
}