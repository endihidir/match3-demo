using Core.Level;

namespace Core.Item
{
    public interface IItemTypeWriter
    {
        void ApplyItem(GridItemKind itemKind, int typeId);
        void ApplyItem(ItemType type) => ApplyItem(GridItemKind.Regular, (int)type);
        void ApplyItem(BoosterType type) => ApplyItem(GridItemKind.Booster, (int)type);
        void ApplyItem(ObstacleType type) => ApplyItem(GridItemKind.Obstacle, (int)type);
        void ApplyItem(GridObjectTypeData data) => ApplyItem(data.gridItemKind, data.typeId);
    }
}