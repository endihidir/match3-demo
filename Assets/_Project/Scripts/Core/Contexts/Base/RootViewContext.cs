using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using VContainer;

namespace Core.Context
{
    public abstract class RootViewContext : MonoBehaviour
    {
        [field: SerializeField] private ManagedViewContext[] ManagedContexts { get; set; }
        protected IObjectResolver ObjectResolver { get; private set; }
        private bool IsPlaying => Application.isPlaying;
        
        [Inject]
        private void Construct(IObjectResolver objectResolver)
        {
            ObjectResolver = objectResolver;

            ConstructAsync().Forget();
        }

        private async UniTask ConstructAsync()
        {
            await Initialize();
            await InitializeChildren();
        }

        protected abstract UniTask Initialize();
        protected virtual async UniTask InitializeChildren()
        {
            foreach (var child in ManagedContexts)
            {
                await child.Construct(ObjectResolver);
            }
        }

        [Button, HideIf(nameof(IsPlaying))]
        private void PullChildContexts() => ManagedContexts = GetComponentsInChildren<ManagedViewContext>();
    }
}