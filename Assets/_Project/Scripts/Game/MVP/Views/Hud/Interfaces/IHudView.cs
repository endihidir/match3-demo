using System;
using UnityEngine;

namespace Core.UI
{
    public interface IHudView
    {
        event Action OnInitialize;
        Transform GoalsHolder { get; }
        Transform GoalFxHolder { get; }
        void Initialize(int goalCount, int moveCount);
        void SetMoveCount(int moveCount);
    }
}