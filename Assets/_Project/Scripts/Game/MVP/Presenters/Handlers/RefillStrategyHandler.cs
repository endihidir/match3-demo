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

        public void Initialize(RefillSettingsSO settingsSo)
        {
            foreach (var refillStrategy in _refillStrategies)
            {
                refillStrategy.SetRefillSettings(settingsSo);
            }
        }

        public IRefillStrategy SelectStrategy(IGridModel model) => _refillStrategies.FirstOrDefault(refillStrategy => refillStrategy.CanRefill(model));
    }
}