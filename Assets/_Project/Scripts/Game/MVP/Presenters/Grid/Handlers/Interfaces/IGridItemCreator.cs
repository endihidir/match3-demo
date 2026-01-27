using Core.Item;

namespace Core.Handlers
{
    public interface IGridItemCreator
    {
        void CreateGridItems(GridObjectType[,] gridObjectTypes, int width, int height, out BaseGridObject[,] itemObjects);
    }
}