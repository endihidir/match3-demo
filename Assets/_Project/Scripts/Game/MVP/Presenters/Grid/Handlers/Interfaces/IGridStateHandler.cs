using Core.StateMachineCore;
using Game.Grid.Contexts;
using UnityEngine;

namespace Game.Grid.Handlers
{
    public interface IGridStateHandler
    {
        public IStateMachine StateMachine { get; }
        public GridStateContext Context { get; }
        bool TryEnqueueInput(Vector2Int sourceCoord, Vector2Int direction);
    }
}