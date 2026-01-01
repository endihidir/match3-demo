using Core.StateMachineCore;
using UnityEngine;

namespace Core.Handlers
{
    public interface IGridStateHandler
    {
        public IStateMachine StateMachine { get; }
        public GridStateContext Context { get; }
        bool TryEnqueueInput(Vector2Int sourceCoord, Vector2Int direction);
    }
}