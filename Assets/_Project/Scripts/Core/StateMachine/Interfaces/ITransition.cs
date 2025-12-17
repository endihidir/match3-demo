namespace Core.StateMachineCore
{
    public interface ITransition
    {
        IState From { get; }
        IState To { get; }
        int Priority { get; }
        bool OneShot { get; }
        bool RequestTransition();
    }
}