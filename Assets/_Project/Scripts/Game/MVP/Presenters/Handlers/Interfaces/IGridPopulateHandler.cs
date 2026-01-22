using Core.Item;

namespace Core.Handlers
{
    public interface IGridPopulateHandler
    {
        void PopulateGrid(GridObjectType[,] gridObjectTypes, out BaseGridObject[,] itemObjects);
    }
}