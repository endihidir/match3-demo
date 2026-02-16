using UnityEngine;

namespace Core.UI
{
    public class ItemBlastFxView : BlastFxView
    {
        public void Initialize(Color color) => ParticleFxModule.SetStartColor(color);
    }
}