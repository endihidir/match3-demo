using NaughtyAttributes;
using UnityEngine;

namespace Core.Item
{
    public class BoosterObject : BaseItemObject
    {
        [field: SerializeField, ReadOnly] public BoosterType BoosterType { get; private set; }

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
        
        public override string ToString() => $"Type: {BoosterType}";
    }
}