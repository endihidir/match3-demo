using System.Collections.Generic;
using Core.Models;

namespace Core.Handlers
{
    public interface IRefillStrategyHandler
    {
        IRefillStrategy SelectStrategy(IGridModel model);
    }
    
    public sealed class RefillStrategyHandler : IRefillStrategyHandler
    {
        private readonly IEnumerable<IRefillStrategy> _refillStrategies;
     
        public RefillStrategyHandler(IEnumerable<IRefillStrategy> refillStrategies)
        {
            _refillStrategies = refillStrategies;
        }
        
        public IRefillStrategy SelectStrategy(IGridModel model)
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