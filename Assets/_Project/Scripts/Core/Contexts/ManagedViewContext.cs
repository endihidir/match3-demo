using Core.MVPContext;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Context
{
    public abstract class ManagedViewContext : MonoBehaviour, IContextOwner
    { 
        public IMVPContext RootContext { get; protected set; }
        public IMVPContext OwnerContext { get; private set; }
        
        public async UniTask Construct(IMVPContext rootContext, IMVPContext ownerContext)
        {
            RootContext = rootContext;
            
            OwnerContext = ownerContext;
        
            await Initialize();
        }

        protected abstract UniTask Initialize();
    }
}