using System.Collections.Generic;
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

        public IFillStrategy SelectStrategy(IGridModel model)
        {
            foreach (var refillStrategy in _refillStrategies)
            {
                if (refillStrategy.CanRefill(model))
                {
                    return refillStrategy;
                }
            }
            
            return null;
        }
    }
}