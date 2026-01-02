using System;
using System.Collections.Generic;
using Core.Utils;

namespace Core.StateMachineCore
{
    public class StateMachine : IStateMachine
    {
        private readonly Dictionary<string, IState> _states = new();
        private readonly List<ITransition> _transitions = new();
        private ITransition _pending;

        public string CurrentStateID { get; private set; }
        public IState CurrentState { get; private set; }

        public IStateMachine Register(IState state)
        {
            if (state == null || string.IsNullOrEmpty(state.StateID)) return this;
            _states[state.StateID] = state;
            return this;
        }

        public IStateMachine Register<T>(T state) where T : class, IState => Register((IState)state);
        public IStateMachine Register<T>(T[] states) where T : class, IState
        {
            for (var i = 0; i < states.Length; i++)
            {
                var state = states[i];
                Register((IState)state);
            }
            
            return this;
        }

        public bool TryGet(string id, out IState state) => _states.TryGetValue(id, out state);

        public bool TryGet<T>(string id, out T state) where T : class, IState
        {
            state = null;

            if (_states.TryGetValue(id, out var s))
            {
                state = s as T;
                return state != null;
            }

            return false;
        }

        public IStateMachine SetInitialState(string stateID)
        {
            if (!_states.TryGetValue(stateID, out var s))
            {
                EditorLogger.LogError($"[FSM] Initial state '{stateID}' not found.");
                return this;
            }

            return SetInitialState(s);
        }

        public IStateMachine SetInitialState(IState state)
        {
            if (state == null) return this;

            foreach (var kv in _states)
                if (kv.Value.IsActive) kv.Value.Exit();

            state.Enter();
            CurrentState = state;
            CurrentStateID = state.StateID;
            _pending = null;
            return this;
        }

        public IStateMachine SetInitialState<T>(T state) where T : class, IState => SetInitialState((IState)state);

        public IStateMachine AddTransition(string from, string to, Func<bool> condition)
        {
            if (!TryGet(from, out var f) || !TryGet(to, out var t))
            {
                EditorLogger.LogError($"[FSM] Transition {from} -> {to} cannot be created. State not found.");
                return this;
            }

            _transitions.Add(new Transition(f, t, condition));

            return this;
        }

        public IStateMachine AddTransition(IState from, IState to, Func<bool> condition)
        {
            if (from == null || to == null) return this;

            _transitions.Add(new Transition(from, to, condition));

            return this;
        }

        public IStateMachine AddTransition(string from, string to, Func<bool> condition, int priority, bool oneShot)
        {
            if (!TryGet(from, out var f) || !TryGet(to, out var t))
            {
                EditorLogger.LogError($"[FSM] Transition {from} -> {to} cannot be created. State not found.");
                return this;
            }

            _transitions.Add(new Transition(f, t, condition, priority, oneShot));

            return this;
        }

        public IStateMachine AddTransition(IState from, IState to, Func<bool> condition, int priority, bool oneShot)
        {
            if (from == null || to == null) return this;
            _transitions.Add(new Transition(from, to, condition, priority, oneShot));
            return this;
        }

        public IStateMachine AddTransition<TFrom, TTo>(TFrom from, TTo to, Func<bool> condition, int priority = 0, bool oneShot = false)
            where TFrom : class, IState
            where TTo : class, IState
        {
            return AddTransition((IState)from, (IState)to, condition, priority, oneShot);
        }

        public void Update(float deltaTime)
        {
            if(CurrentState is IUpdatableState updatableState)
                updatableState.Update(deltaTime);

            if (_pending != null)
            {
                if (_pending.From.IsExitReady)
                {
                    ApplyTransition(_pending);
                    _pending = null;
                }
                return;
            }

            if (CurrentState == null) return;

            ITransition selected = null;
            var bestPriority = int.MaxValue;

            for (int i = 0; i < _transitions.Count; i++)
            {
                var t = _transitions[i];

                if (t.From != CurrentState && !t.From.IsActive) continue;

                if (!t.RequestTransition()) continue;

                if (t.Priority < bestPriority)
                {
                    bestPriority = t.Priority;
                    selected = t;
                }
            }

            if (selected == null) return;

            if (selected.From.NeedsExitPermission && !selected.From.IsExitReady)
            {
                _pending = selected;
                selected.From.RequestExit();
                return;
            }

            ApplyTransition(selected);
        }

        public void FixedUpdate(float deltaTime)
        {
            if(CurrentState is IFixedUpdatableState fixedUpdatableState)
                fixedUpdatableState.FixedUpdate(deltaTime);
        }

        public void LateUpdate(float deltaTime)
        {
            if(CurrentState is ILateUpdatableState lateUpdatableState)
                lateUpdatableState.LateUpdate(deltaTime);
        }

        public T CurrentAs<T>() where T : class, IState => CurrentState as T;
        
        public IStateMachine ForceState(IState state)
        {
            CurrentState?.Exit();
            CurrentState = state;
            CurrentState?.Enter();
            return this;
        }
        
        public IStateMachine ForceState<T>() where T : IState
        {
            var key = typeof(T).Name;
            if (!_states.TryGetValue(key, out var state)) return this;
            CurrentState?.Exit();
            CurrentState = state;
            CurrentState?.Enter();
            return this;
        }
        
        private void ApplyTransition(ITransition tr)
        {
            if (tr.From == tr.To) return;

            tr.From.Exit();
            tr.To.Enter();

            if (tr.OneShot) _transitions.Remove(tr);

            CurrentState = tr.To;
            CurrentStateID = tr.To.StateID;
        }
    }
}