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
                    var itemBlast = _blastFxFactory.GetItemBlastFx(item.ItemType);
                    PlayBlastAt(itemBlast, pos, parent).Forget();
                    break;
                case ObstacleObject obstacle:
                    var obstacleBlast = _blastFxFactory.GetObstacleBlastFx(obstacle.ObstacleType);
                    PlayBlastAt(obstacleBlast, pos, parent).Forget();
                    break;
            }
        }

        private async UniTask PlayBlastAt(BlastFxView blastFxView, Vector3 pos, Transform parent)
        {
            blastFxView.transform.SetParent(parent, false);
            blastFxView.transform.position = pos;
            await blastFxView.Play();
            OnBlastFxComplete(blastFxView);
        }

        private void OnBlastFxComplete(BlastFxView blastFxView) => _blastFxFactory.ReleaseBlast(blastFxView);
    }
}