using Core.Item;

namespace Core.Handlers
{
    public interface IGridPopulateHandler
    {
        void PopulateGrid(GridObjectType[,] gridObjectTypes, int width, int height, out BaseGridObject[,] itemObjects);
    }
}