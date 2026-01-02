
namespace Core.StateMachineCore
{
    public abstract class StateBase<TContext> : IState where TContext : class
    {
        public string StateID { get; protected set; }
        public bool IsActive { get; private set; }
        protected TContext Context { get; private set; }
        public bool IsExitReady { get; protected set; }
        public virtual bool NeedsExitPermission => false;

        protected StateBase(TContext context)
        {
            StateID = GetType().Name;
            
            Context = context;
        }

        public void Enter()
        {
            if(IsActive) return;
            
            IsExitReady = false;
            
            IsActive = true;

            OnEnter();
        }

        public virtual bool Exit()
        {
            if (!IsActive) return false;

            IsActive = false;

            OnExit();

            return true;
        }

        public virtual void RequestExit() => IsExitReady = true;
        protected virtual void OnEnter(){}
        protected virtual void OnExit(){}
        
        public virtual void ClearAll()
        {
            StateID = string.Empty;
            IsActive = false;
            Context = null;
            IsExitReady = false;
        }
    }
}
