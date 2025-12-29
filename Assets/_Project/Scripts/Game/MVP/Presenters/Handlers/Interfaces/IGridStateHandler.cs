using Core.StateMachineCore;
using UnityEngine;

namespace Core.Handlers
{
    public interface IGridStateHandler
    {
        public IStateMachine StateMachine { get; }
        public GridStateContext StateContext { get; }
        bool TryEnqueueInput(Vector2Int sourceCoord, Vector2Int direction);
    }
}