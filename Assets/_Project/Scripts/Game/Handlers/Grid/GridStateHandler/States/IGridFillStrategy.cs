using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public interface IGridFillStrategy
    {
        UniTask Execute(GridContext context, List<UniTask> tasks, float startDelay = 0f);
    }
}