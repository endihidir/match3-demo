using Core.Item;
using Core.Item.Factories;
using Core.Utils;

namespace Core.Extensions
{
    public static class GridItemFactoryExtensions
    {
        // ---------- Generic helpers ----------

        public static T GetItem<T>(this IGridItemFactory factory, GridItemKind itemKind, int typeId) where T : BaseGridObject => 
            factory.GetItem<T>(new GridObjectType(itemKind, typeId));

        public static BaseGridObject GetItem(this IGridItemFactory factory, GridObjectType typeData) => typeData.ItemKind switch
        {
            GridItemKind.Regular  => factory.GetItem<ItemObject>(typeData),
            GridItemKind.Booster  => factory.GetItem<BoosterObject>(typeData),
            GridItemKind.Obstacle => factory.GetItem<ObstacleObject>(typeData),
            _ => null
        };

        // ---------- Typed creators ----------

        public static ItemObject GetRegularItem(this IGridItemFactory factory, ItemType itemType)
        {
            var type = new GridObjectType(GridItemKind.Regular, (int)itemType);
            return factory.GetItem<ItemObject>(type);
        }

        public static BoosterObject GetBoosterItem(this IGridItemFactory factory, BoosterType boosterType)
        {
            var type = new GridObjectType(GridItemKind.Booster, (int)boosterType);
            return factory.GetItem<BoosterObject>(type);
        }

        public static ObstacleObject GetObstacleItem(this IGridItemFactory factory, ObstacleType obstacleType)
        {
            var type = new GridObjectType(GridItemKind.Obstacle, (int)obstacleType);
            return factory.GetItem<ObstacleObject>(type);
        }

        // ---------- Random helpers ----------

        public static ItemObject GetRandomItem(this IGridItemFactory factory)
        {
            var randomType = LevelGridRandomUtil.GetRandomEnumValue<ItemType>(1);
            var type = new GridObjectType(GridItemKind.Regular, (int)randomType);
            return factory.GetItem<ItemObject>(type);
        }

        public static ObstacleObject GetRandomObstacle(this IGridItemFactory factory)
        {
            var randomType = LevelGridRandomUtil.GetRandomEnumValue<ObstacleType>(1);
            var type = new GridObjectType(GridItemKind.Obstacle, (int)randomType);
            return factory.GetItem<ObstacleObject>(type);
        }

        public static BoosterObject GetRandomBooster(this IGridItemFactory factory)
        {
            var randomType = LevelGridRandomUtil.GetRandomEnumValue<BoosterType>(1);
            var type = new GridObjectType(GridItemKind.Booster, (int)randomType);
            return factory.GetItem<BoosterObject>(type);
        }

        // ---------- Batch / grid helpers ----------

        public static void PopulateGridItems(this IGridItemFactory factory, GridObjectType[,] gridObjectTypes, int width, int height, out BaseGridObject[,] itemObjects)
        {
            itemObjects = new BaseGridObject[width, height];

            for (int i = 0; i < width * height; i++)
            {
                var coord = GridIndexUtil.ToCoord(i, width);
                var x = coord.x;
                var y = coord.y;

                var typeData = gridObjectTypes[x, y];
                if (typeData is { TypeId: -1 }) continue;

                itemObjects[x, y] = factory.GetItem(typeData);
            }
        }
    }
}