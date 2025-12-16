using NaughtyAttributes;
using UnityEngine;

namespace Core.Item
{
    public class BoosterObject : BaseItemObject
    {
        [field: SerializeField, ReadOnly] public BoosterType BoosterType { get; private set; }

        protected override void OnInitialize(int typeId)
        {
            BoosterType = (BoosterType)typeId;
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            BoosterType = BoosterType.None;
        }
    }
}