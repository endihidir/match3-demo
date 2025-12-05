using Cysharp.Threading.Tasks;

namespace Core.Context
{
    public class HudViewContext : ManagedViewContext
    {
        protected override async UniTask Initialize()
        {
            await UniTask.CompletedTask;
        }
    }
}