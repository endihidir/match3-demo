using Game.Configs;
using UnityEngine;

namespace Game.Grid.Contexts
{
    public struct BoosterActionContext
    {
        public Vector2Int OriginCoord { get; private set; }
        public BoosterActionBase BoosterAction { get; private set; }
        public float TriggerDelay { get; private set; }

        public BoosterActionContext(Vector2Int originCoord, BoosterActionBase boosterAction, float triggerDelay = 0f)
        {
            OriginCoord = originCoord;
            BoosterAction = boosterAction;
            TriggerDelay = triggerDelay;
        }
    }
}