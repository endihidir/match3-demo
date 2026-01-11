using System.Collections.Generic;
using System.Linq;
using Core.Models;

namespace Core.Handlers
{
    public sealed class FillStrategyHandler : IFillStrategyHandler
    {
        private readonly IEnumerable<IFillStrategy> _refillStrategies;
        public FillStrategyHandler(IEnumerable<IFillStrategy> refillStrategies)
        {
            _refillStrategies = refillStrategies;
        }

        public IFillStrategy SelectStrategy(IGridModel model) => _refillStrategies.FirstOrDefault(refillStrategy => refillStrategy.CanRefill(model));
    }
}