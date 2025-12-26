using Core.Models;

namespace Core.Handlers
{
    public interface IRefillStrategySelectionHandler
    {
        IRefillStrategy SelectStrategy(IGridModel model);
    }
}