using Core.UI;
using UnityEngine;

namespace Core.Item.Factories
{
    public interface IBlastFxFactory
    {
        public ItemBlastFxView GetItemBlastFx(ItemType itemType, bool activate = true);
        public ObstacleBlastFxView GetObstacleBlastFx(ObstacleType obstacleType, bool activate = true);
        public void ReleaseBlast(BlastFxView blastFxView);
        public void ReleaseSlot(Transform blastFxView);
        public void ReleaseBlastsByType<T>() where T : BlastFxView;
        public void RemovePoolsByType<T>() where T : BlastFxView;
    }
}