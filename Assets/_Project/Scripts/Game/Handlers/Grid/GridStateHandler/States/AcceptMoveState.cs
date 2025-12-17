using Core.StateMachineCore;

namespace Core.Handlers
{
    public sealed class AcceptMoveState : StateBase<GridContext>
    {
        protected override void OnInit() { }
        protected override bool OnBeforeEnter() => true;
        protected override void OnEnter() { }
        protected override void OnUpdate(float deltaTime) { }
        protected override void OnFixedUpdate(float deltaTime) { }
        protected override void OnLateUpdate(float deltaTime) { }
        protected override void OnExit() { }
    }
}