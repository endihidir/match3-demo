using Game.Grid.Item;
using UnityEngine;

namespace Game.Grid.Handlers
{
    public interface IBlastFxHandler
    {
        void PlayBlastParticle(BaseGridObject obj, Vector3 pos, Transform parent);
    }
}