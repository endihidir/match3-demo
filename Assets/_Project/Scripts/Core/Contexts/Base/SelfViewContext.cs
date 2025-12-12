using UnityEngine;
using VContainer;

namespace Core.Context
{
    public abstract class SelfViewContext : MonoBehaviour
    {
        protected IObjectResolver ObjectResolver { get; private set; }

        [Inject]
        private void Construct(IObjectResolver objectResolver)
        {
            ObjectResolver = objectResolver;
            
            Initialize();
        }
        
        protected abstract void Initialize();
    }
}