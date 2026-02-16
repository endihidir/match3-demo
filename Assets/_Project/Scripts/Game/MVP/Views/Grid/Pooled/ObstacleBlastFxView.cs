using UnityEngine;

namespace Core.UI
{
    public class ObstacleBlastFxView : BlastFxView
    {
        public void Initialize(Sprite[] sprites) => ParticleFxModule.SetTextureSheetSprites(sprites);
    }
}