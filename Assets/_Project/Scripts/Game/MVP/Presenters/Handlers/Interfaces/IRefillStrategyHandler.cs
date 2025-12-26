using Core.Config;
using Core.Models;

namespace Core.Handlers
{
    public interface IRefillStrategyHandler
    {
        void Initialize(RefillSettings settings);
        IRefillStrategy SelectStrategy(IGridModel model);
    }
}