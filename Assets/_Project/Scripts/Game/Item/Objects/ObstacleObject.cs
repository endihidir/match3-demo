using Core.Config;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Item
{
    public class ObstacleObject : BaseItemObject
    {
        [field: SerializeField, ReadOnly] public ObstacleType ObstacleType { get; private set; }

        protected override void OnInitialize(int typeId)
        {
            ObstacleType = (ObstacleType)typeId;
        }

        public override void ApplyData(BaseItemConfigData baseItemConfigData)
        {
            base.ApplyData(baseItemConfigData);
            
            if (baseItemConfigData is ObstacleConfigData obstacleConfigData)
            {
                
            }
        }

        public override string ToString() => $"X: {Coordinate.x}, Y: {Coordinate.y}, Type: {ObstacleType}";
    }
}