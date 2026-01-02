using Core.Models;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public interface IRefillStrategy
    {
        bool CanRefill(IGridModel model);
        IRefillStrategy Execute(GridStateContext context);
        UniTask WaitAnimationsAsync();
    }
}