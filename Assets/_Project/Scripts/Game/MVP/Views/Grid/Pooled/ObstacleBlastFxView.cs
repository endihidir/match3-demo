using UnityEngine;

namespace Game.Views
{
    public class ObstacleBlastFxView : BlastFxView
    {
        public void Initialize(Sprite[] sprites) => ParticleFxModule.SetTextureSheetSprites(sprites);
    }
}