using System.Collections.Generic;
using Core.Models;

namespace Core.Handlers
{
    public sealed class FillStrategyResolver : IFillStrategyResolver
    {
        private readonly IEnumerable<IFillStrategy> _fillStrategies;
        public FillStrategyResolver(IEnumerable<IFillStrategy> fillStrategies)
        {
            _fillStrategies = fillStrategies;
        }

        public IFillStrategy ResolveStrategy(IGridModel model)
        {
            foreach (var fillStrategy in _fillStrategies)
            {
                if (fillStrategy.CanHandle(model))
                {
                    return fillStrategy;
                }
            }
            
            return null;
        }
    }
}