using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace Core.Context
{
    public abstract class ManagedViewContext : MonoBehaviour
    { 
        protected IObjectResolver ObjectResolver { get; private set; }
        
        public async UniTask Construct(IObjectResolver objectResolver)
        {
            ObjectResolver = objectResolver;
        
            await Initialize();
        }

        protected abstract UniTask Initialize();
    }
}