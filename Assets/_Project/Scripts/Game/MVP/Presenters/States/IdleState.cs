using Core.StateMachineCore;

namespace Core.Handlers
{
    public class IdleState : StateBase<GridStateContext>
    {
        public IdleState(GridStateContext context, bool showLogs = true) : base(context, showLogs)
        {
            
        }
    }
}