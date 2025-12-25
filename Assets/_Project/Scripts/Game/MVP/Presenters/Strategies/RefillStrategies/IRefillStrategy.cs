using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public interface IRefillStrategy
    {
        UniTask Execute(GridStateContext stateContext, List<UniTask> tasks, float startDelay = 0f);
    }
}