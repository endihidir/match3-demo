using Cysharp.Threading.Tasks;

namespace Core.Context
{
    public class GridViewContext : ManagedViewContext
    {
        protected override async UniTask Initialize()
        {
            await UniTask.CompletedTask;
        }
    }
}