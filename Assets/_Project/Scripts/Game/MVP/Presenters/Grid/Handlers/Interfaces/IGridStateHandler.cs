using System;
using Core.StateMachineCore;
using UnityEngine;

namespace Game.Grid.Handlers
{
    public interface IGridStateHandler
    {
        string CurrentStateID { get; }
        event Action OnDestructionStateComplete;
        bool TryEnqueueInput(Vector2Int sourceCoord, Vector2Int direction);
        void ForceState<T>() where T : class, IState;
    }
}