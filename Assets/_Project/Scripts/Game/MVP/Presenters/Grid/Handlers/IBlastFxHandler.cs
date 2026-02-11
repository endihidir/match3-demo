using Core.Item;
using UnityEngine;

namespace Core.Handlers
{
    public interface IBlastFxHandler
    {
        void PlayBlastParticle(BaseGridObject obj, Vector3 pos);
    }
}