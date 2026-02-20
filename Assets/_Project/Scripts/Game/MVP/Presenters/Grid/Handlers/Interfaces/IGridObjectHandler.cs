using Game.Grid.Item;

namespace Game.Grid.Handlers
{
    public interface IGridObjectHandler
    {
        void PopulateGridObjects(GridObjectType[,] gridObjectTypes, int width, int height, out BaseGridObject[,] itemObjects);
        bool TryGetObject<T>(GridObjectType typeData, out T gridObject) where T : BaseGridObject;
        BaseGridObject GetObject(GridObjectType typeData);
    }
}