using Core.Item;

namespace Core.Handlers
{
    public interface IGridItemFactoryHandler
    {
        void PopulateGridWith(GridObjectType[,] gridObjectTypes, out BaseGridObject[,] itemObjects);
    }
}