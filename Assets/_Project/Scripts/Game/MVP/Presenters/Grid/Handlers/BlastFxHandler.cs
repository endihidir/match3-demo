using Core.Item;
using Core.Item.Factories;
using Core.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class BlastFxHandler : IBlastFxHandler
    {
        private readonly IBlastFxFactory _blastFxFactory;
        public BlastFxHandler(IBlastFxFactory blastFxFactory) => _blastFxFactory = blastFxFactory;

        public void PlayBlastParticle(BaseGridObject obj, Vector3 pos, Transform parent)
        {
            switch (obj)
            {
                case ItemObject item:
                    var itemBlast = _blastFxFactory.GetItemBlast(item.ItemType);
                    PlayBlastAt(itemBlast, pos, parent);
                    break;
                case ObstacleObject obstacle:
                    var obstacleBlast = _blastFxFactory.GetObstacleBlast(obstacle.ObstacleType);
                    PlayBlastAt(obstacleBlast, pos, parent);
                    break;
            }
        }

        private void PlayBlastAt(BlastFxView blastFxView, Vector3 pos, Transform parent)
        {
            blastFxView.transform.position = pos;
            blastFxView.transform.SetParent(parent, false);
            blastFxView.Play(()=> OnBlastFxComplete(blastFxView)).Forget();
        }

        private void OnBlastFxComplete(BlastFxView blastFxView) => _blastFxFactory.ReleaseBlast(blastFxView);
    }
}