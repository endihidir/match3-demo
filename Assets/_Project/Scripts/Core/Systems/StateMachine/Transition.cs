using System;

namespace Core.StateMachineCore
{
    public sealed class Transition : ITransition
    {
        public IState From { get; }
        public IState To { get; }
        public int Priority { get; }
        public bool OneShot { get; }

        private readonly Func<bool> _condition;

        public Transition(IState from, IState to, Func<bool> condition, int priority = 0, bool oneShot = false)
        {
            From = from;
            To = to;
            _condition = condition ?? (() => true);
            Priority = priority;
            OneShot = oneShot;
        }

        public bool RequestTransition() => _condition();
    }
}