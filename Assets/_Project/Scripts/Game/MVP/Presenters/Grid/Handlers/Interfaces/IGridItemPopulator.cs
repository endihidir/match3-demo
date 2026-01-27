using Core.Item;

namespace Core.Handlers
{
    public interface IGridItemPopulator
    {
        void PopulateGridItems(GridObjectType[,] gridObjectTypes, int width, int height, out BaseGridObject[,] itemObjects);
    }
}