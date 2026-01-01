using Core.Utils;

namespace Core.StateMachineCore
{
    public abstract class StateBase<TContext> : IState where TContext : class
    {
        public string StateID { get; protected set; }
        public bool HasInit { get; private set; }
        public bool IsActive { get; private set; }
        protected TContext Context { get; private set; }

        protected bool ShowLogs;

        public virtual bool NeedsExitPermission => false;
        public bool IsExitReady { get; protected set; }

        protected StateBase(TContext context, bool showLogs = true)
        {
            StateID = GetType().Name;
            
            Context = context;

            ShowLogs = showLogs;

            if (HasInit) return;

            HasInit = true;
        }

        public void Enter()
        {
            if (!HasInit)
            {
                if(ShowLogs)
                    EditorLogger.LogError($"The {StateID} state has not init yet! You need to init the state before enter!");
                return;
            }

            if(IsActive) return;

            var canActivate = OnBeforeEnter();

            if (!canActivate) return;
            
            IsExitReady = false;
            
            IsActive = true;

            OnEnter();
        }

        public void Update(float deltaTime)
        {
            if (!IsActive) return;

            OnUpdate(deltaTime);
        }

        public void FixedUpdate(float deltaTime)
        {
            if (!IsActive) return;

            OnFixedUpdate(deltaTime);
        }

        public void LateUpdate(float deltaTime)
        {
            if(!IsActive) return;

            OnLateUpdate(deltaTime);
        }

        public virtual bool Exit()
        {
            if (!IsActive) return false;

            PerformExit();

            return true;
        }

        protected void PerformExit()
        {
            if(!IsActive) return;

            IsActive = false;

            OnExit();
        }

        public virtual void RequestExit() => IsExitReady = true;
        protected virtual bool OnBeforeEnter() => true;
        protected virtual void OnEnter(){}
        protected virtual void OnUpdate(float deltaTime){}
        protected virtual void OnFixedUpdate(float deltaTime){}
        protected virtual void OnLateUpdate(float deltaTime){}
        protected virtual void OnExit(){}
        
        public virtual void ClearAll()
        {
            StateID = string.Empty;
            HasInit = false;
            IsActive = false;
            Context = null;
            ShowLogs = false;
            IsExitReady = false;
        }
    }
}
