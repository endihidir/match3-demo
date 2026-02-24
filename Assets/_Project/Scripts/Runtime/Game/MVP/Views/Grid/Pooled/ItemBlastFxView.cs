using UnityEngine;

namespace Game.Views
{
    public class ItemBlastFxView : BlastFxView
    {
        public void Initialize(Color color) => ParticleFxModule.SetStartColor(color);
    }
}