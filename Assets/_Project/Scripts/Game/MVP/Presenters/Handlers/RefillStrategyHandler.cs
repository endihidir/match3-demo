using System.Collections.Generic;
using System.Linq;
using Core.Config;
using Core.Models;

namespace Core.Handlers
{
    public sealed class RefillStrategyHandler : IRefillStrategyHandler
    {
        private readonly IEnumerable<IRefillStrategy> _refillStrategies;
        public RefillStrategyHandler(IEnumerable<IRefillStrategy> refillStrategies)
        {
            _refillStrategies = refillStrategies;
        }

        public void Initialize(RefillSettings settings)
        {
            foreach (var refillStrategy in _refillStrategies)
            {
                refillStrategy.SetRefillSettings(settings);
            }
        }

        public IRefillStrategy SelectStrategy(IGridModel model) => _refillStrategies.FirstOrDefault(refillStrategy => refillStrategy.CanRefill(model));
    }
}