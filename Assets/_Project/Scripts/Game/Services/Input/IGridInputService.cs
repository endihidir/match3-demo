using System;
using UnityEngine;

namespace Core.Services
{
    public interface IGridInputService
    {
        event Action<Vector2, Vector2Int> OnInputGet;
        void Enable();
        void Disable();
    }
}