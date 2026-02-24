namespace Game.Grid.Strategies
{
    public interface IFillStrategyResolver
    {
        IFillStrategy ResolveStrategy();
    }
}