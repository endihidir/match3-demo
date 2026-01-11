using System.Threading.Tasks;
using Core.Models;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public interface IFillStrategy
    {
        bool CanRefill(IGridModel model);
        IFillStrategy Execute(GridStateContext context);
        UniTask WaitAnimationsAsync();
    }
}