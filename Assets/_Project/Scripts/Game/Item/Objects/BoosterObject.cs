using Core.Config;
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

        public override void ApplyData(BaseItemConfigData baseItemConfigData)
        {
            base.ApplyData(baseItemConfigData);
            
            if (baseItemConfigData is BoosterConfigData boosterConfigData)
            {
                
            }
        }

        public override string ToString() => $"X: {Coordinate.x}, Y: {Coordinate.y}, Type: {BoosterType}";
    }
}