using Game.Grid.Contexts;
using Core.StateMachineCore;

namespace Game.Grid.States
{
    public sealed class IdleState : StateBase<GridStateContext>
    {
        public IdleState(GridStateContext context) : base(context)
        {
            
        }
    }
}