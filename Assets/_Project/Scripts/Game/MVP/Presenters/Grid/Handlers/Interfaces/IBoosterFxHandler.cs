using Cysharp.Threading.Tasks;
using Game.Grid.Contexts;

namespace Game.Grid.Handlers
{
    public interface IBoosterFxHandler
    {
         UniTask PlayBoosterFxAsync(BoosterActionContext action, out float animSpeed);
    }
}