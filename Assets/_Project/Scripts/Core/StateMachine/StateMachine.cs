using System;
using System.Collections.Generic;
using System.Linq;
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

        public bool TryGet<T>(out T state) where T : class, IState => TryGet(typeof(T).Name, out state);
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
            foreach (var kv in _states.Where(kv => kv.Value.IsActive)) kv.Value.Exit();
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

        public IStateMachine AddTransition<TFrom, TTo>(TFrom from, TTo to, Func<bool> condition = null, int priority = 0, bool oneShot = false)
            where TFrom : class, IState
            where TTo : class, IState
        {
            return AddTransition((IState)from, (IState)to, condition, priority, oneShot);
        }

        // Main FSM update loop. Runs once per frame.
        // Order is important: the current state is updated first,
        // then pending transitions are resolved, and only then new transitions are evaluated.
        public void Update(float deltaTime)
        {
            // 1) Let the active state run its per-frame logic.
            //    This allows the state to update timers, async flags,
            //    and gameplay conditions that may affect transitions.
            if(CurrentState is IUpdatableState updatableState)
                updatableState.Update(deltaTime);

            // 2) If there is a pending transition waiting for the state to become exit-ready,
            //    check if it is now allowed to leave. If so, apply it and stop processing.
            if (_pending != null)
            {
                if (_pending.From.IsExitReady)
                {
                    ApplyTransition(_pending);
                    _pending = null;
                }
                return;
            }

            // 3) If no state is active, nothing to do.
            if (CurrentState == null) return;

            // 4) Find the best valid transition from the current state.
            //    All transitions whose condition is true are candidates;
            //    the one with the lowest priority value wins.
            ITransition selected = null;
            var bestPriority = int.MaxValue;

            for (int i = 0; i < _transitions.Count; i++)
            {
                var t = _transitions[i];

                // Only transitions that originate from the current (or active) state are considered.
                if (t.From != CurrentState && !t.From.IsActive) continue;

                // The transition’s condition must be true to be considered.
                if (!t.RequestTransition()) continue;

                // Choose the transition with the highest priority (lowest numeric value).
                if (t.Priority < bestPriority)
                {
                    bestPriority = t.Priority;
                    selected = t;
                }
            }

            // 5) If no transition is valid this frame, stay in the current state.
            if (selected == null) return;

            // 6) If the state requires exit permission and is not ready yet,
            //    register this transition as pending and ask the state to prepare for exit.
            //    The actual transition will be applied once IsExitReady becomes true.
            if (selected.From.NeedsExitPermission && !selected.From.IsExitReady)
            {
                _pending = selected;
                selected.From.RequestExit();
                return;
            }

            // 7) If the state is already allowed to exit, apply the transition immediately.
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
        
        public IStateMachine ForceState<T>() where T : class, IState
        {
            if (!TryGet(out T state)) return this;
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