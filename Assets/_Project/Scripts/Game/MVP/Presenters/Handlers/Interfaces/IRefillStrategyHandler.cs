using Core.Models;

namespace Core.Handlers
{
    public interface IRefillStrategyHandler
    {
        IRefillStrategy SelectStrategy(IGridModel model);
    }
}