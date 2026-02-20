using Game.Grid.Item;
using UnityEngine;

namespace Game.Grid.Handlers
{
    public interface ILevelGoalProgressHandler
    {
        void ProgressMove();
        void ProgressGoal(IDamageableGridObject damageableGridObject, Vector2Int coord, Vector2 spriteSize);
    }
}

