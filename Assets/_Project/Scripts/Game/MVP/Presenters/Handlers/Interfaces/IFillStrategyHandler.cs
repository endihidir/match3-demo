using Core.Models;

namespace Core.Handlers
{
    public interface IFillStrategyHandler
    {
        IFillStrategy SelectStrategy(IGridModel model);
    }
}