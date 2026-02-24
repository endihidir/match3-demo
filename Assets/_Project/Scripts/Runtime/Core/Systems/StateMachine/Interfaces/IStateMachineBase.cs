namespace Core.StateMachineCore
{
    public interface IStateMachineBase
    {
        string CurrentStateID { get; }
        void Update(float dt);
        void FixedUpdate(float dt);
        void LateUpdate(float dt);
    }
}