using System;

namespace Core.StateMachineCore
{
    public interface IStateMachine : IStateMachineBase
    {
        IState CurrentState { get; }
        IStateMachine Register(IState state);
        bool TryGet(string id, out IState state);

        IStateMachine Register<T>(T state) where T : class, IState;
        bool TryGet<T>(string id, out T state) where T : class, IState;

        IStateMachine SetInitialState(string stateID);
        IStateMachine SetInitialState(IState state);

        IStateMachine SetInitialState<T>(T state) where T : class, IState;

        IStateMachine AddTransition(string from, string to, Func<bool> condition);
        IStateMachine AddTransition(IState from, IState to, Func<bool> condition);
        IStateMachine AddTransition(string from, string to, Func<bool> condition, int priority, bool oneShot);
        IStateMachine AddTransition(IState from, IState to, Func<bool> condition, int priority, bool oneShot);
        IStateMachine AddTransition<TFrom, TTo>(TFrom from, TTo to, Func<bool> condition, int priority = 0, bool oneShot = false) where TFrom : class, IState where TTo : class, IState;
        T CurrentAs<T>() where T : class, IState;
    }
}