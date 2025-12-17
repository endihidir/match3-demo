using Core.StateMachineCore;

namespace Core.Handlers
{
    public sealed class ShiftRefillState : StateBase<GridContext>
    {
        public override bool NeedsExitTime => true;
        public bool ResolveAgainRequested { get; private set; }

        protected override void OnInit() { }
        protected override bool OnBeforeEnter() => true;

        protected override void OnEnter()
        {
            ResolveAgainRequested = false;

            // TODO: Shift/gravity
            // TODO: Refill
            // TODO: If new matches => ResolveAgainRequested = true;

            RequestExit();
        }

        protected override void OnUpdate(float deltaTime) { }
        protected override void OnFixedUpdate(float deltaTime) { }
        protected override void OnLateUpdate(float deltaTime) { }
        protected override void OnExit() { }
    }
}