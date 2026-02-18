using System;
using UnityEngine;

namespace  Game.Grid.Services
{
    public interface IGridInputService
    {
        event Action<Vector2, Vector2Int> OnInputGet;
        void Enable();
        void Disable();
    }
}