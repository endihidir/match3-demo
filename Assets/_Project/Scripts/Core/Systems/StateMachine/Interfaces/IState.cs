namespace Core.StateMachineCore
{
    public interface IState
    {
        public string StateID { get; }
        public bool IsActive { get; }
        public bool NeedsExitPermission { get; }
        public bool IsExitReady { get; }
        public void RequestExit();
        public void Enter();
        public bool Exit();
        public void ClearAll();
    }

    public interface IUpdatableState
    {
        public void Update(float deltaTime);
    }

    public interface IFixedUpdatableState
    {
        public void FixedUpdate(float deltaTime);
    }
    
    public interface ILateUpdatableState
    {
        public void LateUpdate(float deltaTime);
    }
}