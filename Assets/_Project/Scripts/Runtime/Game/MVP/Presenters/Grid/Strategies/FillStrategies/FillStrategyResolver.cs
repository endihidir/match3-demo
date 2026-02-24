using System.Collections.Generic;

namespace Game.Grid.Strategies
{
    public sealed class FillStrategyResolver : IFillStrategyResolver
    {
        private readonly IEnumerable<IFillStrategy> _fillStrategies;
        public FillStrategyResolver(IEnumerable<IFillStrategy> fillStrategies)
        {
            _fillStrategies = fillStrategies;
        }

        public IFillStrategy ResolveStrategy()
        {
            foreach (var fillStrategy in _fillStrategies)
            {
                if (fillStrategy.CanHandle())
                {
                    return fillStrategy;
                }
            }
            
            return null;
        }
    }
}