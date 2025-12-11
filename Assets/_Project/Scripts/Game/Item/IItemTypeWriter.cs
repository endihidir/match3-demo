using Core.Level;

namespace Core.Item
{
    public interface IItemTypeWriter
    {
        void UpdateItemType(GridItemKind itemKind, int typeId);
        void UpdateItemType(ItemType type) => UpdateItemType(GridItemKind.Regular, (int)type);
        void UpdateItemType(BoosterType type) => UpdateItemType(GridItemKind.Booster, (int)type);
        void UpdateItemType(ObstacleType type) => UpdateItemType(GridItemKind.Obstacle, (int)type);
        void UpdateItemType(GridObjectTypeData data) => UpdateItemType(data.gridItemKind, data.typeId);
    }
}