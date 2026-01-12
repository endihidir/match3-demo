using Core.Models;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public interface IFillStrategy
    {
        bool CanHandle(IGridModel model);
        IFillStrategy Execute(GridStateContext context);
        UniTask WaitAnimationsAsync();
    }
}