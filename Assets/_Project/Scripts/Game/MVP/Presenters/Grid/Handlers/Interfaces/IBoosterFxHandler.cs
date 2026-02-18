using Cysharp.Threading.Tasks;
using Game.Grid.Contexts;
using Game.Models;
using Game.Views;

namespace Game.Grid.Handlers
{
    public interface IBoosterFxHandler
    {
        UniTask PlayBoosterFxAsync(BoosterActionContext action, IGridModel model, IGridView view, out float animSpeed);
    }
}