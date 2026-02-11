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

        public BlastFxHandler(IBlastFxFactory blastFxFactory)
        {
            _blastFxFactory = blastFxFactory;
        }
        
        public void PlayBlastParticle(BaseGridObject obj, Vector3 pos)
        {
            switch (obj)
            {
                case ItemObject item:
                    var itemBlast = _blastFxFactory.GetItemBlast(item.ItemType);
                    itemBlast.transform.position = pos;
                    itemBlast.Play(()=> OnBlastFxComplete(itemBlast)).Forget();
                    break;
                case ObstacleObject obstacle:
                    var obstacleBlast = _blastFxFactory.GetObstacleBlast(obstacle.ObstacleType);
                    obstacleBlast.transform.position = pos;
                    obstacleBlast.Play(()=> OnBlastFxComplete(obstacleBlast)).Forget();
                    break;
            }
        }

        private void OnBlastFxComplete(BlastFxView blastFxView)
        {
            _blastFxFactory.ReleaseBlast(blastFxView);
        }
    }
}