using System.Collections.Generic;
using System.Linq;
using Core.Models;

namespace Core.Handlers
{
    public sealed class RefillStrategySelectionHandler : IRefillStrategySelectionHandler
    {
        private readonly IEnumerable<IRefillStrategy> _refillStrategies;
        public RefillStrategySelectionHandler(IEnumerable<IRefillStrategy> refillStrategies) => _refillStrategies = refillStrategies;
        public IRefillStrategy SelectStrategy(IGridModel model) => _refillStrategies.FirstOrDefault(refillStrategy => refillStrategy.CanRefill(model));
    }
}