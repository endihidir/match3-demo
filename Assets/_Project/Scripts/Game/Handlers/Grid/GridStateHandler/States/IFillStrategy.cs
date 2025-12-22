using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public interface IFillStrategy
    {
        UniTask Execute(GridContext context, List<UniTask> tasks, float startDelay = 0f);
    }
}