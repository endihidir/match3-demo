using Core.Models;

namespace Core.Handlers
{
    public interface IFillStrategyResolver
    {
        IFillStrategy ResolveStrategy(IGridModel model);
    }
}