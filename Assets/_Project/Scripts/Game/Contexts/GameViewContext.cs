using Cysharp.Threading.Tasks;

namespace Core.Context
{
    public class GameViewContext : RootViewContext
    {
        protected override async UniTask Initialize()
        {
            await UniTask.CompletedTask;
        }
    }
}