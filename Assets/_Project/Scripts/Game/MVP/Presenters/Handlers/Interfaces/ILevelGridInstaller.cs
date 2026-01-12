using Core.Item;

namespace Core.Handlers
{
    public interface ILevelGridInstaller
    {
        void PopulateGrid(GridObjectType[,] gridObjectTypes, out BaseGridObject[,] itemObjects);
    }
}