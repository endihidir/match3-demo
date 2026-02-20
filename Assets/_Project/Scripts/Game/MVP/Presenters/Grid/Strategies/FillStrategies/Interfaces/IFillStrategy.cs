using Cysharp.Threading.Tasks;
using Game.Grid.Contexts;
using Game.Models;

namespace Game.Grid.Strategies
{
    public interface IFillStrategy
    {
        bool CanHandle(IGridModel model);
        IFillStrategy Execute();
        UniTask WaitAnimationsAsync();
    }
}