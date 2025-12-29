namespace Core.StateMachineCore
{
    public interface IState
    {
        public string StateID { get; }
        public bool HasInit { get; }
        public bool IsActive { get; }
        public bool NeedsExitPermission { get; }
        public bool IsExitReady { get; }
        public void RequestExit();
        public void Enter();
        public void Update(float deltaTime);
        public void FixedUpdate(float deltaTime);
        public void LateUpdate(float deltaTime);
        public bool Exit();
        public void ClearAll();
    }
}