using System.Collections.Generic;
using Core.Config;
using Core.Models;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public interface IRefillStrategy
    {
        bool CanRefill(IGridModel model);
        UniTask Execute(GridStateContext stateContext, List<UniTask> tasks, float startDelay = 0f);
        public void SetRefillSettings(RefillSettings refillSettings);
    }
}