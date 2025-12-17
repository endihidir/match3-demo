using Core.StateMachineCore;

namespace Core.Handlers
{
    public sealed class ResolveState : StateBase<GridContext>
    {
        public override bool NeedsExitTime => true;

        protected override void OnInit() { }
        protected override bool OnBeforeEnter() => true;

        protected override void OnEnter()
        {
            // TODO: Detect matches.
            // TODO: Blast/Merge + mark empties.
            RequestExit();
        }

        protected override void OnUpdate(float deltaTime) { }
        protected override void OnFixedUpdate(float deltaTime) { }
        protected override void OnLateUpdate(float deltaTime) { }
        protected override void OnExit() { }
    }
}