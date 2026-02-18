using Game.Models;

namespace Game.Grid.Strategies
{
    public interface IFillStrategyResolver
    {
        IFillStrategy ResolveStrategy(IGridModel model);
    }
}