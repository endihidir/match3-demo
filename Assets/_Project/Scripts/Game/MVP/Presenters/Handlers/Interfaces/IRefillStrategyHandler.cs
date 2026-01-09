using Core.Models;

namespace Core.Handlers
{
    public interface IRefillStrategyHandler
    {
        IFillStrategy SelectStrategy(IGridModel model);
    }
}